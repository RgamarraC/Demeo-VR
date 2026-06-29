using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

public class DungeonMasterController : NetworkBehaviour
{
    [Header("Configuración de Invocación")]
    [Tooltip("Prefab del monstruo registrado en Fusion. Debe tener NetworkObject.")]
    [SerializeField] private NetworkPrefabRef prefabEnemigo;

    [Header("Configuración VR")]
    [Tooltip("El Transform del mando de VR desde donde sale el láser de apuntado.")]
    [SerializeField] private Transform punteroVR;

    [Tooltip("Input Action del gatillo para confirmar la invocación.")]
    [SerializeField] private InputActionReference botonConfirmar;

    [Header("Estado Interno")]
    [SerializeField] private bool estaEnModoInvocacion = false;

    private CasillaComponent ultimaCasillaApuntada = null;

    public override void Spawned()
    {
        Debug.Log(
            "[DungeonMasterController] Spawned en red. " +
            "StateAuthority = " + Object.HasStateAuthority +
            " | LocalPlayer = " + Runner.LocalPlayer
        );
    }

    private void OnEnable()
    {
        if (botonConfirmar != null)
            botonConfirmar.action.Enable();
    }

    private void OnDisable()
    {
        if (botonConfirmar != null)
            botonConfirmar.action.Disable();
    }

    private void Update()
    {
        if (!estaEnModoInvocacion)
            return;

        if (!PuedeUsarModoInvocacionLocal())
        {
            Debug.LogWarning("[DungeonMasterController] Modo invocación cancelado. Ya no eres DM o no es tu turno.");
            estaEnModoInvocacion = false;
            LimpiarCasillaAnterior();
            return;
        }

        if (punteroVR == null)
        {
            Debug.LogWarning("[DungeonMasterController] Falta asignar punteroVR en el Inspector.");
            return;
        }

        Ray ray = new Ray(punteroVR.position, punteroVR.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            CasillaComponent casillaDetectada =
                hit.collider.GetComponentInParent<CasillaComponent>();

            if (ultimaCasillaApuntada != null && ultimaCasillaApuntada != casillaDetectada)
            {
                LimpiarCasillaAnterior();
            }

            if (casillaDetectada != null)
            {
                bool esValida = EsCasillaValidaParaInvocacion(casillaDetectada);

                if (esValida)
                {
                    if (ultimaCasillaApuntada != casillaDetectada)
                    {
                        casillaDetectada.SetearEstadoVisual("DM_Target");
                        ultimaCasillaApuntada = casillaDetectada;

                        Debug.Log(
                            "[DungeonMasterController] Casilla válida apuntada: " +
                            casillaDetectada.name +
                            " | X = " + casillaDetectada.coordenadaX +
                            " | Z = " + casillaDetectada.coordenadaZ
                        );
                    }

                    bool gatilloApretado =
                        botonConfirmar != null &&
                        botonConfirmar.action.WasPressedThisFrame();

                    if (gatilloApretado)
                    {
                        SolicitarInvocacion(casillaDetectada);
                    }
                }
                else
                {
                    LimpiarCasillaAnterior();
                }
            }
        }
        else
        {
            LimpiarCasillaAnterior();
        }
    }

    public void AlternarModoInvocacion()
    {
        if (!PuedeUsarModoInvocacionLocal())
        {
            Debug.LogWarning("[DungeonMasterController] No puedes activar invocación. No eres DM o no es tu turno.");
            return;
        }

        estaEnModoInvocacion = !estaEnModoInvocacion;

        if (estaEnModoInvocacion)
        {
            Debug.Log("[DungeonMasterController] Modo Invocación ACTIVADO.");
        }
        else
        {
            Debug.Log("[DungeonMasterController] Modo Invocación CANCELADO.");
            LimpiarCasillaAnterior();
        }
    }

    private bool PuedeUsarModoInvocacionLocal()
    {
        if (GameplayManager.Instance == null)
            return false;

        if (TurnManager.Instance == null)
            return false;

        if (GameplayManager.Instance.LocalPlayerRole != "Dungeon Master")
            return false;

        if (!TurnManager.Instance.IsMyTurn())
            return false;

        return true;
    }

    private bool EsCasillaValidaParaInvocacion(CasillaComponent casilla)
    {
        if (casilla == null)
            return false;

        bool esValida =
            !casilla.esObstaculo &&
            !casilla.estaOcupada &&
            casilla.estaEnNiebla;

        return esValida;
    }

