using UnityEngine;
using TMPro;
using Fusion;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class LobbyGenerator : MonoBehaviour
{
    [Header("PREFAB DE DATOS")]
    public NetworkObject lobbyPlayerPrefab;

    [Header("INPUT HOST")]
    public TMP_InputField nombreInput;

    [Header("TEXTOS LOBBY")]
    public TMP_Text textoCodigo;
    public TMP_Text textoJugadores;
    public TMP_Text textoMensajeLobby;

    [Header("SLOTS")]
    public PlayerSlotUI slot1;
    public PlayerSlotUI slot2;
    public PlayerSlotUI slot3;

    public static string codigoLobby;
    public static string hostName;

    private NetworkRunner runner;
    private LobbyCallbacks callbacks;

    private Dictionary<PlayerRef, NetworkObject> spawnedLobbyPlayers =
        new Dictionary<PlayerRef, NetworkObject>();

    private float updateTimer;

    // Esto nos dice si el runner fue creado por el LobbyGenerator como host.
    // Si es true, al cerrar destruimos todo el GameObject temporal.
    private bool destruirRunnerGameObjectAlCerrar = false;

    public async void CrearSala()
    {
        hostName = nombreInput.text.Trim();

        if (string.IsNullOrEmpty(hostName))
        {
            Debug.LogWarning("Falta nombre del host");
            return;
        }

        // Limpiar cualquier runner anterior antes de crear nueva sala.
        await LimpiarRunnerActual();

        codigoLobby = Random.Range(1000, 10000).ToString();

        // Creamos un objeto temporal SOLO para el NetworkRunner.
        GameObject runnerObject = new GameObject("LobbyNetworkRunner");
        runner = runnerObject.AddComponent<NetworkRunner>();

        destruirRunnerGameObjectAlCerrar = true;

        callbacks = new LobbyCallbacks(this);
        runner.AddCallbacks(callbacks);

        Debug.Log("Creando sala con código: " + codigoLobby);

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = codigoLobby,
            PlayerCount = 3
        });

        if (!result.Ok)
        {
            Debug.LogError("No se pudo crear la sala: " + result.ErrorMessage);
            await LimpiarRunnerActual();
            codigoLobby = "";
            return;
        }

        if (textoCodigo != null)
            textoCodigo.text = codigoLobby;

        UpdateUI();
    }

    public async void ConfigurarComoInvitado(NetworkRunner nuevoRunner, string nombreLocal, string codigo)
    {
        // Si ya había otro runner, lo limpiamos antes.
        if (runner != null && runner != nuevoRunner)
        {
            await LimpiarRunnerActual();
        }

        runner = nuevoRunner;

        // Este runner viene desde JoinLobbyUI, así que NO asumimos que podemos
        // destruir su GameObject completo. Solo lo cerramos como componente.
        destruirRunnerGameObjectAlCerrar = false;

        callbacks = new LobbyCallbacks(this);
        runner.AddCallbacks(callbacks);

        if (textoCodigo != null)
            textoCodigo.text = codigo;

        UpdateUI();
    }

    private void Update()
    {
        if (runner == null)
            return;

        updateTimer += Time.deltaTime;

        if (updateTimer >= 0.25f)
        {
            updateTimer = 0f;
            UpdateUI();
        }
    }

    public void HandlePlayerJoined(NetworkRunner networkRunner, PlayerRef player)
    {
        if (!networkRunner.IsServer)
        {
            RefreshUI();
            return;
        }

        if (spawnedLobbyPlayers.ContainsKey(player))
            return;

        if (lobbyPlayerPrefab == null)
        {
            Debug.LogError("LobbyGenerator: falta asignar LobbyPlayerPrefab.");
            return;
        }

        int slotIndex = GetNextAvailableSlot();

        if (slotIndex == -1)
        {
            Debug.LogWarning("Lobby llena. No hay slots disponibles.");
            return;
        }

        NetworkObject obj = networkRunner.Spawn(
            lobbyPlayerPrefab,
            Vector3.zero,
            Quaternion.identity,
            player
        );

        LobbyPlayerData data = obj.GetComponent<LobbyPlayerData>();

        data.Owner = player;
        data.SlotIndex = slotIndex;

        spawnedLobbyPlayers[player] = obj;

        RefreshUI();
    }

    public void HandlePlayerLeft(NetworkRunner networkRunner, PlayerRef player)
    {
        if (networkRunner.IsServer && spawnedLobbyPlayers.ContainsKey(player))
        {
            NetworkObject obj = spawnedLobbyPlayers[player];

            if (obj != null)
                networkRunner.Despawn(obj);

            spawnedLobbyPlayers.Remove(player);
        }

        RefreshUI();
    }

    // HOST MIGRATION REAL:
    // Aquí ya no solo detectamos. Ahora intentamos crear un nuevo runner
    // usando el HostMigrationToken.
    public async void HandleHostMigration(NetworkRunner oldRunner, HostMigrationToken hostMigrationToken)
    {
        Debug.Log("HOST MIGRATION DETECTADA EN LOBBY GENERATOR");
        Debug.Log("Iniciando proceso real de Host Migration...");

        NetworkRunner runnerViejo = oldRunner;

        // Limpiamos referencias locales para no reutilizar el runner viejo.
        runner = null;
        callbacks = null;
        spawnedLobbyPlayers.Clear();
        destruirRunnerGameObjectAlCerrar = false;

        if (runnerViejo != null)
        {
            Debug.Log("Apagando runner viejo por Host Migration...");

            await runnerViejo.Shutdown(shutdownReason: ShutdownReason.HostMigration);

            await Task.Yield();

            if (runnerViejo != null)
                Destroy(runnerViejo);
        }

        await Task.Yield();

        // Creamos un runner nuevo y limpio.
        GameObject runnerObject = new GameObject("LobbyNetworkRunner_Migrated");
        runner = runnerObject.AddComponent<NetworkRunner>();

        destruirRunnerGameObjectAlCerrar = true;

        callbacks = new LobbyCallbacks(this);
        runner.AddCallbacks(callbacks);

        Debug.Log("Creando nuevo NetworkRunner usando HostMigrationToken...");

        var result = await runner.StartGame(new StartGameArgs()
        {
            HostMigrationToken = hostMigrationToken,
            HostMigrationResume = HostMigrationResume
        });

        if (!result.Ok)
        {
            Debug.LogError("Falló Host Migration: " + result.ErrorMessage);
            await LimpiarRunnerActual();
            return;
        }

        Debug.Log("Host Migration completada.");

        await Task.Yield();

        ReordenarSlotsDespuesDeMigracion();
        UpdateUI();
    }

    private void HostMigrationResume(NetworkRunner newRunner)
    {
        Debug.Log("HostMigrationResume ejecutado.");

        foreach (NetworkObject resumeObject in newRunner.GetResumeSnapshotNetworkObjects())
        {
            if (resumeObject == null)
                continue;

            LobbyPlayerData oldData = resumeObject.GetComponent<LobbyPlayerData>();

            // Solo reconstruimos objetos que sean datos de lobby.
            if (oldData == null)
                continue;

            PlayerRef owner;

            try
            {
                owner = oldData.Owner;
            }
            catch (System.InvalidOperationException)
            {
                continue;
            }

            Debug.Log("Reconstruyendo LobbyPlayerData desde snapshot.");

            newRunner.Spawn(
                resumeObject,
                Vector3.zero,
                Quaternion.identity,
                owner,
                (runnerLocal, newObject) =>
                {
                    newObject.CopyStateFrom(resumeObject);

                    LobbyPlayerData newData = newObject.GetComponent<LobbyPlayerData>();

                    if (newData != null)
                    {
                        try
                        {
                            spawnedLobbyPlayers[newData.Owner] = newObject;
                        }
                        catch (System.InvalidOperationException)
                        {
                            // Si todavía no se puede leer la data, no rompemos el flujo.
                        }
                    }
                }
            );
        }
    }

    private void ReordenarSlotsDespuesDeMigracion()
    {
        if (runner == null)
            return;

        // Solo el nuevo host/server debe reordenar slots.
        if (!runner.IsServer)
        {
            Debug.Log("Este jugador no es el nuevo host. No reordena slots.");
            return;
        }

        LobbyPlayerData[] players =
            FindObjectsByType<LobbyPlayerData>(FindObjectsSortMode.None);

        List<LobbyPlayerData> jugadoresActivos = new List<LobbyPlayerData>();

        foreach (LobbyPlayerData data in players)
        {
            if (data == null)
                continue;

            PlayerRef owner;

            try
            {
                owner = data.Owner;
            }
            catch (System.InvalidOperationException)
            {
                continue;
            }

            bool ownerSigueActivo = false;

            foreach (PlayerRef activePlayer in runner.ActivePlayers)
            {
                if (activePlayer == owner)
                {
                    ownerSigueActivo = true;
                    break;
                }
            }

            // Si el Owner ya no está activo, era probablemente el host que se fue.
            if (!ownerSigueActivo)
            {
                NetworkObject networkObject = data.GetComponent<NetworkObject>();

                if (networkObject != null)
                {
                    Debug.Log("Eliminando dato de jugador que ya no está activo.");
                    runner.Despawn(networkObject);
                }

                continue;
            }

            jugadoresActivos.Add(data);
        }

        // Ordenamos por el slot anterior.
        jugadoresActivos.Sort((a, b) => a.SlotIndex.CompareTo(b.SlotIndex));

        // Reasignamos slots desde 0.
        for (int i = 0; i < jugadoresActivos.Count; i++)
        {
            jugadoresActivos[i].SlotIndex = i;
        }

        Debug.Log("Slots reordenados después de Host Migration.");
    }

    private int GetNextAvailableSlot()
    {
        bool[] usedSlots = new bool[3];

        LobbyPlayerData[] players =
            FindObjectsByType<LobbyPlayerData>(FindObjectsSortMode.None);

        foreach (LobbyPlayerData player in players)
        {
            if (player == null)
                continue;

            int index;

            try
            {
                index = player.SlotIndex;
            }
            catch (System.InvalidOperationException)
            {
                continue;
            }

            if (index >= 0 && index < 3)
                usedSlots[index] = true;
        }

        for (int i = 0; i < 3; i++)
        {
            if (!usedSlots[i])
                return i;
        }

        return -1;
    }

    public void UpdateUI()
    {
        LobbyPlayerData[] players =
            FindObjectsByType<LobbyPlayerData>(FindObjectsSortMode.None);

        string[] nombres = new string[3] { "Vacío", "Vacío", "Vacío" };
        string[] roles = new string[3] { "-", "-", "-" };

        int count = 0;

        foreach (LobbyPlayerData player in players)
        {
            if (player == null)
                continue;

            int slot;
            string nombre;
            string rolJugador;

            try
            {
                slot = player.SlotIndex;
                nombre = player.PlayerName.ToString();
                rolJugador = player.PlayerRole.ToString();
            }
            catch (System.InvalidOperationException)
            {
                continue;
            }

            if (slot < 0 || slot >= 3)
                continue;

            if (string.IsNullOrEmpty(nombre))
                nombre = "Player";

            if (string.IsNullOrEmpty(rolJugador))
                rolJugador = "Sin rol";

            nombres[slot] = nombre;
            roles[slot] = rolJugador;

            count++;
        }

        if (textoJugadores != null)
            textoJugadores.text = count + "/3";

        ActualizarSlotSeguro(slot1, nombres[0], roles[0]);
        ActualizarSlotSeguro(slot2, nombres[1], roles[1]);
        ActualizarSlotSeguro(slot3, nombres[2], roles[2]);
    }

    private void ActualizarSlotSeguro(PlayerSlotUI slot, string nombre, string rol)
    {
        if (slot == null)
        {
            Debug.LogWarning("Hay un PlayerSlotUI no asignado en el LobbyGenerator.");
            return;
        }

        slot.ActualizarSlot(nombre, rol);
    }

    private void MostrarMensajeLobby(string mensaje)
    {
        Debug.LogWarning(mensaje);

        if (textoMensajeLobby != null)
            textoMensajeLobby.text = mensaje;
    }
    
    public void RefreshUI()
    {
        UpdateUI();
    }

