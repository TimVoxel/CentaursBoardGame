using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

#nullable enable

public class MockBoard : MonoBehaviour, IBoardHandler
{
    //Mock class used to test without connecting the arduino circuit
    [SerializeField] private float _responseTimeSeconds;
    [SerializeField] private TextMeshProUGUI _encodedRequestText;
    [SerializeField] private ArduinoResponse _mockResponse;

    [SerializeField] private UnityEvent<IArduinoRequest> _onSentRequest;
    [SerializeField] private UnityEvent<ArduinoResponse> _onReceivedResponse;
    
    private WaitForSeconds _responseDelay;

    public event Action<ArduinoResponse>? OnReceivedResponse;

    private void Awake()
    {
        _responseDelay = new WaitForSeconds(_responseTimeSeconds);
    }

    private void OnEnable()
    {
        OnReceivedResponse += ForwardResponseToUnityEvent;
    }

    private void OnDisable()
    {
        OnReceivedResponse -= ForwardResponseToUnityEvent;
    }

    public void RotateRings(IList<BoardRotation> rings)
    {
        LogAndSendRequest(new ArduinoRotateRequest(rings));
        StartCoroutine(nameof(SendResponse));
    }

    public void StopRings(IList<BoardRing> rings)
    {
        LogAndSendRequest(new ArduinoStopRequest(rings));
        StartCoroutine(nameof(SendResponse));
    }

    public void StopAllRings()
        => StopRings(GameFacts.RotatableRings);

    private IEnumerator SendResponse()
    {
        yield return _responseDelay;
        OnReceivedResponse?.Invoke(_mockResponse);
    }

    private void LogAndSendRequest(IArduinoRequest request)
    {
        var serialized = request.Serialize();
        Debug.Log(serialized);
        _onSentRequest?.Invoke(request);
    }

    private void ForwardResponseToUnityEvent(ArduinoResponse response)
    {
        _onReceivedResponse?.Invoke(response);
    }
}
