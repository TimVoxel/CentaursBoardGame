using CentaursBoardGame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

#nullable enable

public enum GameStateKind : byte
{ 
    ConnectingToBoard,
    Startup,
    AwaitingPlayerInput,
    ShowingBoardAttack,
    Victory,
}

public abstract class GameState
{
    protected readonly IStateSwitcher<GameState> _stateSwitcher;

    //This exists so that it is easier to switch states from the editor
    public abstract GameStateKind Kind { get; }
    
    public GameState(IStateSwitcher<GameState> stateSwitcher)
    {
        _stateSwitcher = stateSwitcher;
    }

    public abstract void Enter();
    public abstract void Exit();
}

public class ConnectingToBoardState : GameState
{
    private readonly IBoardHandler _boardHandler;
    private readonly GameObject _panel;

    public override GameStateKind Kind => GameStateKind.ConnectingToBoard;

    public ConnectingToBoardState(IStateSwitcher<GameState> stateSwitcher, IBoardHandler boardHandler, GameObject panel) : base(stateSwitcher) 
    {
        _boardHandler = boardHandler;
        _panel = panel;
    }

    public override void Enter()
    {
        if (_boardHandler is BluetoothLowEnergyBoard bleBoard)
        {
            Debug.Log("Entering ConnectingToBoard state: attempting to connect to BLE board");
            
            var communicator = bleBoard.Communicator;

            communicator.TryFindAndConnect();
            communicator.OnConnected += () =>
            {
                _stateSwitcher.SwitchState<StartupState>();
            };
            _panel.SetActive(true);
        }
        else
        {
            _stateSwitcher.SwitchState<StartupState>();
        }
    }

    public override void Exit()
    {
        _panel.SetActive(false);
    }
}

public class StartupState : GameState
{
    private readonly IBoardHandler _boardHandler;
    private readonly GameObject _panel;
    private readonly GameContext _context;

    public override GameStateKind Kind => GameStateKind.Startup;

    private byte RandomSectorCount => (byte) UnityEngine.Random.Range(1, _context.BoardSectorCount);

    public StartupState(IStateSwitcher<GameState> stateSwitcher,
                        IBoardHandler boardHandler,
                        GameObject panel,
                        GameContext context) : base(stateSwitcher)
    {
        _boardHandler = boardHandler;
        _panel = panel;
        _context = context;
    }

    public override void Enter()
    {
        _panel.SetActive(true);
        _boardHandler.OnReceivedResponse += EnterAwaitingInput;

        ShuffleRings();
    }

    public override void Exit()
    {
        _panel.SetActive(false);
    }

    private void EnterAwaitingInput(ArduinoResponse response)
    {
        if (response.Status == ArduinoResponseStatus.Failed)
        {
            Debug.LogError($"Failed response received from arduino server: {response.Message}");
        }

        _boardHandler.OnReceivedResponse -= EnterAwaitingInput;
        _stateSwitcher.SwitchState<AwaitingPlayerInputState>();
    }    

    private void ShuffleRings()
        => _boardHandler.RotateRings(new BoardRotation[2]
        {
            new BoardRotation(BoardRing.Inner, RandomSectorCount),
            new BoardRotation(BoardRing.Middle, RandomSectorCount)
        });
}

public interface IStateSwitcher<I>
{
    void SwitchState<T>() where T : I;
}

public class AwaitingPlayerInputState : GameState
{
    private readonly GameObject _panel;
    
    public override GameStateKind Kind => GameStateKind.AwaitingPlayerInput;
    public AwaitingPlayerInputState(IStateSwitcher<GameState> stateSwitcher,
                                    GameObject panel) : base(stateSwitcher)
    {
        _panel = panel;
    }

    public override void Enter()
    {
        _panel.SetActive(true);
    }

    public override void Exit()
    {
        _panel.SetActive(false);
    }
}