// ============================================================
// BOTÓN START + VALIDACIÓN DE ROLES
// ============================================================

    public void IniciarPartida()
    {
        if (runner == null)
        {
            MostrarMensajeLobby("No hay sala activa.");
            return;
        }

        if (!runner.IsServer)
        {
            MostrarMensajeLobby("Solo el host puede iniciar.");
            return;
        }

        if (!RolesValidosParaIniciar(out string mensaje))
        {
            MostrarMensajeLobby(mensaje);
            return;
        }

        MostrarMensajeLobby("Roles válidos. Cargando escena Test...");

        SceneManager.LoadScene("Test");
    }

private bool RolesValidosParaIniciar(out string mensaje)
{
    LobbyPlayerData[] players =
        FindObjectsByType<LobbyPlayerData>(FindObjectsSortMode.None);

    int total = 0;
    bool dm = false;
    bool heroe1 = false;
    bool heroe2 = false;

    foreach (LobbyPlayerData player in players)
    {
        if (player == null)
            continue;

        string rol = player.PlayerRole.ToString().Trim();

        if (string.IsNullOrEmpty(rol) || rol == "Sin rol")
        {
            mensaje = "Todos deben elegir un rol.";
            return false;
        }

        string r = NormalizarRol(rol);

        if (r == "dungeon master")
            dm = true;
        else if (r == "heroe 1")
            heroe1 = true;
        else if (r == "heroe 2")
            heroe2 = true;

        total++;
    }

    if (total < 2)
    {
        mensaje = "Se necesitan al menos 2 jugadores.";
        return false;
    }

    if (total == 2)
    {
        if (!dm)
        {
            mensaje = "Con 2 jugadores debe haber Dungeon Master.";
            return false;
        }

        if (!heroe1 && !heroe2)
        {
            mensaje = "Con 2 jugadores debe haber un héroe.";
            return false;
        }

        mensaje = "Roles válidos.";
        return true;
    }

    if (total == 3)
    {
        if (!dm || !heroe1 || !heroe2)
        {
            mensaje = "Con 3 jugadores debe haber DM, Heroe 1 y Heroe 2.";
            return false;
        }

        mensaje = "Roles válidos.";
        return true;
    }

    mensaje = "Cantidad de jugadores no válida.";
    return false;
}

