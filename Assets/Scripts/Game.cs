using CentaursBoardGame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

#nullable enable

public enum GameState : byte
{ 
    AwaitingPlayerInput,
    ShowingBoardAttack,
    AwaitingBoardCallback,
    TransitionAnimation,
    Victory,
}

public class Game : MonoBehaviour
{
    private enum ContextSource
    {
        File,
        Builder,
        Available,
    }

    [SerializeField] private ContextSource _contextSource;
    [SerializeField] private bool _autoSaveOnAppQuit;
    [SerializeField] private GameContextBuilder _gameContextBuilder;

    [Space(20)]
    [SerializeField] private InterfaceReference<IBoardHandler> _boardHandler;
    [SerializeField] private PlayerMenu _playerMenu;
    
    [Space(20)]
    [SerializeField] private UnityEvent<BoardAttack>? _onAttacked;
    [SerializeField] private UnityEvent<BoardAttack>? _onShowAttack;
    [SerializeField] private UnityEvent? _onHideAttack;
    [SerializeField] private UnityEvent<Player>? _onPlayerFinished;
    [SerializeField] private UnityEvent _onVictory;

    private BoardAttackController _boardAttackController;
    private GameContext _context;
    private GameState _state;

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
    public GameContext Context => _context;

    public event Action<GameState>? GameStateChanged;

    private void Awake()
    {
        _context = _contextSource switch
        {
            ContextSource.File => GameContext.Load() ?? throw new Exception("Unable to load game context from file"),
            ContextSource.Builder => _gameContextBuilder.ToContext(),
            ContextSource.Available => GameContext.Load() ?? _gameContextBuilder.ToContext(),
            _ => throw new Exception($"Unexpected context source: {_contextSource}")
        };
        
        _boardAttackController = new BoardAttackController(_context);
        _playerMenu.BuildFromPlayers(_context.UnfinishedPlayers.ToList());
    }

    private void Start()
    {
        State = GameState.AwaitingPlayerInput;
    }

    private void OnEnable()
    {
        _boardAttackController.OnBoardAttacked += LogBoardAttack;
        _boardAttackController.OnBoardAttacked += EnterTransitionAnimationState;
        _playerMenu.OnCommitedPlayer += RegisterPlayerFinished;
    }

    private void OnDisable()
    {
        _boardAttackController.OnBoardAttacked -= LogBoardAttack;
        _boardAttackController.OnBoardAttacked -= EnterTransitionAnimationState;
        _playerMenu.OnCommitedPlayer -= RegisterPlayerFinished;
    }

    private void OnApplicationQuit()
    {
        if (_autoSaveOnAppQuit)
        {
            _context.Save();
        }
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

    private void EnterTransitionAnimationState(BoardAttack attack)
    {
        _onAttacked?.Invoke(attack);
        State = GameState.TransitionAnimation;
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
        Debug.Log($"Board just performed an attack: {textWriter}");
    }

    private void RegisterPlayerFinished(Player player)
    {
        _context.RegisterFinished(player);
        _onPlayerFinished?.Invoke(player);

        if (_context.AllFinished)
        {
            State = GameState.Victory;
            _onVictory?.Invoke();
        }        
    }
}