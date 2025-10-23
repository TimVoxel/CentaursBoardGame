using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using UnityEngine;

#nullable enable

[Serializable]
public class GameContextBuilder
{
    //Helper to create game contexts in the editor
    //And load them using JsonUtility

    public Player[]? Players = new Player[0];
    public int BoardSectorCount = GameFacts.BoardSectorCount;
    public BoardAttack[]? AttackHistory;
    public string[]? FinishedNames;

    public GameContextBuilder(Player[] players,
                              int boardSectorCount,
                              BoardAttack[] attackHistory,
                              string[] finishedNames)
    {
        Players = players;
        BoardSectorCount = boardSectorCount;
        AttackHistory = attackHistory;
        FinishedNames = finishedNames;
    }

    public GameContext ToContext()
    {
        Debug.Assert(Players != null && Players.Length > 1, "There must be at least 2 players");
        
        var players = ImmutableArray.CreateRange(Players ?? throw new Exception());
        var finishedPlayers = new HashSet<Player>();
        var attackHistory = new List<BoardAttack>();

        if (AttackHistory != null)
        {
            attackHistory.AddRange(AttackHistory);
        }

        if (FinishedNames != null)
        {
            foreach (var name in FinishedNames)
            {
                var player = Players.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                if (player == null)
                {
                    Debug.LogWarning($"The player \"{name}\" was not registered in the game context and therefore cannot be marked as finished");
                }
                else
                {
                    finishedPlayers.Add(player);
                }
            }
        }
        return new GameContext(players,
                               BoardSectorCount,
                               attackHistory,
                               finishedPlayers);
    }
}

public class GameContext
{
    private readonly HashSet<Player> _finishedPlayers;
    public ImmutableArray<Player> Players { get; }
    public int BoardSectorCount { get; }
    public List<BoardAttack> AttackHistory { get; }

    public int FinishedCount => _finishedPlayers.Count;
    public bool AllFinished => Players.Length == _finishedPlayers.Count;

    public IEnumerable<Player> UnfinishedPlayers => Players.Except(_finishedPlayers);

    public GameContext(ImmutableArray<Player> players,
                        int boardSectorCount) :
        this(players, 
            boardSectorCount, 
            new List<BoardAttack>(),
            new HashSet<Player>()) { }

    public GameContext(ImmutableArray<Player> players,
                        int boardSectorCount,
                        List<BoardAttack> attackHistory,
                        HashSet<Player> finishedPlayers)
    {
        _finishedPlayers = finishedPlayers;
        AttackHistory = attackHistory;
        Players = players;
        BoardSectorCount = boardSectorCount;
    }

    public Player GetRandomPlayer()
    {
        var index = UnityEngine.Random.Range(0, Players.Length);
        return Players[index];
    }

    public void RegisterFinished(Player player)
        => _finishedPlayers.Add(player);

    public bool HasFinished(Player player)
        => _finishedPlayers.Contains(player);

    private GameContextBuilder ToBuilder()
        => new GameContextBuilder(Players.ToArray(),
                                  BoardSectorCount,
                                  AttackHistory.ToArray(),
                                  _finishedPlayers.Select(p => p.Name).ToArray());
    

    public void Save(string fileName = GameFacts.DefaultContextSaveFileName)
    {
        var path = Path.Combine(Application.persistentDataPath, fileName);
        var builder = ToBuilder();
        var serialized = JsonUtility.ToJson(builder);
        File.WriteAllText(path, serialized);
    }

    public static GameContext? Load(string fileName = GameFacts.DefaultContextSaveFileName)
    {
        var path = Path.Combine(Application.persistentDataPath, fileName);
        
        if (File.Exists(path))
        {
            var text = File.ReadAllText(path);
            return JsonUtility.FromJson<GameContextBuilder>(text).ToContext();
        }

        return null;
    }
}