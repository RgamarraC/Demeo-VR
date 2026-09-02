using Fusion;
using UnityEngine;

public class LobbyPlayerData : NetworkBehaviour
{
    [Networked] public NetworkString<_16> PlayerName { get; set; }
    [Networked] public NetworkString<_16> PlayerRole { get; set; }
    [Networked] public int SlotIndex { get; set; }
    [Networked] public PlayerRef Owner { get; set; }

    public override void Spawned()
    {
        // Rol inicial por defecto
        if (Object.HasStateAuthority)
        {
            if (string.IsNullOrEmpty(PlayerRole.ToString()))
                PlayerRole = "Sin rol";
        }

        // Cada jugador envía su nombre local
        if (Object.HasInputAuthority)
        {
            EnviarNombreLocal();
        }
    }

    private void EnviarNombreLocal()
    {
        string localName = "";

        if (!string.IsNullOrEmpty(LobbyGenerator.hostName))
            localName = LobbyGenerator.hostName;
        else if (!string.IsNullOrEmpty(JoinLobbyUI.nombreJugador))
            localName = JoinLobbyUI.nombreJugador;

        if (string.IsNullOrEmpty(localName))
            localName = "Player";

        if (Object.HasStateAuthority)
        {
            PlayerName = localName;
        }
        else
        {
            RPC_SetPlayerName(localName);
        }
    }

    public void CambiarRolLocal(string nuevoRol)
    {
        if (string.IsNullOrEmpty(nuevoRol))
            nuevoRol = "Sin rol";

        if (Object.HasInputAuthority || Object.HasStateAuthority)
        {
            GameplayRoleCache.LocalRole = nuevoRol;
        }

        if (Object.HasStateAuthority)
        {
            PlayerRole = nuevoRol;
        }
        else
        {
            RPC_SetPlayerRole(nuevoRol);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetPlayerName(string name)
    {
        if (string.IsNullOrEmpty(name))
            name = "Player";

        PlayerName = name;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetPlayerRole(string role)
    {
        if (string.IsNullOrEmpty(role))
            role = "Sin rol";

        PlayerRole = role;
    }
}