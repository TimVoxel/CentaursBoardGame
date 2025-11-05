using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable

namespace CentaursBoardGame
{
    public static class BluetoothAddressCache
    {
        private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private static string CacheLocation => Path.Combine(Application.persistentDataPath, "addresses.txt");

        public static IEnumerable<string> Load()
            => File.Exists(CacheLocation) 
                ? File.ReadAllText(CacheLocation).Split('\n').Select(s => s.Trim())
                : Enumerable.Empty<string>();
    
        public static void Save(IEnumerable<string> addresses)
            => File.WriteAllLines(CacheLocation, addresses);

        public static async Task SaveAsync(HashSet<string> addresses)
        {
            await _fileLock.WaitAsync();

            try
            {
                var location = CacheLocation;

                await using var stream = new FileStream(location,
                                                        FileMode.Create,
                                                        FileAccess.Write,
                                                        FileShare.None,
                                                        bufferSize: 4096,
                                                        useAsync: true);

                await using var writer = new StreamWriter(stream);

                foreach (var address in addresses)
                {
                    await writer.WriteLineAsync(address);
                }
            }
            finally
            {
                _fileLock.Release();
            }
        }
    }

    public class BluetoothLowEnergyCommunicator : MonoBehaviour, IBluetoothCommunicator
    {
        private enum State
        {
            Disconnected,
            Connected,
            Scanning,
            Connecting,
            Subscribing,
            Disconnecting,
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

        private static HashSet<string>? _addresses;
        private string? _pairedAddress;
        
        public event Action<string>? StartedConnecting;
        public event Action<BluetoothDeviceInfo>? OnFoundDevice;
        public event Action? OnConnected;
        public event Action? OnDisconnected;
        public event Action? OnScanStarted;
        public event Action<string>? OnSentData;
        public event Action<string>? OnReceivedData;

        public bool IsConnected => _state == State.Connected;

        private State _state;
        private float _timeout;

        private StringBuilder _inputStream = new StringBuilder();

        private void Awake()
        {
            if (_addresses == null)
            {
                _addresses = BluetoothAddressCache.Load().ToHashSet();
            }

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
            Debug.Log(newState);
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
                    TryConnect(_pairedAddress ?? throw new Exception("Trying to connect with automatically found address without actually finding it"));
                    break;

                case State.Subscribing:
                    TrySubscribe();
                    break;

                case State.Disconnecting:
                    TryDisconnect();
                    break;

                default:
                    throw new Exception($"Unexpected communicator state: {_state}");
            }

            if (_state != State.Connected)
            {
                _state = State.Disconnected;
            }
        }

        public void TryConnect(string targetAddress)
        {
            Debug.Log($"Trying to connect to {_peripheralData.Name}");
            StartedConnecting?.Invoke(targetAddress);

            BluetoothLEHardwareInterface.ConnectToPeripheral(
                name: targetAddress,
                connectAction: null,
                serviceAction: null,
                (address, serviceUUID, characteristicUUID) =>
                {
                    if (serviceUUID == _peripheralData.ServiceUUID && characteristicUUID == _peripheralData.CharacteristicUUID)
                    {
                        _pairedAddress = address;
                        SetState(State.Subscribing, 3f);
                    }
                },
                (address) =>
                {
                    Debug.Log($"Disconnected from {address}");
                    OnDisconnected?.Invoke();
                    SetState(State.Disconnected, 0f);
                });
        }

        public void StartScan()
        {
            Debug.Log($"Scanning for BLE devices...");
            BluetoothLEHardwareInterface.StopScan();

            BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(
                serviceUUIDs: null,
                action: (address, name) =>
                {
                    var device = new BluetoothDeviceInfo(name, address);
                    OnFoundDevice?.Invoke(device);
                }
            );

            OnScanStarted?.Invoke();
        }

        private void TrySubscribe()
        {
            BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(
                _pairedAddress,
                _peripheralData.ServiceUUID,
                _peripheralData.CharacteristicUUID,
                notificationAction: (deviceAddress, characteristic) =>
                {
                    SetState(State.Connected, 0f);
                    Debug.Log($"Connected to {deviceAddress}");
                    OnConnected?.Invoke();
                },
                (deviceAddress, characteristic, data) =>
                {
                    OnBLEPacketReceived(data);
                });
        }

        public void SendBluetoothMessage(string message)
        {
            var bytes = Encoding.ASCII.GetBytes(message);

            BluetoothLEHardwareInterface.WriteCharacteristic(
                _pairedAddress, 
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
        private void TryBindAddress()
        {
            Debug.Log($"Trying to find address of device {_peripheralData.Name}");
            BluetoothLEHardwareInterface.StopScan();

            BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(
                serviceUUIDs: null,
                action: (address, name) =>
                {
                    Debug.Log($"Found Device: {name} ({address})");

                    if (name.Contains(_peripheralData.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        BluetoothLEHardwareInterface.StopScan();
                        _pairedAddress = address;
                        HandleAddress(address);
                        SetState(State.Connecting, 2f);
                    }
                }
            );

            OnScanStarted?.Invoke();
        }

        private void HandleAddress(string address)
        {
            if (_addresses?.Contains(address) == false)
            {
                Debug.Log($"New address registered: {address}");
                _addresses.Add(address);
                _ = CacheAddresses();
            }
        }

        private Task CacheAddresses()
            => BluetoothAddressCache.SaveAsync(_addresses ?? throw new Exception("Unloaded"));

        public void TryFindAndConnect()
        {
            if (_pairedAddress == null)
            {
                SetState(State.Scanning, 2f);
            }
            else 
            {
                SetState(State.Connecting, 2f);
            }
        }
        
        public void Disconnect()
            => SetState(State.Disconnecting, 3f);
          
        private void TryDisconnect()
            => BluetoothLEHardwareInterface.DisconnectPeripheral(_pairedAddress, address =>
            {
                OnDisconnected?.Invoke();
                SetState(State.Disconnected, 0f);
            });

        private void OnBLEMessageReceived(string message)
        {
            Debug.Log($"BLE message received from {_pairedAddress}: \"{message}\"");
            OnReceivedData?.Invoke(message);
        }

        private void OnBLEPacketReceived(byte[] data)
        {
            var str = Encoding.ASCII.GetString(data);
            _inputStream.Append(str);

            if (str.EndsWith('\0'))
            {
                var message = _inputStream.ToString().TrimEnd('\0');
                _inputStream.Clear();
                OnBLEMessageReceived(message);
            }
        }

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
                TryFindAndConnect();
                _reconnectTime = _reconnectRateSeconds;
            }
        }
    }
}