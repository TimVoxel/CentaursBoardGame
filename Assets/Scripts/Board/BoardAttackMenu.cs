using CentaursBoardGame;
using UnityEngine;
using UnityEngine.Events;

#nullable enable

public class BoardAttackMenu : MonoBehaviour
{
    [SerializeField] private Game _game;
    [SerializeField] private GameObject _confirmPanel;
    [SerializeField] private ArduinoCommunicationDisplay _arduinoCommunicationDisplay;

    private bool _isLocked = false;

    private void Awake()
    {
        _confirmPanel.SetActive(false);
    }

    public void PerformAttack()
    {
        if (!_isLocked && _game.StateKind == GameStateKind.AwaitingPlayerInput)
        {
            ShowConfirmPanel();
        }
    }

    public void GoBack()
    {
        if (!_isLocked && _game.StateKind == GameStateKind.ShowingBoardAttack)
        {
            _game.SwitchState<AwaitingPlayerInputState>();
        }
    }

    public void Lock()
        => _isLocked = true;

    public void Unlock()
        => _isLocked = false;

    public void ShowConfirmPanel()
    {
        _confirmPanel.SetActive(true);
    }

    public void HideConfirmPanel()
    {
        _confirmPanel.SetActive(false);
    }

    public void ConfirmAttack()
    {
        _game.SwitchState<ShowAttackState>();
    }
}
