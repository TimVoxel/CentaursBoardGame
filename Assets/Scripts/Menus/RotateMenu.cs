using TMPro;
using UnityEngine;

#nullable enable

public class RotateMenu : MonoBehaviour
{
    [SerializeField] private Game _game;
    [SerializeField] private GameObject _rootPanel;
    [SerializeField] private TMP_Dropdown _directionDropdown;
    [SerializeField] private TMP_Dropdown _ringDropdown;
    [SerializeField] private TMP_Dropdown _amountDropdown;

    private bool _isShown = false;

    public IBoardHandler BoardHandler => _game.BoardHandler;

    public void Toggle()
    {
        if (!_isShown)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    public void Show()
    {
        _rootPanel.SetActive(true);
        _isShown = true;
    }

    public void Hide()
    {
        _rootPanel.SetActive(false);
        _isShown = false;
    }

    public void Rotate()
    {
        BoardHandler.RotateRings(new BoardRotation[1]
        {
             GetRotation()
        });
        Hide();
    }

    private BoardRotation GetRotation()
    {
        var isClockwise = _directionDropdown.value == 0;
        var ring = _ringDropdown.value switch
        {
            0 => BoardRing.Middle,
            1 => BoardRing.Inner,
            _ => throw new System.Exception($"Unexpected selected ring index: {_ringDropdown.value}")
        };
        var amount = (byte) (_amountDropdown.value + 1);
        return new BoardRotation(ring, amount, isClockwise);
    }
}