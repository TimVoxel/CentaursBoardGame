using System;

#nullable enable

namespace CentaursBoardGame
{
    public interface IBluetoothCommunicator
    {
        public event Action<string>? StartedConnecting;
        public event Action? OnConnected;
        public event Action? OnDisconnected;
        public event Action? OnScanStarted;
        public event Action<BluetoothDeviceInfo>? OnFoundDevice;
        public event Action<string>? OnSentData;
        public event Action<string>? OnReceivedData;

        public bool IsConnected { get; }

        public void TryConnect(string address);
        public void TryReconnect();
        public void StartScan();

        public void SendBluetoothMessage(string message);
    }
}
