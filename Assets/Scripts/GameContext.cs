using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;

#nullable enable

[System.Serializable]
public class GameContextBuilder
{
    //Helper to create game contexts in the editor

    [SerializeField] private Player[] _players = new Player[0];
    [SerializeField] private int _boardSectorCount = GameFacts.BoardSectorCount;
    [SerializeField] private List<BoardAttack> _attackHistory = new List<BoardAttack>();
    [SerializeField] private List<string> _finishedNames = new List<string>();

    public GameContext ToContext()
    {
        Debug.Assert(_players != null && _players.Length > 1, "There must be at least 2 players");

        var players = ImmutableArray.CreateRange(_players ?? throw new Exception());
        var attackHistory = _attackHistory ?? new List<BoardAttack>();
        var finishedPlayers = new HashSet<Player>();

        if (_finishedNames != null)
        {
            foreach (var name in _finishedNames)
            {
                var player = _players.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

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
        return new GameContext(players, _boardSectorCount, attackHistory, finishedPlayers);
    }
}

public class GameContext
{
       

    private readonly HashSet<Player> _finishedPlayers;
        
    public ImmutableArray<Player> Players { get; }
    public int BoardSectorCount { get; }
    public List<BoardAttack> AttackHistory { get; }

    public int FinishedCount => _finishedPlayers.Count;

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
}