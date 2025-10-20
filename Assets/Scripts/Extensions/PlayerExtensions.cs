using System.IO;

public static class PlayerExtensions
{
    public static void WriteTo(this Player player, TextWriter writer)
    {
        writer.Write($"Player (Name: {player.Name}, Color: {player.Color})");
    }
}
