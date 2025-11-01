using System.Collections.Generic;
using System.Text;

#nullable enable

public class ArduinoRotateRequest : IArduinoRequest
{
    private string? _serialized;
    public IList<BoardRotation> Rotations { get; }
    
    public ArduinoRequestType Type => ArduinoRequestType.Rotate;

    public ArduinoRotateRequest(IList<BoardRotation> rotations)
    {
        Rotations = rotations;
    }

    public string Serialize()
    {
        if (_serialized == null)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("r:");

            for (var i = 0; i < Rotations.Count; i++)
            {
                var rotation = Rotations[i];
                var isClockwiseMarker = rotation.IsClockwise
                    ? '1' 
                    : '0';
                var ringIndex = (byte) rotation.Ring;
                var sectorCount = rotation.SectorCount;
                stringBuilder.Append($"{isClockwiseMarker}{ringIndex} {sectorCount}");

                if (i != Rotations.Count - 1)
                {
                    stringBuilder.Append(",");
                }
            }
            _serialized = stringBuilder.ToString();
        }
        return _serialized;
    }

}