private string NormalizarRol(string rol)
{
    return rol.Trim()
        .ToLower()
        .Replace("á", "a")
        .Replace("é", "e")
        .Replace("í", "i")
        .Replace("ó", "o")
        .Replace("ú", "u");
}

public async void SalirLobby()
{
    await LimpiarRunnerActual();

    codigoLobby = "";
    hostName = "";

    ActualizarSlotSeguro(slot1, "Vacío", "-");
    ActualizarSlotSeguro(slot2, "Vacío", "-");
    ActualizarSlotSeguro(slot3, "Vacío", "-");

    if (textoJugadores != null)
        textoJugadores.text = "0/3";

    if (textoCodigo != null)
        textoCodigo.text = "";
}
    private async Task LimpiarRunnerActual()
    {
        NetworkRunner runnerParaCerrar = runner;
        bool destruirGameObject = destruirRunnerGameObjectAlCerrar;

        runner = null;
        destruirRunnerGameObjectAlCerrar = false;

        if (runnerParaCerrar != null)
        {
            if (callbacks != null)
                runnerParaCerrar.RemoveCallbacks(callbacks);

            await runnerParaCerrar.Shutdown();

            // Esperamos un frame para que Fusion termine de limpiar internamente.
            await Task.Yield();

            if (runnerParaCerrar != null)
            {
                if (destruirGameObject && runnerParaCerrar.gameObject != gameObject)
                {
                    Destroy(runnerParaCerrar.gameObject);
                }
                else
                {
                    Destroy(runnerParaCerrar);
                }
            }
        }

        callbacks = null;
        spawnedLobbyPlayers.Clear();
    }
}