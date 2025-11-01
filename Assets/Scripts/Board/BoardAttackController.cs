using System;

#nullable enable

[Serializable]
public class BoardAttackController
{
	private readonly BoardAttackFactory _factory;
	private readonly GameContext _gameContext;

	public BoardAttackController(GameContext gameContext)
	{
		_gameContext = gameContext;
        _factory = new BoardAttackFactory(gameContext);
    }

	public BoardAttack RegisterNewAttack()
	{
		var attack = _factory.CreateRandomAttack();
		_gameContext.AttackHistory.Add(attack);
		return attack;
	}
}