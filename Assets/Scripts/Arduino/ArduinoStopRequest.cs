using System.Collections.Generic;
using System.Text;

#nullable enable

public class ArduinoStopRequest : IArduinoRequest
{
    private string? _serialized;

    public IList<BoardRing> Rings { get; }
    
    public ArduinoRequestType Type => ArduinoRequestType.Stop;

    public ArduinoStopRequest(IList<BoardRing> rings)
    {
        Rings = rings;
    }

    public string Serialize()
    {
        if (_serialized == null)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("s:");

            for (var i = 0; i < Rings.Count; i++)
            {
                var ring = Rings[i];
                stringBuilder.Append(ring);

                if (i != Rings.Count - 1)
                {
                    stringBuilder.Append(",");
                }
            }
            _serialized = stringBuilder.ToString();
        }
        return _serialized;
    }
}
