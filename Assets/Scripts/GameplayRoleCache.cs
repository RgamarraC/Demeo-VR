using Fusion;
using System.Collections.Generic;

public static class GameplayRoleCache
{
    public struct PlayerInfo
    {
        public PlayerRef PlayerRef;
        public string PlayerName;
        public string PlayerRole;
    }

    public static List<PlayerInfo> Players = new List<PlayerInfo>();

    public static string LocalRole = "Heroe 1";

    public static void Clear()
    {
        Players.Clear();
    }

    public static void Add(PlayerRef playerRef, string playerName, string playerRole)
    {
        Players.Add(new PlayerInfo
        {
            PlayerRef = playerRef,
            PlayerName = playerName,
            PlayerRole = playerRole
        });
    }
}
