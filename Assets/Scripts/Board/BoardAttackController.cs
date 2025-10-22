using System;
using System.Linq;

#nullable enable

[System.Serializable]
public class BoardAttackController
{
	private readonly BoardAttackFactory _factory;
	private readonly GameContext _gameContext;

	public event Action<BoardAttack>? OnBoardAttacked;

	public BoardAttackController(GameContext gameContext)
	{
		_gameContext = gameContext;
        _factory = new BoardAttackFactory(gameContext);
    }

	public void PerformNextAttack()
	{
		var attack = _factory.CreateRandomAttack();
		_gameContext.AttackHistory.Add(attack);
		OnBoardAttacked?.Invoke(attack);
	}
}