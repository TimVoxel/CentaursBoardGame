using System;
using System.IO;
using UnityEngine;

#nullable enable

public class Game : MonoBehaviour
{
    [SerializeField] private InterfaceReference<IBoardHandler> _boardHandler;
    [SerializeField] private GameContextBuilder _gameContext;
    
    private BoardAttackController _boardAttackController;

    public IBoardHandler BoardHandler => _boardHandler.Value ?? throw new NullReferenceException("The board handler was not assigned in the game script");
    public BoardAttackController BoardAttackController => _boardAttackController;

    private void Awake()
    {
        Debug.Assert(_boardHandler != null, "Board handler not assigned, cannot start game");

        var gameContext = _gameContext.ToContext();
        _boardAttackController = new BoardAttackController(gameContext);
    }

    private void OnEnable()
    {
        _boardAttackController.OnBoardAttacked += LogBoardAttack;
    }

    private void OnDisable()
    {
        _boardAttackController.OnBoardAttacked -= LogBoardAttack;
    }

    public void LogBoardAttack(BoardAttack attack)
    {
        var textWriter = new StringWriter();
        attack.WriteTo(textWriter);
        Debug.Log($"Board just performed an attack: {textWriter.ToString()}");
    }
}