public class ShowAttackState : GameState
{
    private readonly GameObject _panel;
    private readonly IBoardHandler _boardHandler;
    private readonly BoardAttackMenu _boardAttackMenu;
    private readonly BoardAttackController _boardAttackController;

    public override GameStateKind Kind => GameStateKind.ShowingBoardAttack;
    public ShowAttackState(IStateSwitcher<GameState> stateSwitcher,
                           IBoardHandler boardHandler,
                           BoardAttackMenu boardAttackMenu,
                           BoardAttackController boardAttackController,
                           GameObject panel) : base(stateSwitcher)
    {
        _panel = panel;
        _boardHandler = boardHandler;
        _boardAttackMenu = boardAttackMenu;
        _boardAttackController = boardAttackController;
    }

    public override void Enter()
    {
        _panel.SetActive(true);
        
        var attack = _boardAttackController.RegisterNewAttack();
        LogBoardAttack(attack);

        switch (attack)
        {
            case RotateRingAttack rotateAttack:
                _boardHandler.RotateRings(new BoardRotation[]
                {
                    new BoardRotation(rotateAttack.Ring, rotateAttack.SectorCount)
                });
                LockMenuUntilResponse();
                break;

            case ShuffleRingsAttack shuffleAttack:
                _boardHandler.RotateRings(shuffleAttack.Rotations);
                LockMenuUntilResponse();
                break;

            default:
                break;
        }

        _boardHandler.OnReceivedResponse += UnlockMenu;
    }

    private void LockMenuUntilResponse()
    {
        if (_boardHandler is BluetoothLowEnergyBoard bluetoothLowEnergyBoard && bluetoothLowEnergyBoard.Communicator.IsConnected)
        {
            _boardAttackMenu.Lock();
        }
    }

    public override void Exit()
    {
        _panel.SetActive(false);
        _boardHandler.OnReceivedResponse -= UnlockMenu;
    }

    private void UnlockMenu(ArduinoResponse response)
    {
        _boardAttackMenu.Unlock();
        _boardHandler.OnReceivedResponse -= UnlockMenu;
    }

    private void LogBoardAttack(BoardAttack attack)
    {
        var textWriter = new StringWriter();
        attack.WriteTo(textWriter);
        Debug.Log($"Board just performed an attack: {textWriter}");
    }
}

public class VictoryState : GameState
{
    private readonly GameObject _panel;
    private readonly GameContext _context;

    public override GameStateKind Kind => GameStateKind.Victory;

    public VictoryState(IStateSwitcher<GameState> stateSwitcher,
                        GameObject panel,
                        GameContext context) : base(stateSwitcher)
    {
        _panel = panel;
        _context = context;
    }

    public override void Enter()
    {
        _panel.SetActive(true);
        _context.Save(resetFinished: true);
    }

    public override void Exit()
    {
        _panel.SetActive(false);
    }
}

