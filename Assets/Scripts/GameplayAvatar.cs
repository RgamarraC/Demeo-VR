using Fusion;
using UnityEngine;

public class GameplayAvatar : NetworkBehaviour
{
    [Networked] public PlayerRef Owner { get; set; }
    [Networked] public NetworkString<_32> PlayerName { get; set; }
    [Networked] public NetworkString<_32> PlayerRole { get; set; }

    public void SetData(PlayerRef owner, string playerName, string playerRole)
    {
        Owner = owner;
        PlayerName = playerName;
        PlayerRole = playerRole;
    }

    public override void Spawned()
    {
        Debug.Log(
            "GAMEPLAY AVATAR SPAWNED | Nombre: " +
            PlayerName.ToString() +
            " | Rol: " +
            PlayerRole.ToString() +
            " | InputAuthority: " +
            Object.InputAuthority
        );
    }
}