    private void SolicitarInvocacion(CasillaComponent casilla)
    {
        if (casilla == null)
            return;

        if (Runner == null)
        {
            Debug.LogError("[DungeonMasterController] No hay Runner. No se puede invocar enemigo en red.");
            return;
        }

        Debug.Log(
            "[DungeonMasterController] Solicitando invocación al host. " +
            "Solicitante = " + Runner.LocalPlayer +
            " | Casilla = " + casilla.name +
            " | X = " + casilla.coordenadaX +
            " | Z = " + casilla.coordenadaZ
        );

        RPC_RequestInvocarEnemigo(
            Runner.LocalPlayer,
            casilla.coordenadaX,
            casilla.coordenadaZ
        );
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestInvocarEnemigo(PlayerRef requester, int x, int z)
    {
        Debug.Log(
            "[DungeonMasterController HOST] Petición recibida para invocar enemigo. " +
            "Requester = " + requester +
            " | X = " + x +
            " | Z = " + z
        );

        if (!EsTurnoDePlayer(requester))
        {
            Debug.LogWarning(
                "[DungeonMasterController HOST] Invocación rechazada. No es turno de " +
                requester
            );
            return;
        }

        string rolSolicitante = ObtenerRolDePlayer(requester);

        if (rolSolicitante != "Dungeon Master")
        {
            Debug.LogWarning(
                "[DungeonMasterController HOST] Invocación rechazada. " +
                "El requester no es Dungeon Master. Rol = " + rolSolicitante
            );
            return;
        }

        if (GridManager.Instance == null)
        {
            Debug.LogError("[DungeonMasterController HOST] No existe GridManager.");
            return;
        }

        Vector2Int coord = new Vector2Int(x, z);

        if (!GridManager.Instance.DiccionarioTablero.TryGetValue(coord, out CasillaComponent casilla))
        {
            Debug.LogWarning("[DungeonMasterController HOST] No existe casilla en coordenada: " + coord);
            return;
        }

        if (!EsCasillaValidaParaInvocacion(casilla))
        {
            Debug.LogWarning(
                "[DungeonMasterController HOST] Casilla inválida para invocar. " +
                "Ocupada = " + casilla.estaOcupada +
                " | Obstáculo = " + casilla.esObstaculo +
                " | EnNiebla = " + casilla.estaEnNiebla
            );
            return;
        }

        NetworkObject enemigo = Runner.Spawn(
            prefabEnemigo,
            casilla.ObtenerCentro(),
            Quaternion.identity,
            null,
            (runner, obj) =>
            {
                FichaEnemigoAI ia = obj.GetComponent<FichaEnemigoAI>();

                if (ia != null)
                {
                    ia.ConfigurarInicialEnRed(x, z);
                }
                else
                {
                    Debug.LogWarning("[DungeonMasterController HOST] El prefab enemigo no tiene FichaEnemigoAI.");
                }
            }
        );

        casilla.estaOcupada = true;

        Debug.Log(
            "[DungeonMasterController HOST] Enemigo spawneado en red. " +
            "NetworkObject = " + enemigo.name +
            " | X = " + x +
            " | Z = " + z
        );

        if (GridManager.Instance != null)
        {
            GridManager.Instance.ActualizarNieblaDeGuerraGlobal();
        }

        RPC_NotificarInvocacionRealizada(x, z);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_NotificarInvocacionRealizada(int x, int z)
    {
        Debug.Log(
            "[DungeonMasterController TODOS] Enemigo invocado y sincronizado. " +
            "X = " + x +
            " | Z = " + z
        );

        estaEnModoInvocacion = false;
        LimpiarCasillaAnterior();

        if (GridManager.Instance != null)
        {
            GridManager.Instance.ActualizarNieblaDeGuerraGlobal();
        }
    }

    private bool EsTurnoDePlayer(PlayerRef player)
    {
        if (GameplayManager.Instance == null)
            return false;

        if (TurnManager.Instance == null)
            return false;

        if (GameplayManager.Instance.TurnOrder == null ||
            GameplayManager.Instance.TurnOrder.Count == 0)
            return false;

        int index = TurnManager.Instance.CurrentTurnIndex;

        if (index < 0 || index >= GameplayManager.Instance.TurnOrder.Count)
            index = 0;

        return GameplayManager.Instance.TurnOrder[index].PlayerRef == player;
    }

    private string ObtenerRolDePlayer(PlayerRef player)
    {
        if (GameplayManager.Instance != null &&
            GameplayManager.Instance.TurnOrder != null)
        {
            foreach (GameplayRoleCache.PlayerInfo info in GameplayManager.Instance.TurnOrder)
            {
                if (info.PlayerRef == player)
                    return info.PlayerRole;
            }
        }

        foreach (GameplayRoleCache.PlayerInfo info in GameplayRoleCache.Players)
        {
            if (info.PlayerRef == player)
                return info.PlayerRole;
        }

        return "Desconocido";
    }

    private void LimpiarCasillaAnterior()
    {
        if (ultimaCasillaApuntada != null)
        {
            ultimaCasillaApuntada.SetearEstadoVisual("Niebla");
            ultimaCasillaApuntada = null;
        }
    }
}