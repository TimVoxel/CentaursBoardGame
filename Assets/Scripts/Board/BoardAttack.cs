using System.Collections.Generic;
using UnityEngine;

#nullable enable

public class ShuffleRingsAttack : BoardAttack
{
    private readonly IList<BoardRotation> _ringToSectors;

    public override BoardAttackType Type => BoardAttackType.ShuffleRings;

    public IList<BoardRotation> Rotations => _ringToSectors;

    public ShuffleRingsAttack(IList<BoardRotation> rotations)
    {
        Debug.Assert(rotations.Count > 0);
        _ringToSectors = rotations;
    }

    public ShuffleRingsAttack(params BoardRotation[] rotations)
    {
        Debug.Assert(rotations.Length > 0);
        _ringToSectors = rotations;
    }
}

public class AttackRingAttack : BoardAttack
{
    public BoardRing Ring { get; }

    public override BoardAttackType Type => BoardAttackType.AttackRing;

    public AttackRingAttack(BoardRing ring)
    {
        Ring = ring;
    }
}

public class AttackCentreEntrancesAttack : BoardAttack
{
    public override BoardAttackType Type => BoardAttackType.AttackCentreEntrances;
}

public class ForceDiscardCardsAmount : BoardAttack
{
    public byte CardCount { get; }

    public override BoardAttackType Type => BoardAttackType.ForceDiscardCards;

    public ForceDiscardCardsAmount(byte cardCount)
    {
        CardCount = cardCount;
    }
}

public class ForceSwapHandsAttack : BoardAttack
{
    public Player First { get; }
    public Player Second { get; }

    public override BoardAttackType Type => BoardAttackType.ForceSwapHands;

    public ForceSwapHandsAttack(Player first, Player second)
    {
        First = first;
        Second = second;
    }
}

public class RotateRingAttack : BoardAttack
{
    public BoardRing Ring { get; }
    public byte SectorCount { get; }

    public override BoardAttackType Type => BoardAttackType.RotateRing;

    public RotateRingAttack(BoardRing ring, byte sectorCount)
    {
        Ring = ring;
        SectorCount = sectorCount;
    }
}

public abstract class BoardAttack
{
    public abstract BoardAttackType Type { get; }
}