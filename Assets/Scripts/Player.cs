using System.IO;
using UnityEngine;

#nullable enable

[System.Serializable]
public class Player
{
    [SerializeField] private string _name;
    [SerializeField] private PlayerColor _color;

    public string Name => _name;
    public PlayerColor Color => _color;

    public Player(string name, PlayerColor color)
    {
        _name = name;
        _color = color;
    }

    public Player(PlayerColor color)
    {
        _name = color.ToString();
        _color = color;
    }

    public string WriteToString()
    {
        var stringWriter = new StringWriter();
        this.WriteTo(stringWriter);
        return stringWriter.ToString();
    }
}
