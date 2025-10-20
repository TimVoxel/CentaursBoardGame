using UnityEngine;

#nullable enable

public class BoardAttackMenu : MonoBehaviour
{
    [SerializeField] private Game _game;

    public void OnAttackClicked()
    {
        _game.BoardAttackController.PerformNextAttack();
    }
}
