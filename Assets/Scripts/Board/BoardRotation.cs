
#nullable enable

public struct BoardRotation
{
    public BoardRing Ring { get; }
    public byte SectorCount { get; }
    public bool IsClockwise { get; }

    public BoardRotation(BoardRing ring, byte sectorCount, bool isClockwise)
    {
        Ring = ring;
        SectorCount = sectorCount;
        IsClockwise = isClockwise;
    }
}
