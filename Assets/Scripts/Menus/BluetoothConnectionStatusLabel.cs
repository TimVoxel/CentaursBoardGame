using TMPro;
using UnityEngine;

#nullable enable

namespace CentaursBoardGame
{
    public class BluetoothConnectionStatusLabel : MonoBehaviour
    {
        [SerializeField] private InterfaceReference<IBluetoothCommunicator> _communicator;
        [SerializeField] private TextMeshProUGUI _text;

        [SerializeField] private string _scanningText = "Scanning for devices...";
        [SerializeField] private string _connectedText = "Connected to device!";
        [SerializeField] private string _connectingText = "Connecting to device...";

        public IBluetoothCommunicator? Communicator => _communicator.Value;

        private void OnEnable()
        {
            var communicator = Communicator;

            if (communicator == null)
            {
                Debug.LogWarning("No Bluetooth communicator assigned to BluetoothConnectionStatusLabel.");
                return;
            }

            communicator.OnScanStarted += ShowStartScanningMessage;
            communicator.OnConnected += ShowConnected;
            communicator.StartedConnecting += ShowStartedConnecting;
        }

        private void OnDisable()
        {
            var communicator = Communicator;

            if (communicator != null)
            {
                communicator.OnScanStarted -= ShowStartScanningMessage;
                communicator.OnConnected -= ShowConnected;
                communicator.StartedConnecting -= ShowStartedConnecting;
            }
        }
        
        public void ShowStartedConnecting(string address)
        {
            _text.text = $"{_connectingText} (address: {address})";
        }

        private void ShowStartScanningMessage()
        {
            _text.text = _scanningText;
        }

        private void ShowConnected()
        {
            _text.text = _connectedText;
        }
    }
}