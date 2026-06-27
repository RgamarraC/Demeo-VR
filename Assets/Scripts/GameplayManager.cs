using UnityEngine;
using Fusion;
using System.Collections;
using System.Collections.Generic;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;

    [Header("Datos del jugador local")]
    public PlayerRef LocalPlayerRef;
    public string LocalPlayerName;
    public string LocalPlayerRole;

    [Header("Orden de turnos")]
    public List<GameplayRoleCache.PlayerInfo> TurnOrder =
        new List<GameplayRoleCache.PlayerInfo>();

    private NetworkRunner runner;

    private IEnumerator Start()
    {
        Instance = this;

        // Esperamos un poquito para que el Runner y el cache ya estén listos en Test
        yield return new WaitForSeconds(1f);

        BuscarRunner();

        if (runner == null)
        {
            Debug.Log("GAMEPLAY MANAGER: No se encontró NetworkRunner en Test.");
            yield break;
        }

        LocalPlayerRef = runner.LocalPlayer;

        LeerDatosDeLobby();
        DetectarJugadorLocal();
        CrearOrdenDeTurnos();
        MostrarResumen();
    }

    private void BuscarRunner()
    {
        runner = FindFirstObjectByType<NetworkRunner>();
    }

    private void LeerDatosDeLobby()
    {
        Debug.Log("GAMEPLAY MANAGER: Jugadores recibidos = " + GameplayRoleCache.Players.Count);

        foreach (GameplayRoleCache.PlayerInfo info in GameplayRoleCache.Players)
        {
            Debug.Log(
                "GAMEPLAY MANAGER DATA: PlayerRef = " +
                info.PlayerRef +
                " | Nombre = " +
                info.PlayerName +
                " | Rol = " +
                info.PlayerRole
            );
        }
    }

    private void DetectarJugadorLocal()
    {
        LocalPlayerName = "Jugador";
        LocalPlayerRole = "Sin rol";

        foreach (GameplayRoleCache.PlayerInfo info in GameplayRoleCache.Players)
        {
            if (info.PlayerRef == LocalPlayerRef)
            {
                LocalPlayerName = info.PlayerName;
                LocalPlayerRole = info.PlayerRole;

                Debug.Log(
                    "GAMEPLAY MANAGER LOCAL: Soy " +
                    LocalPlayerName +
                    " | Rol = " +
                    LocalPlayerRole
                );

                return;
            }
        }

        Debug.LogWarning(
            "GAMEPLAY MANAGER: No se encontró el jugador local en GameplayRoleCache. LocalPlayerRef = " +
            LocalPlayerRef
        );
    }

    private void CrearOrdenDeTurnos()
    {
        TurnOrder.Clear();

        GameplayRoleCache.PlayerInfo heroe1 = default;
        GameplayRoleCache.PlayerInfo heroe2 = default;
        GameplayRoleCache.PlayerInfo dungeonMaster = default;

        bool existeHeroe1 = false;
        bool existeHeroe2 = false;
        bool existeDM = false;

        foreach (GameplayRoleCache.PlayerInfo info in GameplayRoleCache.Players)
        {
            string rol = info.PlayerRole.Trim();

            if (rol == "Heroe 1")
            {
                heroe1 = info;
                existeHeroe1 = true;
            }
            else if (rol == "Heroe 2")
            {
                heroe2 = info;
                existeHeroe2 = true;
            }
            else if (rol == "Dungeon Master")
            {
                dungeonMaster = info;
                existeDM = true;
            }
        }

        if (existeHeroe1)
            TurnOrder.Add(heroe1);

        if (existeHeroe2)
            TurnOrder.Add(heroe2);

        if (existeDM)
            TurnOrder.Add(dungeonMaster);
    }

    private void MostrarResumen()
    {
        Debug.Log("========== GAMEPLAY MANAGER RESUMEN ==========");

        Debug.Log("Jugador local: " + LocalPlayerName);
        Debug.Log("Rol local: " + LocalPlayerRole);
        Debug.Log("PlayerRef local: " + LocalPlayerRef);

        Debug.Log("Orden de turnos:");

        for (int i = 0; i < TurnOrder.Count; i++)
        {
            Debug.Log(
                (i + 1) +
                ". " +
                TurnOrder[i].PlayerName +
                " | " +
                TurnOrder[i].PlayerRole +
                " | " +
                TurnOrder[i].PlayerRef
            );
        }

        Debug.Log("==============================================");
    }
}
