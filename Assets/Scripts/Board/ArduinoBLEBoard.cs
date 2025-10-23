using CentaursBoardGame;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#nullable enable

public class ArduinoBLEBoard : MonoBehaviour, IBoardHandler
{
    [SerializeField] private BLECommunicator _communicator;

    [SerializeField] private UnityEvent<IArduinoRequest>? _onSentRequest;
    [SerializeField] private UnityEvent<ArduinoResponse>? _onReceivedResponse;
    [SerializeField] private UnityEvent? _onTriedToSendRequestWhileNotConnected;

    [Space(30)]
    [SerializeField] private bool _autoReconnectAfterRequestFailure;

    public event Action<ArduinoResponse>? OnReceivedResponse;

    private void OnEnable()
    {
        _communicator.OnReceivedData += OnReceivedData;
        OnReceivedResponse += ForwardResponseToUnityEvent;
    }

    private void OnDisable()
    {
        _communicator.OnReceivedData -= OnReceivedData;
        OnReceivedResponse -= ForwardResponseToUnityEvent;
    }

    public void OnReceivedData(string data)
    {
        try
        {
            var response = ArduinoResponse.Deserialize(data);
            OnReceivedResponse?.Invoke(response);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
        }
    }

    public void RotateRings(IList<BoardRotation> rings)
        => TrySendRequest(new ArduinoRotateRequest(rings));
    
    public void StopRings(IList<BoardRing> rings)
        => TrySendRequest(new ArduinoStopRequest(rings));

    public void StopAllRings()
       => StopRings(GameFacts.RotatableRings);

    private void TrySendRequest(IArduinoRequest request)
    {
        if (_communicator.IsConnected)
        {
            var serialized = request.Serialize();
            _communicator.SendBLEMessage(serialized);
            _onSentRequest?.Invoke(request);
        }
        else
        {
            Debug.LogWarning("The BLE board is not connected, unable to send request");
            _onTriedToSendRequestWhileNotConnected?.Invoke();

            if (_autoReconnectAfterRequestFailure)
            {
                _communicator.TryReconnect();
            }
        }
    }
    private void ForwardResponseToUnityEvent(ArduinoResponse response)
    {
        _onReceivedResponse?.Invoke(response);
    }
}
