using CentaursBoardGame;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

#nullable enable

public class BoardAttackMenu : MonoBehaviour
{
    [SerializeField] private Game _game;
    [SerializeField] private GameObject _confirmPanel;
    [SerializeField] private ArduinoCommunicationDisplay _arduinoCommunicationDisplay;

    [SerializeField] private UnityEvent<BoardAttack>? _onBoardAttacked;

    private void Awake()
    {
        _confirmPanel.SetActive(false);
    }

    private void OnEnable()
    {
        _game.BoardAttackController.OnBoardAttacked += OnBoardAttacked;
        _game.BoardHandler.OnReceivedResponse += ShowArduinoResponseOnDisplay;
    }

    private void OnDisable()
    {
        _game.BoardAttackController.OnBoardAttacked -= OnBoardAttacked;
        _game.BoardHandler.OnReceivedResponse -= ShowArduinoResponseOnDisplay;
    }

    public void OnPerformAttackClicked()
    {
        if (_game.State == GameState.AwaitingPlayerInput)
        {
            ShowConfirmPanel();
        }
    }

    public void OnGoBackClicked()
    {
        if (_game.State == GameState.ShowingBoardAttack)
        {
            _game.EnterAwaitingInputState();
        }
    }

    public void ShowConfirmPanel()
    {
        _confirmPanel.SetActive(true);
    }

    public void HideConfirmPanel()
    {
        _confirmPanel.SetActive(false);
    }

    public void OnAttackConfirmed()
    {
        _game.BoardAttackController.PerformNextAttack();
    }

    private void OnBoardAttacked(BoardAttack attack)
    {
        _onBoardAttacked?.Invoke(attack);
    }

    private void ShowArduinoResponseOnDisplay(ArduinoResponse response)
    {
        _arduinoCommunicationDisplay.ShowResponse(response);
    }
}
