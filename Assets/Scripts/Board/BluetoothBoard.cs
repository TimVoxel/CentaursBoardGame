using System;
using System.Text;
using UnityEngine;
using UnityEngine.Android;

[System.Serializable]
public class BluetoothBoard : MonoBehaviour, IBoardHandler
{
    private const string HC08ServiceUUID = "0000ffe0-0000-1000-8000-00805f9b34fb";
    private const string HC08CharacteristicUUID = "0000ffe1-0000-1000-8000-00805f9b34fb";

    private string hc08Address = "";

    public event Action OnStoppedRotating;

    private void Awake()
    {
#if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android)
        {
            // For Android 12+ (API 31 and up)
            if (!Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN"))
                Permission.RequestUserPermission("android.permission.BLUETOOTH_SCAN");

            if (!Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT"))
                Permission.RequestUserPermission("android.permission.BLUETOOTH_CONNECT");

            // For older Androids (BLE scan needs location)
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
                Permission.RequestUserPermission(Permission.FineLocation);
        }
#endif
    }

    private void Start()
    {
        BluetoothLEHardwareInterface.Initialize(
            asCentral: true,
            asPeripheral: false, () =>
            {
                Debug.Log("BLE Initialized. Scanning...");
                StartScan();
            }, (error) =>
            {
                Debug.LogError("BLE Init Error: " + error);
            });
    }

    private void StartScan()
    {
        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null,
            (address, name) =>
            {
                Debug.Log($"Found Device: {name} ({address})");
                if (name.Contains("HC-08", StringComparison.OrdinalIgnoreCase))
                {
                    hc08Address = address;
                    BluetoothLEHardwareInterface.StopScan();
                    ConnectToHC08();
                }
            },
            null
        );
    }

    private void ConnectToHC08()
    {
        BluetoothLEHardwareInterface.ConnectToPeripheral(hc08Address,
            connectAction: null,
            serviceAction: null,
            (address, serviceUUID, characteristicUUID) =>
            {
                if (serviceUUID == HC08ServiceUUID && characteristicUUID == HC08CharacteristicUUID)
                {
                    Debug.Log("Connected to HC-08 BLE!");
                    SendBLEMessage("r:2 10");
                }
            },
            (address) =>
            {
                Debug.Log("Disconnected from HC-08.");
            }
        );
    }

    public void SendBLEMessage(string message)
    {
        var bytes = Encoding.ASCII.GetBytes(message);
        BluetoothLEHardwareInterface.WriteCharacteristic(hc08Address, HC08ServiceUUID, HC08CharacteristicUUID, bytes, bytes.Length, true,
            (characteristic) =>
            {
                Debug.Log("Sent: " + message);
            });
    }

    void OnApplicationQuit()
    {
        BluetoothLEHardwareInterface.DeInitialize(() =>
        {
            Debug.Log("BLE Deinitialized");
        });
    }

    public void RotateRings(params (byte ring, byte sectorCount)[] rings)
    {
        throw new NotImplementedException();
    }

    public void StopRings(params byte[] rings)
    {
        throw new NotImplementedException();
    }

    public void StopAllRings()
    {
        throw new NotImplementedException();
    }

    public bool IsConnected()
    {
        throw new NotImplementedException();
    }
}
