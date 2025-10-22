public interface IArduinoRequest 
{
    public ArduinoRequestType Type { get; }
    public string Serialize();
}
