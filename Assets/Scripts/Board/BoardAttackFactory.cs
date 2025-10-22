using System;

public class BoardAttackFactory
{
    private GameContext _gameContext;

    //We're not including the actual sector count because that would result in a full rotation
    //Which will just do nothing gameplay-wise
    private byte RandomSectorCount => (byte)UnityEngine.Random.Range(1, _gameContext.BoardSectorCount);

    public BoardAttackFactory(GameContext gameContext)
    {
        _gameContext = gameContext;
    }

    public BoardAttack CreateRandomAttack()
    {
        //TODO: Select based on attack history;

        BoardAttackType attackType;

        do
        {
            attackType = GameFacts.GetRandomBoardAttackType();
        }
        while (attackType == BoardAttackType.ForceSwapHands && _gameContext.FinishedCount == 1);

        return attackType switch
        {
            BoardAttackType.AttackRing => CreateAttackRing(),
            BoardAttackType.AttackCentreEntrances => CreateAttackCentreEntrances(),
            BoardAttackType.ForceDiscardCards => CreateForceDiscardCards(),
            BoardAttackType.ForceSwapHands => CreateForceSwapHands(),
            BoardAttackType.RotateRing => CreateRotateRing(),
            BoardAttackType.ShuffleRings => CreateShuffleRings(),
            _ => throw new Exception($"Unexpected attack type: {attackType}")
        };
    }
    private AttackRingAttack CreateAttackRing()
        => new AttackRingAttack(GameFacts.GetRandomBoardRing());
    private AttackCentreEntrancesAttack CreateAttackCentreEntrances()
        => new AttackCentreEntrancesAttack();
   
    private ForceDiscardCardsAmount CreateForceDiscardCards()
    {
        var cardCount = (byte)UnityEngine.Random.Range(1, GameFacts.MaxDiscardCardsAmount + 1);
        return new ForceDiscardCardsAmount(cardCount);
    }

    private ForceSwapHandsAttack CreateForceSwapHands()
    {
        Player player1;
        Player player2;

        do
        {
            player1 = _gameContext.GetRandomPlayer();
        }
        while (_gameContext.HasFinished(player1));

        do
        {
            player2 = _gameContext.GetRandomPlayer();
        }
        while (player1 == player2 || _gameContext.HasFinished(player2));

        return new ForceSwapHandsAttack(player1, player2);
    }

    private RotateRingAttack CreateRotateRing()
        => new RotateRingAttack(GameFacts.GetRandomRotatableRing(), RandomSectorCount);
    
    private ShuffleRingsAttack CreateShuffleRings()
        => new ShuffleRingsAttack(
            new BoardRotation(BoardRing.Inner, RandomSectorCount),
            new BoardRotation(BoardRing.Middle, RandomSectorCount));

}