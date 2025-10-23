using System;

public static class GameFacts
{
    public static readonly BoardRing[] BoardRingValues = (BoardRing[]) Enum.GetValues(typeof(BoardRing));
    public static readonly BoardAttackType[] BoardAttackTypeValues = (BoardAttackType[]) Enum.GetValues(typeof(BoardAttackType));
    public static readonly BoardRing[] RotatableRings = new BoardRing[2] 
    {
        BoardRing.Middle,
        BoardRing.Inner
    };

    public const string DefaultContextSaveFileName = "context.txt";
    public const int BoardSectorCount = 24;
    public const int MaxDiscardCardsAmount = 2;
    public const int MaxPlayers = 4;

    public static BoardRing GetRandomBoardRing()
    {
        var index = UnityEngine.Random.Range(0, BoardRingValues.Length);
        return BoardRingValues[index];
    }

    public static BoardRing GetRandomRotatableRing()
    {
        var index = UnityEngine.Random.Range(0, RotatableRings.Length);
        return RotatableRings[index];
    }

    public static BoardAttackType GetRandomBoardAttackType()
    {
        var index = UnityEngine.Random.Range(0, BoardAttackTypeValues.Length);
        return BoardAttackTypeValues[index];
    }
}