using System;
using System.Collections;
using System.Linq;
using UnityEngine;

#nullable enable

namespace CentaursBoardGame
{
    public class MockBluetoothCommunicator : MonoBehaviour, IBluetoothCommunicator
    {
        [SerializeField] private float _mockScanDuration = 1f;
        [SerializeField] private BluetoothDeviceInfo[] _mockFoundDevices = new BluetoothDeviceInfo[]
        {
            new BluetoothDeviceInfo("Mock Device 1", "00:11:22:33:44:55"),
            new BluetoothDeviceInfo("Mock Device 2", "66:77:88:99:AA:BB"),
            new BluetoothDeviceInfo("Mock Device 3", "CC:DD:EE:FF:00:11"),
        };
        [SerializeField] private float _responseDelaySeconds = 1f;
        [SerializeField] private bool _sendResponseAfterMessages = true;

        private WaitForSeconds _mockScanDelay;
        private WaitForSeconds _responseDelay;

        public event Action<string>? StartedConnecting;
        public event Action? OnConnected;
        public event Action? OnDisconnected;
        public event Action? OnScanStarted;
        public event Action<BluetoothDeviceInfo>? OnFoundDevice;
        public event Action<string>? OnSentData;
        public event Action<string>? OnReceivedData;
        
        public bool IsConnected { get; private set; }

        private void Awake()
        {
            _mockScanDelay = new WaitForSeconds(_mockScanDuration);
            _responseDelay = new WaitForSeconds(_responseDelaySeconds);
        }

        public void StartScan()
        {
            Debug.Log("Starting mock scan...");
            OnScanStarted?.Invoke();
            StartCoroutine(ReturnMockFoundDevices());
        }

        public void TryConnect(string address)
        {
            StartedConnecting?.Invoke(address);

            if (IsConnected)
            {
                Debug.Log("Already connected");
                return;
            }
            else
            {
                StartCoroutine(ConnectAfterDelay(address));
            }
        }

        public void SendBluetoothMessage(string message)
        {
            Debug.Log($"Sent mock message: {message}");
            OnSentData?.Invoke(message);

            if (_sendResponseAfterMessages)
            {
                StartCoroutine(SendResponseToMessage());
            }
        }

        public void TryReconnect()
            => TryConnect(_mockFoundDevices.First().Address);

        private IEnumerator ReturnMockFoundDevices()
        {
            foreach (var device in _mockFoundDevices)
            {
                yield return _mockScanDelay;
                OnFoundDevice?.Invoke(device);
            }
        }

        private IEnumerator ConnectAfterDelay(string address)
        {
            yield return _responseDelay;
            Debug.Log($"Connected to {address}");
            IsConnected = true;
            OnConnected?.Invoke();
        }

        private IEnumerator SendResponseToMessage()
        {
            yield return _responseDelay;
            OnReceivedData?.Invoke("s:Mock response from device");
        }
    }
}
