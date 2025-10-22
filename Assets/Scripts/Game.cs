using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

#nullable enable

public enum GameState : byte
{ 
    AwaitingPlayerInput,
    ShowingBoardAttack,
    AwaitingBoardCallback,
    TransitionAnimation
}

public class Game : MonoBehaviour
{
    [SerializeField] private InterfaceReference<IBoardHandler> _boardHandler;
    [SerializeField] private GameContextBuilder _gameContextBuilder;

    private BoardAttackController _boardAttackController;
    private GameContext _context;
    private GameState _state;

    [SerializeField] private UnityEvent<BoardAttack> _onShowAttack;
    [SerializeField] private UnityEvent _onHideAttack;

    public GameState State
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                _state = value;
                GameStateChanged?.Invoke(_state);
            }
        }
    }

    public IBoardHandler BoardHandler => _boardHandler.Value ?? throw new NullReferenceException("The board handler was not assigned in the game script");
    public BoardAttackController BoardAttackController => _boardAttackController;

    public event Action<GameState>? GameStateChanged;

    private void Awake()
    {
        Debug.Assert(_boardHandler != null, "Board handler not assigned, cannot start game");

        _context = _gameContextBuilder.ToContext();
        _boardAttackController = new BoardAttackController(_context);
    }

    private void Start()
    {
        State = GameState.AwaitingPlayerInput;
    }

    private void OnEnable()
    {
        _boardAttackController.OnBoardAttacked += LogBoardAttack;
    }

    private void OnDisable()
    {
        _boardAttackController.OnBoardAttacked -= LogBoardAttack;
    }

    public void EnterShowAttackState()
    {
        var attack = _context.AttackHistory.Last();
        _onShowAttack?.Invoke(attack);
        State = GameState.ShowingBoardAttack;
    }

    public void EnterAwaitingInputState()
    {
        _onHideAttack?.Invoke();
        State = GameState.AwaitingPlayerInput;
    }

    public void TrySendAttackToBoard(BoardAttack attack)
    {
        switch (attack)
        {
            case RotateRingAttack rotateRingAttack:

                var list = new List<BoardRotation>(1) { new BoardRotation(rotateRingAttack.Ring, rotateRingAttack.SectorCount) };
                BoardHandler.RotateRings(list);
                break;

            case ShuffleRingsAttack shuffleRingsAttack:
                BoardHandler.RotateRings(shuffleRingsAttack.Rotations);
                break;

            default:
                break;
        }
    }

    public void LogBoardAttack(BoardAttack attack)
    {
        var textWriter = new StringWriter();
        attack.WriteTo(textWriter);
        Debug.Log($"Board just performed an attack: {textWriter.ToString()}");
    }
}