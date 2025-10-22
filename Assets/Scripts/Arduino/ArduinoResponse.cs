using System;
using System.Linq;
using UnityEngine;

#nullable enable

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
