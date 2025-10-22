using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Android;

#nullable enable

public struct BoardRotation
{
    public BoardRing Ring { get; }
    public byte SectorCount { get; }

    public BoardRotation(BoardRing ring, byte sectorCount)
    {
        Ring = ring;
        SectorCount = sectorCount;
    }
}

public class ArduinoRotateRequest : IArduinoRequest
{
    private string? _serialized;
    public IList<BoardRotation> Rotations { get; }
    
    public ArduinoRequestType Type => ArduinoRequestType.Rotate;

    public ArduinoRotateRequest(IList<BoardRotation> rotations)
    {
        Rotations = rotations;
    }

    public string Serialize()
    {
        if (_serialized == null)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("r:");

            for (var i = 0; i < Rotations.Count; i++)
            {
                var rotation = Rotations[i];
                var ringIndex = (byte) rotation.Ring;
                var sectorCount = rotation.SectorCount;
                stringBuilder.Append($"{ringIndex} {sectorCount}");

                if (i != Rotations.Count - 1)
                {
                    stringBuilder.Append(",");
                }
            }
            _serialized = stringBuilder.ToString();
        }
        return _serialized;
    }

}

public class ArduinoStopRequest : IArduinoRequest
{
    private string? _serialized;

    public IList<BoardRing> Rings { get; }
    
    public ArduinoRequestType Type => ArduinoRequestType.Stop;

    public ArduinoStopRequest(IList<BoardRing> rings)
    {
        Rings = rings;
    }

    public string Serialize()
    {
        if (_serialized == null)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("s:");

            for (var i = 0; i < Rings.Count; i++)
            {
                var ring = Rings[i];
                stringBuilder.Append(ring);

                if (i != Rings.Count - 1)
                {
                    stringBuilder.Append(",");
                }
            }
            _serialized = stringBuilder.ToString();
        }
        return _serialized;
    }
}

public interface IArduinoRequest 
{
    public ArduinoRequestType Type { get; }
    public string Serialize();
}

[System.Serializable]
public class ArduinoResponse
{
    [SerializeField] private ArduinoResponseStatus _status;
    [SerializeField] private string? _message;

    private string? _serialized;

    public ArduinoResponseStatus Status => _status;
    public string? Message => _message;
   
    public ArduinoResponse(ArduinoResponseStatus status, string? message = null)
    {
        _status = status;
        _message = message;
    }

    public static ArduinoResponse Deserialize(string raw)
    {
        var code = raw.First();
        var type = code switch
        {
            's' => ArduinoResponseStatus.Successful,
            'f' => ArduinoResponseStatus.Failed,
            _ => throw new Exception($"Unexepected arduino response status code: {code}")
        };

        string? message = null;

        if (raw[1] == ':' && raw.Length >= 2)
        {
            message = raw.Substring(2);
        }
        return new ArduinoResponse(type, message);
    }

    public string Serialize()
        => _serialized ??= $"{_status}:{_message}"; 
}

public enum ArduinoRequestType
{ 
    Rotate,
    Stop,
}

public enum ArduinoResponseStatus
{ 
    Successful,
    Failed,
}

[Serializable]
public class BluetoothBoard : MonoBehaviour, IBoardHandler
{
    private const string HC08ServiceUUID = "0000ffe0-0000-1000-8000-00805f9b34fb";
    private const string HC08CharacteristicUUID = "0000ffe1-0000-1000-8000-00805f9b34fb";

    private string hc08Address = "";

    public event Action<ArduinoResponse>? OnReceivedResponse;

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

    public void RotateRings(IList<BoardRotation> rings)
    {
        throw new NotImplementedException();
    }

    public void StopRings(IList<BoardRing> rings)
    {
        throw new NotImplementedException();
    }

    public void StopAllRings()
       => StopRings(GameFacts.RotatableRings);

    public bool IsConnected()
    {
        throw new NotImplementedException();
    }
}
