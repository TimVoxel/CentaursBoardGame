using System;
using System.Collections.Generic;

#nullable enable

public interface IBoardHandler
{
    public void RotateRings(IList<BoardRotation> rotations);
    public void StopRings(IList<BoardRing> rings);
    public void StopAllRings();

    public event Action<ArduinoResponse>? OnReceivedResponse;
}