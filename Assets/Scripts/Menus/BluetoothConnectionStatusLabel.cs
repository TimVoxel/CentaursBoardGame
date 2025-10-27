using TMPro;
using UnityEngine;

#nullable enable

namespace CentaursBoardGame
{
    public class BluetoothConnectionStatusLabel : MonoBehaviour
    {
        [SerializeField] private BLECommunicator _handle;
        [SerializeField] private TextMeshProUGUI _text;

        [SerializeField] private string _scanningText = "Scanning for devices...";
        [SerializeField] private string _connectedText = "Connected to device!";
        [SerializeField] private string _connectingText = "Connecting to device...";

        private void OnEnable()
        {
            _handle.OnScanStarted += ShowStartScanningMessage;
            _handle.OnConnected += ShowConnected;
        }

        private void OnDisable()
        {
            _handle.OnScanStarted -= ShowStartScanningMessage;
            _handle.OnConnected -= ShowConnected;
        }
        
        public void ShowStartedConnecting()
        {
            _text.text = _connectingText;
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