public class Game : MonoBehaviour, IStateSwitcher<GameState>
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
    [SerializeField] private BoardAttackMenu _boardAttackMenu;
    [SerializeField] private PlayerMenu _playerMenu;

    [Space(20)]
    [SerializeField] private UnityEvent<BoardAttack>? _enteredShowAttackState;
    [SerializeField] private UnityEvent<BoardAttack>? _showAttackTransitionalAnimationEnded;
    [SerializeField] private UnityEvent? _enteredAwaitingInputState;
    [SerializeField] private UnityEvent _enteredVictoryState;

    [Space(20)]
    [SerializeField] private UnityEvent<Player>? _onPlayerFinished;

    [Space(20)]
    [SerializeField] private GameObject _connectingPanel;
    [SerializeField] private GameObject _startupPanel;
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _victoryPanel;

    private BoardAttackController _boardAttackController;
    private GameContext _context;

    private readonly List<GameState> _states = new List<GameState>();
    private GameState? _state;

    public IBoardHandler BoardHandler => _boardHandler.Value ?? throw new NullReferenceException("The board handler was not assigned in the game script");
    public BoardAttackController BoardAttackController => _boardAttackController;
    public GameContext Context => _context;
    public GameStateKind StateKind => _state?.Kind ?? throw new InvalidOperationException("The game has no current state");

    private void Awake()
    {
        _connectingPanel.SetActive(false);
        _startupPanel.SetActive(false);
        _mainPanel.SetActive(false);
        _victoryPanel.SetActive(false);

        _context = _contextSource switch
        {
            ContextSource.File => GameContext.Load() ?? throw new Exception("Unable to load game context from file"),
            ContextSource.Builder => _gameContextBuilder.ToContext(),
            ContextSource.Available => GameContext.Load() ?? _gameContextBuilder.ToContext(),
            _ => throw new Exception($"Unexpected context source: {_contextSource}")
        };

        _boardAttackController = new BoardAttackController(_context);
        _playerMenu.BuildFromPlayers(_context.UnfinishedPlayers.ToList());

        var boardHandler = BoardHandler;

        _states.AddRange(new List<GameState>()
        {
            new ConnectingToBoardState(this, boardHandler, _connectingPanel),
            new StartupState(this, boardHandler, _startupPanel, _context),
            new AwaitingPlayerInputState(this, _mainPanel),
            new ShowAttackState(this, boardHandler, _boardAttackMenu, _boardAttackController, _mainPanel),
            new VictoryState(this, _victoryPanel, _context)
        });
    }

    private void Start()
        => SwitchState<ConnectingToBoardState>();

    private void OnEnable()
    {
        _playerMenu.OnCommitedPlayer += RegisterPlayerFinished;
    }

    private void OnDisable()
    {
        _playerMenu.OnCommitedPlayer -= RegisterPlayerFinished;
    }

    private void OnApplicationQuit()
    {
        if (_autoSaveOnAppQuit && _state?.Kind != GameStateKind.Victory)
        {
            _context.Save();
        }
    }

    public void SwitchState<T>() where T : GameState
    {
        var state = _states.FirstOrDefault(s => s is T);

        if (state != null)
        {
            SetState(state);
        }
        else
        {
            Debug.LogError($"Game has no registered state {nameof(T)}");
        }
    }

    public void SwitchState(GameStateKind kind)
    {
        var state = _states.FirstOrDefault(s => s.Kind == kind);

        if (state != null)
        {
            SetState(state);
        }
        else
        {
            Debug.LogError($"Game has no registered state {kind}");
        }
    }

    public void NotifyTransitionalAnimationEnded(GameStateKind kind)
    {
        if (kind == GameStateKind.ShowingBoardAttack)
        {
            _showAttackTransitionalAnimationEnded?.Invoke(_context.LastAttack);
        }
    }

    private void SetState(GameState state)
    {
        _state?.Exit();
        _state = state;
        _state.Enter();
        ForwardStateChangeToEvent(state.Kind);
    }

    private void ForwardStateChangeToEvent(GameStateKind kind)
    {
        switch (kind)
        {
            case GameStateKind.AwaitingPlayerInput:
                _enteredAwaitingInputState?.Invoke();
                break;

            case GameStateKind.ShowingBoardAttack:
                _enteredShowAttackState?.Invoke(_context.LastAttack);
                break;

            case GameStateKind.Victory:
                _enteredVictoryState?.Invoke();
                break;
        }
    }

    /*{
        var attack = _context.AttackHistory.Last();
        _onShowAttack?.Invoke(attack);
        State = GameState.ShowingBoardAttack;
    }*/

    /*{
        _onHideAttack?.Invoke();
        State = GameState.AwaitingPlayerInput;
    }*/

    /*
    private void EnterAttackState(BoardAttack attack)
    {
        _onAttacked?.Invoke(attack);
        State = GameState.TransitionAnimation;
    }*/

    private void RegisterPlayerFinished(Player player)
    {
        _context.RegisterFinished(player);
        _onPlayerFinished?.Invoke(player);

        if (_context.AllFinished)
        {
            SwitchState<VictoryState>();
        }        
    }
}