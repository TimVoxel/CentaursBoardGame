using System;

public class BoardAttackFactory
{
    private GameContext _gameContext;

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
    {
        var ring = GameFacts.GetRandomBoardRing();
        return new AttackRingAttack(ring);
    }

    private AttackCentreEntrancesAttack CreateAttackCentreEntrances()
    {
        return new AttackCentreEntrancesAttack();
    }

    private ForceDiscardCardsAmount CreateForceDiscardCards()
    {
        var sectorCount = (byte)UnityEngine.Random.Range(1, GameFacts.MaxDiscardCardsAmount + 1);
        return new ForceDiscardCardsAmount(sectorCount);
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
    {
        var ring = GameFacts.GetRandomRotatableRing();
        var sectorCount = (byte)UnityEngine.Random.Range(0, _gameContext.BoardSectorCount);
        return new RotateRingAttack(ring, sectorCount);
    }

    private ShuffleRingsAttack CreateShuffleRings()
    {
        var sectorCountInner = (byte)UnityEngine.Random.Range(0, _gameContext.BoardSectorCount);
        var sectorCountMiddle = (byte)UnityEngine.Random.Range(0, _gameContext.BoardSectorCount);
        return new ShuffleRingsAttack(
            (BoardRing.Inner, sectorCountInner),
            (BoardRing.Middle, sectorCountMiddle));
    }
}