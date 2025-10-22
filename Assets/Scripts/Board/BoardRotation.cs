
#nullable enable

public struct BoardRotation
{
    public BoardRing Ring { get; }
    public byte SectorCount { get; }

    public BoardRotation(BoardRing ring, byte sectorCount)
    {
        Ring = ring;
        SectorCount = sectorCount;
    }
}
