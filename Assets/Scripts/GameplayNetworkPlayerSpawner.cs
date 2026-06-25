using UnityEngine;
using Fusion;
using System.Collections;

public class GameplayNetworkPlayerSpawner : MonoBehaviour
{
    [Header("Prefab del jugador")]
    [SerializeField] private NetworkPrefabRef playerPrefab;

    [Header("Spawns en Test")]
    [SerializeField] private Transform spawn1;
    [SerializeField] private Transform spawn2;
    [SerializeField] private Transform spawn3;

    private bool yaSpawneo = false;

    private IEnumerator Start()
    {
        // Esperamos un poco para que Fusion termine de cargar la escena Test
        yield return new WaitForSeconds(1f);

        NetworkRunner runner = BuscarRunnerActivo();

        if (runner == null)
        {
            Debug.LogError("GAMEPLAY SPAWNER: No se encontró el NetworkRunner que viene de la lobby.");
            yield break;
        }

        runner.ProvideInput = true;

        Debug.Log("GAMEPLAY SPAWNER: Runner encontrado.");
        Debug.Log("GAMEPLAY SPAWNER: Soy host/server = " + runner.IsServer);

        int totalJugadores = 0;

        foreach (PlayerRef player in runner.ActivePlayers)
        {
            totalJugadores++;
            Debug.Log("GAMEPLAY SPAWNER: Player conectado = " + player);
        }

        Debug.Log("GAMEPLAY SPAWNER: Total jugadores conectados = " + totalJugadores);

        // Solo el host debe hacer los spawns
        if (!runner.IsServer)
        {
            Debug.Log("GAMEPLAY SPAWNER: Soy cliente. El host hará los spawns.");
            yield break;
        }

        if (yaSpawneo)
            yield break;

        yaSpawneo = true;

        int index = 0;

        foreach (PlayerRef player in runner.ActivePlayers)
        {
            Transform spawn = ObtenerSpawn(index);

            Vector3 posicion = spawn != null ? spawn.position : Vector3.zero;
            Quaternion rotacion = spawn != null ? spawn.rotation : Quaternion.identity;

            runner.Spawn(
                playerPrefab,
                posicion,
                rotacion,
                player
            );

            Debug.Log("GAMEPLAY SPAWNER: Network Player spawneado para " + player);

            index++;
        }
    }

    private NetworkRunner BuscarRunnerActivo()
    {
        NetworkRunner[] runners =
            FindObjectsByType<NetworkRunner>(FindObjectsSortMode.None);

        foreach (NetworkRunner runner in runners)
        {
            if (runner == null)
                continue;

            int cantidad = 0;

            foreach (PlayerRef player in runner.ActivePlayers)
            {
                cantidad++;
            }

            if (cantidad > 0)
                return runner;
        }

        return null;
    }

    private Transform ObtenerSpawn(int index)
    {
        if (index == 0)
            return spawn1;

        if (index == 1)
            return spawn2;

        if (index == 2)
            return spawn3;

        return null;
    }
}
