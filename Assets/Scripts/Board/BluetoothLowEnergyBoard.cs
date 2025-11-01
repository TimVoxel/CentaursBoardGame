using CentaursBoardGame;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#nullable enable

public class BluetoothLowEnergyBoard : MonoBehaviour, IBoardHandler
{
    [SerializeField] private InterfaceReference<IBluetoothCommunicator> _communicator;

    [SerializeField] private UnityEvent<IArduinoRequest>? _onSentRequest;
    [SerializeField] private UnityEvent<ArduinoResponse>? _onReceivedResponse;
    [SerializeField] private UnityEvent? _onTriedToSendRequestWhileNotConnected;

    [Space(30)]
    [SerializeField] private bool _autoReconnectAfterRequestFailure;

    public event Action<ArduinoResponse>? OnReceivedResponse;

    public IBluetoothCommunicator Communicator => _communicator.Value ?? throw new Exception("No communicator set");

    private void OnEnable()
    {
        Communicator.OnReceivedData += OnReceivedData;
        OnReceivedResponse += ForwardResponseToUnityEvent;
    }

    private void OnDisable()
    {
        if (_communicator.Value != null)
        {
            _communicator.Value.OnReceivedData -= OnReceivedData;
        }
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
        var communicator = Communicator;

        if (communicator.IsConnected == true)
        {
            var serialized = request.Serialize();
            communicator.SendBluetoothMessage(serialized);
            _onSentRequest?.Invoke(request);
        }
        else
        {
            Debug.LogWarning("The BLE board is not connected, unable to send request");
            _onTriedToSendRequestWhileNotConnected?.Invoke();

            if (_autoReconnectAfterRequestFailure)
            {
                communicator.TryFindAndConnect();
            }
        }
    }

    private void ForwardResponseToUnityEvent(ArduinoResponse response)
    {
        _onReceivedResponse?.Invoke(response);
    }
}
