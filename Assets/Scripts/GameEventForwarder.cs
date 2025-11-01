using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
public class GameEventForwarder : MonoBehaviour
{
    [Serializable]
    private struct GameEvent

    {
        [SerializeField] private GameState _state;
        [SerializeField] private UnityEvent<GameState> _event;

        public GameState State => _state;
        public UnityEvent<GameState> Event => _event;
    }

    [SerializeField] private GameEvent[] _events;
    [SerializeField] private Game _game;

    private Dictionary<GameState, UnityEvent<GameState>> _bakedEvents;

    private void Awake()
    {
        _bakedEvents = new Dictionary<GameState, UnityEvent<GameState>>(_events.Length);

        foreach (var gameEvent in _events)
        {
            if (!_bakedEvents.ContainsKey(gameEvent.State))
            {
                _bakedEvents.Add(gameEvent.State, gameEvent.Event);
            }
            else
            {
                Debug.LogWarning($"Duplicate game event for state {gameEvent.State} in {nameof(GameEventForwarder)} on {gameObject.name}");
            }
        }
    }

    private void OnEnable()
    {
        _game.GameStateChanged += ForwardGameState;
    }

    private void OnDisable()
    {
        _game.GameStateChanged -= ForwardGameState;
    }

    private void ForwardGameState(GameState state)
    {
        if (_bakedEvents.TryGetValue(state, out var e))
        {
            e.Invoke(state);
        }
    }
}*/