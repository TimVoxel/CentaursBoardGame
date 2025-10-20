using System;

public static class GameFacts
{
    private static readonly BoardRing[] BoardRingValues = (BoardRing[]) Enum.GetValues(typeof(BoardRing));
    private static readonly BoardAttackType[] BoardAttackTypeValues = (BoardAttackType[]) Enum.GetValues(typeof(BoardAttackType));

    public const int BoardSectorCount = 24;
    public const int MaxDiscardCardsAmount = 2;

    public static BoardRing GetRandomBoardRing()
    {
        var index = UnityEngine.Random.Range(0, BoardRingValues.Length);
        return BoardRingValues[index];
    }

    public static BoardRing GetRandomRotatableRing()
        => UnityEngine.Random.Range(0, 2) == 0
            ? BoardRing.Inner
            : BoardRing.Middle;

    public static BoardAttackType GetRandomBoardAttackType()
    {
        var index = UnityEngine.Random.Range(0, BoardAttackTypeValues.Length);
        return BoardAttackTypeValues[index];
    }
}