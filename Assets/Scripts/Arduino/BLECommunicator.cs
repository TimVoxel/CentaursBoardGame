using System;
using System.Text;
using UnityEngine;

#nullable enable

namespace CentaursBoardGame
{
    //TODO: test with arduino circuit
    public class BLECommunicator : MonoBehaviour
    {
        private enum State
        {
            Disconnected,
            Connected,
            Scanning,
            Connecting,
            Subscribing
        }

        //"0000ffe0-0000-1000-8000-00805f9b34fb"
        //0000ffe1-0000-1000-8000-00805f9b34fb

        [SerializeField] private PeripheralData _peripheralData = new PeripheralData(
            "0000ffe0-0000-1000-8000-00805f9b34fb",
            "0000ffe1-0000-1000-8000-00805f9b34fb",
            "HC-08");

        [SerializeField] private bool _tryConnectOnAwake;
        [SerializeField] private bool _enableReconnectRate;
        [SerializeField] private float _reconnectRateSeconds;

        private float _reconnectTime = 0f;

        private static bool _isInitialized;
        private string? _address;

        public event Action<string>? OnSentData;
        public event Action<string>? OnReceivedData;

        public bool IsConnected => _state == State.Connected;

        private State _state;
        private float _timeout;

        private void Awake()
        {
            if (!_isInitialized)
            {
                BluetoothLEHardwareInterface.Initialize(
                    asCentral: true,
                    asPeripheral: false,
                    action: () =>
                    {
                        Debug.Log("BLE Initialized. Scanning...");
                    },
                    errorAction: (error) =>
                    {
                        Debug.LogError("BLE Init Error: " + error);
                    });
            }

            _state = State.Disconnected;

            if (_tryConnectOnAwake)
            {
                SetState(State.Scanning, 2f);
            }

            _reconnectTime = _reconnectRateSeconds;
        }

        private void SetState(State newState, float timeout)
        {
            _state = newState;
            _timeout = timeout;
        }

        private void Update()
        {
            if (_state == State.Disconnected)
            {
                HandleAutomaticReconnect();
                return;
            }

            if (_timeout <= 0f)
            {
                return;
            }

            _timeout -= Time.deltaTime;

            if (_timeout > 0)
            {
                return;
            }
            
            _timeout = 0f;

            //This is needed because all operations in the BLE plugin are async with java-style handling

            switch (_state)
            {
                case State.Connected:
                    break;

                case State.Scanning:
                    TryBindAddress();
                    break;

                case State.Connecting:
                    TryConnect();
                    break;

                case State.Subscribing:
                    TrySubscribe();
                    break;

                default:
                    throw new Exception($"Unexpected communicator state: {_state}");
            }

            if (_state != State.Connected)
            {
                _state = State.Disconnected;
            }
        }

        private void TrySubscribe()
        {
            BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(
                _address,
                _peripheralData.ServiceUUID,
                _peripheralData.CharacteristicUUID,
                notificationAction: null,
                (deviceAddress, characteristic, data) =>
                {
                    OnBLEMessageReceived(data);
                    SetState(State.Connected, 0f);
                });
        }

        public void SendBLEMessage(string message)
        {
            var bytes = Encoding.ASCII.GetBytes(message);

            BluetoothLEHardwareInterface.WriteCharacteristic(
                _address, 
                _peripheralData.ServiceUUID,
                _peripheralData.CharacteristicUUID,
                data: bytes, 
                length: bytes.Length,
                withResponse: true,
                action: (characteristic) =>
                {
                    Debug.Log("Sent: " + message);
                    OnSentData?.Invoke(characteristic);
                });
        }

        public void TryConnect(bool enforceAddressSearch = false)
        {
            Debug.Log($"Trying to connect to {_peripheralData.Name}");
           
            BluetoothLEHardwareInterface.ConnectToPeripheral(_address,
                connectAction: null,
                serviceAction: null,
                (address, serviceUUID, characteristicUUID) =>
                {
                    if (serviceUUID == _peripheralData.ServiceUUID && characteristicUUID == _peripheralData.CharacteristicUUID)
                    {
                        Debug.Log($"Connected to {address}");
                        //_isConnected = true;
                        SetState(State.Subscribing, 1f);

                    }
                },
                (address) =>
                {
                    Debug.Log($"Disconnected from {address}");
                    //_isConnected = false;
                    SetState(State.Disconnected, 0f);
                });
        }

        private void TryBindAddress()
        {
            Debug.Log($"Trying to find address of device {_peripheralData.Name}");

            BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(
                serviceUUIDs: null,
                action: (address, name) =>
                {
                    Debug.Log($"Found Device: {name} ({address})");

                    if (name.Contains(_peripheralData.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        BluetoothLEHardwareInterface.StopScan();
                        _address = address;
                        SetState(State.Connecting, 2f);
                    }
                }
            );
        }

        private void OnBLEMessageReceived(string message)
        {
            Debug.Log($"BLE message received from {_address}: \"{message}\"");
            OnReceivedData?.Invoke(message);
        }

        private void OnBLEMessageReceived(byte[] data)
            => OnBLEMessageReceived(Encoding.ASCII.GetString(data));

        private void OnApplicationQuit()
        {
            if (!Application.isEditor && _isInitialized)
            {
                _isInitialized = false;

                BluetoothLEHardwareInterface.DeInitialize(() =>
                {
                    Debug.Log("BLE Deinitialized");
                });
            }
        }

        private void HandleAutomaticReconnect()
        {
            if (!_enableReconnectRate || IsConnected)
            {
                return;
            }

            _reconnectTime -= Time.deltaTime;

            if (_reconnectTime <= 0f)
            {
                if (_address == null)
                {
                    TryBindAddress();
                }
                    
                _reconnectTime = _reconnectRateSeconds;
            }
        }
    }

}