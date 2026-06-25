using UnityEngine;
using Fusion;
using System.Collections;

public class GameplayRoleReader : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);

        NetworkRunner runner = FindFirstObjectByType<NetworkRunner>();

        if (runner == null)
        {
            Debug.LogError("GAMEPLAY: No se encontró NetworkRunner en Test.");
        }
        else
        {
            Debug.Log("GAMEPLAY: NetworkRunner encontrado.");
            Debug.Log("GAMEPLAY: Soy host/server = " + runner.IsServer);

            int totalPlayers = 0;

            foreach (PlayerRef player in runner.ActivePlayers)
            {
                totalPlayers++;
                Debug.Log("GAMEPLAY: Player conectado = " + player);
            }

            Debug.Log("GAMEPLAY: Total jugadores conectados = " + totalPlayers);
        }

        Debug.Log("GAMEPLAY CACHE: Jugadores guardados = " + GameplayRoleCache.Players.Count);

        foreach (GameplayRoleCache.PlayerInfo info in GameplayRoleCache.Players)
        {
            Debug.Log(
                "GAMEPLAY CACHE DATA: PlayerRef = " +
                info.PlayerRef +
                " | Nombre = " +
                info.PlayerName +
                " | Rol = " +
                info.PlayerRole
            );
        }
    }
}