using System;

public interface IBoardHandler
{
    public void RotateRings(params (byte ring, byte sectorCount)[] rings);
    public void StopRings(params byte[] rings);
    public void StopAllRings();

    public bool IsConnected();

    public event Action OnStoppedRotating;
}