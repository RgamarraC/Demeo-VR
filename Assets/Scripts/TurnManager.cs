using UnityEngine;
using TMPro;
using Fusion;
using System.Collections;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance;

    [Header("UI")]
    public TMP_Text textoTurno;

    [Header("Estado del turno")]
    [Networked] public int CurrentTurnIndex { get; set; }

    public string CurrentTurnName;
    public string CurrentTurnRole;
    public PlayerRef CurrentTurnPlayerRef;

    private GameplayManager gameplayManager;
    private bool listo = false;
    private int ultimoTurnoMostrado = -1;

    private float debugTimer = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {
        // Esperar a que exista GameplayManager
        while (GameplayManager.Instance == null)
        {
            yield return null;
        }

        gameplayManager = GameplayManager.Instance;

        // Esperar a que GameplayManager tenga el orden de turnos listo
        while (gameplayManager.TurnOrder == null || gameplayManager.TurnOrder.Count == 0)
        {
            yield return null;
        }

        listo = true;

        // Solo el host / State Authority inicializa el turno
        if (Object != null && Object.HasStateAuthority)
        {
            CurrentTurnIndex = 0;
            Debug.Log("TURN MANAGER NETWORK: Host inicializó el turno en 0.");
        }

        ActualizarTurnoActual();

        Debug.Log("TURN MANAGER NETWORK: Listo.");
    }

    private void Update()
    {
        if (!listo)
        {
            IntentarInicializarLocal();

            if (!listo)
                return;
        }

        // Si el turno cambió en red, actualizamos datos del turno
        if (ultimoTurnoMostrado != CurrentTurnIndex)
        {
            ActualizarTurnoActual();
        }
    }

    private void ActualizarTurnoActual()
    {
        if (gameplayManager == null)
            return;

        if (gameplayManager.TurnOrder == null || gameplayManager.TurnOrder.Count == 0)
            return;

        if (CurrentTurnIndex < 0 || CurrentTurnIndex >= gameplayManager.TurnOrder.Count)
            CurrentTurnIndex = 0;

        GameplayRoleCache.PlayerInfo jugadorActual =
            gameplayManager.TurnOrder[CurrentTurnIndex];

        CurrentTurnName = jugadorActual.PlayerName;
        CurrentTurnRole = jugadorActual.PlayerRole;
        CurrentTurnPlayerRef = jugadorActual.PlayerRef;

        if (textoTurno != null)
        {
            textoTurno.text = "Turno de " + CurrentTurnName;
        }

        ultimoTurnoMostrado = CurrentTurnIndex;

        Debug.Log(
            "TURN MANAGER NETWORK: Turno actual = " +
            CurrentTurnName +
            " | " +
            CurrentTurnRole +
            " | " +
            CurrentTurnPlayerRef
        );
    }

    private void IntentarInicializarLocal()
    {
        if (listo)
            return;

        if (GameplayManager.Instance == null)
            return;

        gameplayManager = GameplayManager.Instance;

        if (gameplayManager.TurnOrder == null || gameplayManager.TurnOrder.Count == 0)
            return;

        listo = true;
        ultimoTurnoMostrado = -999;

        ActualizarTurnoActual();

        Debug.Log(
            "TURN MANAGER NETWORK: Inicializado localmente | LocalPlayer = " +
            gameplayManager.LocalPlayerRef +
            " | Rol = " +
            gameplayManager.LocalPlayerRole +
            " | CurrentTurnIndex = " +
            CurrentTurnIndex +
            " | StateAuthority = " +
            Object.HasStateAuthority
        );
    }

    public bool IsMyTurn()
    {
        if (!listo)
        {
            IntentarInicializarLocal();

            if (!listo)
                return false;
        }

        if (gameplayManager == null)
            return false;

        if (gameplayManager.TurnOrder == null || gameplayManager.TurnOrder.Count == 0)
            return false;

        int index = CurrentTurnIndex;

        if (index < 0 || index >= gameplayManager.TurnOrder.Count)
            index = 0;

        PlayerRef jugadorDelTurno =
            gameplayManager.TurnOrder[index].PlayerRef;

        return gameplayManager.LocalPlayerRef == jugadorDelTurno;
    }

    public void EndTurn()
    {
        if (!listo)
            return;

        if (!IsMyTurn())
        {
            Debug.Log("TURN MANAGER NETWORK: No puedes terminar turno porque no es tu turno.");
            return;
        }

        Debug.Log("TURN MANAGER NETWORK: Solicitando cambio de turno...");

        RPC_RequestEndTurn(gameplayManager.LocalPlayerRef);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestEndTurn(PlayerRef requester)
    {
        if (gameplayManager == null)
            return;

        if (gameplayManager.TurnOrder == null || gameplayManager.TurnOrder.Count == 0)
            return;

        GameplayRoleCache.PlayerInfo jugadorActual =
            gameplayManager.TurnOrder[CurrentTurnIndex];

        // Validación: solo puede pasar turno quien tiene el turno actual
        if (jugadorActual.PlayerRef != requester)
        {
            Debug.LogWarning(
                "TURN MANAGER NETWORK: " +
                requester +
                " intentó pasar turno, pero el turno es de " +
                jugadorActual.PlayerRef
            );

            return;
        }

        CurrentTurnIndex++;

        if (CurrentTurnIndex >= gameplayManager.TurnOrder.Count)
        {
            CurrentTurnIndex = 0;
        }

        Debug.Log("TURN MANAGER NETWORK: Turno cambiado por el host. Nuevo índice = " + CurrentTurnIndex);
    }
}