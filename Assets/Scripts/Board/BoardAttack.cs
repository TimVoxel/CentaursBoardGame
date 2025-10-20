using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;

#nullable enable

public class ShuffleRingsAttack : BoardAttack
{
    private readonly ImmutableDictionary<BoardRing, byte> _ringToSectors;

    public override BoardAttackType Type => BoardAttackType.ShuffleRings;

    public IDictionary<BoardRing, byte> Rotations => _ringToSectors;

    public ShuffleRingsAttack(params (BoardRing ring, byte sectorCount)[] rotations)
    {
        Debug.Assert(rotations.Length > 0);
        var builder = ImmutableDictionary.CreateBuilder<BoardRing, byte>();

        foreach (var rotation in rotations)
        {
            builder.Add(rotation.ring, rotation.sectorCount);
        }

        _ringToSectors = builder.ToImmutable();
    }

    public byte this[BoardRing ring]
    {
        get => _ringToSectors[ring];
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