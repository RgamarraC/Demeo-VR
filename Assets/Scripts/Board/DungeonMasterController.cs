using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;
using System.Collections.Generic;
using DemeoVR.Gameplay;
public class DungeonMasterController : NetworkBehaviour
{
    [System.Serializable]
    private class TrapInfo
    {
        public int x;
        public int z;
        public GameObject visual;
        public bool active;
    }

    [Header("Configuración de Invocación")]
    [Tooltip("Prefab del monstruo registrado en Fusion. Debe tener NetworkObject.")]
    [SerializeField] private NetworkPrefabRef prefabEnemigo;
    [Header("Configuración de Trampas")]
    [SerializeField] private GameObject prefabTrampaVisual;
    [SerializeField] private int dañoTrampa = 20;
    [SerializeField] private float alturaTrampa = 0.01f;
    [SerializeField] private float intervaloRevisionTrampa = 0.25f;

    [Header("Estado Interno de Trampa")]
    [SerializeField] private bool estaEnModoTrampa = false;
    private List<TrapInfo> trampasActivas = new List<TrapInfo>();

    private GameObject trampaVisualLocal;
    private float timerRevisionTrampa = 0f;


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
        RevisarTrampasHost();

        if (!estaEnModoInvocacion && !estaEnModoTrampa)
            return;

        if (!PuedeUsarModoInvocacionLocal())
        {
            Debug.LogWarning("[DungeonMasterController] Modo cancelado. Ya no eres DM o no es tu turno.");
            estaEnModoInvocacion = false;
            estaEnModoTrampa = false;
            LimpiarCasillaAnterior();
            return;
        }

        if (punteroVR == null)
        {
            Debug.LogWarning("[DungeonMasterController] Falta asignar punteroVR en el Inspector.");
            return;
        }

        Ray ray = new Ray(punteroVR.position, punteroVR.forward);
        CasillaComponent casillaDetectada = null;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            casillaDetectada = hit.collider.GetComponentInParent<CasillaComponent>();
        }

        // Si la casilla apuntada cambió, limpiamos la anterior
        if (ultimaCasillaApuntada != null && ultimaCasillaApuntada != casillaDetectada)
        {
            LimpiarCasillaAnterior();
        }

        if (casillaDetectada != null)
        {
            bool esValida = estaEnModoInvocacion ? EsCasillaValidaParaInvocacion(casillaDetectada) : EsCasillaValidaParaTrampa(casillaDetectada);

            if (esValida)
            {
                if (ultimaCasillaApuntada != casillaDetectada)
                {
                    casillaDetectada.SetearEstadoVisual("DM_Target");
                    ultimaCasillaApuntada = casillaDetectada;

                    Debug.Log(
                        $"[DungeonMasterController] Casilla válida apuntada: {casillaDetectada.name} | X = {casillaDetectada.coordenadaX} | Z = {casillaDetectada.coordenadaZ}"
                    );
                }

                if (botonConfirmar != null && botonConfirmar.action.WasPressedThisFrame())
                {
                    if (estaEnModoInvocacion)
                        SolicitarInvocacion(casillaDetectada);
                    else if (estaEnModoTrampa)
                        SolicitarTrampa(casillaDetectada);
                }
            }
            else
            {
                LimpiarCasillaAnterior();
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
            estaEnModoTrampa = false;
            Debug.Log("[DungeonMasterController] Modo Invocación ACTIVADO.");
        }
        else
        {
            Debug.Log("[DungeonMasterController] Modo Invocación CANCELADO.");
            LimpiarCasillaAnterior();
        }
    }

    public void AlternarModoTrampa()
    {
        if (!PuedeUsarModoInvocacionLocal())
        {
            Debug.LogWarning("[DungeonMasterController] No puedes activar trampa. No eres DM o no es tu turno.");
            return;
        }

        estaEnModoTrampa = !estaEnModoTrampa;

        if (estaEnModoTrampa)
        {
            estaEnModoInvocacion = false;
            Debug.Log("[DungeonMasterController] Modo Trampa ACTIVADO.");
        }
        else
        {
            Debug.Log("[DungeonMasterController] Modo Trampa CANCELADO.");
            LimpiarCasillaAnterior();
        }
    }

    private bool EsCasillaBaseValida(CasillaComponent casilla)
    {
        return casilla != null &&
               !casilla.esObstaculo &&
               !casilla.estaOcupada &&
               casilla.estaEnNiebla;
    }

    private bool EsCasillaValidaParaTrampa(CasillaComponent casilla)
    {
        if (!EsCasillaBaseValida(casilla))
            return false;

        return !ExisteTrampaEnCasilla(casilla.coordenadaX, casilla.coordenadaZ);
    }

    private bool ExisteTrampaEnCasilla(int x, int z)
    {
        foreach (TrapInfo trap in trampasActivas)
        {
            if (trap != null && trap.active && trap.x == x && trap.z == z)
                return true;
        }
        return false;
    }

    private void SolicitarTrampa(CasillaComponent casilla)
    {
        if (casilla == null)
            return;

        if (Runner == null)
        {
            Debug.LogError("[DungeonMasterController] No hay Runner. No se puede colocar trampa en red.");
            return;
        }

        Debug.Log(
            "[DungeonMasterController] Solicitando colocar trampa al host. " +
            "Solicitante = " + Runner.LocalPlayer +
            " | Casilla = " + casilla.name +
            " | X = " + casilla.coordenadaX +
            " | Z = " + casilla.coordenadaZ
        );

        RPC_RequestColocarTrampa(
            Runner.LocalPlayer,
            casilla.coordenadaX,
            casilla.coordenadaZ
        );
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestColocarTrampa(PlayerRef requester, int x, int z)
    {
        Debug.Log($"[DungeonMasterController HOST] Petición recibida para colocar trampa. Requester = {requester} | X = {x} | Z = {z}");

        if (!ValidarPeticionDungeonMaster(requester, "Colocar Trampa"))
            return;

        if (!TryObtenerCasilla(x, z, out CasillaComponent casilla))
            return;

        if (!EsCasillaValidaParaTrampa(casilla))
        {
            Debug.LogWarning(
                $"[DungeonMasterController HOST] Casilla inválida para colocar trampa. Ocupada = {casilla.estaOcupada} | Obstáculo = {casilla.esObstaculo} | EnNiebla = {casilla.estaEnNiebla}"
            );
            return;
        }

        Vector3 posicion = casilla.ObtenerCentro() + Vector3.up * alturaTrampa;

        Debug.Log($"[DungeonMasterController HOST] Trampa colocada. X = {x} | Z = {z}");

        RPC_NotificarTrampaColocada(x, z, posicion);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_NotificarTrampaColocada(int x, int z, Vector3 posicion)
    {
        Debug.Log(
            "[DungeonMasterController TODOS] Trampa registrada. " +
            "X = " + x +
            " | Z = " + z
        );

        estaEnModoTrampa = false;
        LimpiarCasillaAnterior();

        bool soyDM =
            GameplayManager.Instance != null &&
            GameplayManager.Instance.LocalPlayerRole == "Dungeon Master";

        GameObject visual = null;

        if (soyDM && prefabTrampaVisual != null)
        {
            visual = Instantiate(prefabTrampaVisual, posicion, Quaternion.identity);
            visual.SetActive(true);

            Debug.Log("[DungeonMasterController TODOS] Trampa visible para el DM.");
        }
        else
        {
            Debug.Log("[DungeonMasterController TODOS] Trampa oculta para este jugador.");
        }

        TrapInfo nuevaTrampa = new TrapInfo
        {
            x = x,
            z = z,
            visual = visual,
            active = true
        };

        trampasActivas.Add(nuevaTrampa);
    }
    private void RevisarTrampasHost()
    {
        if (!Object.HasStateAuthority)
            return;

        if (trampasActivas.Count == 0)
            return;

        if (GameEndManager.Instance != null &&
            GameEndManager.Instance.JuegoTerminado)
            return;

        timerRevisionTrampa += Time.deltaTime;

        if (timerRevisionTrampa < intervaloRevisionTrampa)
            return;

        timerRevisionTrampa = 0f;

        FichaRPG[] fichas =
            FindObjectsByType<FichaRPG>(FindObjectsSortMode.None);

        foreach (FichaRPG ficha in fichas)
        {
            if (ficha == null)
                continue;

            if (!ficha.esHeroe)
                continue;

            if (ficha.casillaActual == null)
                continue;

            BoardPiece statsHeroe = ObtenerBoardPiece(ficha);

            if (statsHeroe == null)
                continue;

            if (statsHeroe.CurrentHealth <= 0)
                continue;

            for (int i = 0; i < trampasActivas.Count; i++)
            {
                TrapInfo trap = trampasActivas[i];

                if (trap == null || !trap.active)
                    continue;

                bool pisaTrampa =
                    ficha.casillaActual.coordenadaX == trap.x &&
                    ficha.casillaActual.coordenadaZ == trap.z;

                if (!pisaTrampa)
                    continue;

                trap.active = false;

                Debug.Log(
                    "[DungeonMasterController HOST] Héroe pisó trampa. Iniciando secuencia de daño. " +
                    "Ficha = " + ficha.name +
                    " | Rol = " + ficha.RolPropietario +
                    " | X = " + trap.x +
                    " | Z = " + trap.z
                );

                StartCoroutine(SecuenciaActivacionTrampa(trap, statsHeroe, ficha.RolPropietario));
                return;
            }
        }
    }

    private System.Collections.IEnumerator SecuenciaActivacionTrampa(TrapInfo trap, BoardPiece statsHeroe, string rolHeroe)
    {
        // 1. Notificar a todos los clientes que la trampa se activó (esto la hace visible para el jugador/héroe)
        RPC_NotificarTrampaActivada(trap.x, trap.z, rolHeroe);

        // 2. Esperar 1 segundo para que el jugador pueda ver la trampa antes de recibir el daño
        yield return new WaitForSeconds(1.0f);

        // 3. Efectuar el daño si el héroe aún es válido
        if (statsHeroe != null && statsHeroe.CurrentHealth > 0)
        {
            statsHeroe.TakeDamage(dañoTrampa, 0);
            Debug.Log($"[DungeonMasterController HOST] Daño de trampa ({dañoTrampa}) aplicado a {rolHeroe}.");
        }

        // 4. Esperar un poco más (0.8s) para que se aprecie la trampa y el cambio en la salud del jugador
        yield return new WaitForSeconds(0.8f);

        // 5. Notificar a todos para destruir/ocultar el visual de la trampa
        RPC_DesvanecerTrampaVisual(trap.x, trap.z);
    }

    private TrapInfo ObtenerTrapInfoLocal(int x, int z)
    {
        foreach (TrapInfo trap in trampasActivas)
        {
            if (trap != null && trap.x == x && trap.z == z)
                return trap;
        }
        return null;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_NotificarTrampaActivada(int x, int z, string rolHeroe)
    {
        Vector3 posicion = Vector3.zero;
        if (GridManager.Instance != null && GridManager.Instance.DiccionarioTablero.TryGetValue(new Vector2Int(x, z), out CasillaComponent casilla))
        {
            posicion = casilla.ObtenerCentro() + Vector3.up * alturaTrampa;
        }

        TrapInfo trap = ObtenerTrapInfoLocal(x, z);
        if (trap == null)
        {
            // Fallback en caso de desincronización
            GameObject tempVisual = null;
            if (prefabTrampaVisual != null && posicion != Vector3.zero)
            {
                tempVisual = Instantiate(prefabTrampaVisual, posicion, Quaternion.identity);
                tempVisual.SetActive(true);
            }

            trap = new TrapInfo
            {
                x = x,
                z = z,
                visual = tempVisual,
                active = false
            };
            trampasActivas.Add(trap);
        }
        else
        {
            if (trap.visual == null && prefabTrampaVisual != null && posicion != Vector3.zero)
            {
                trap.visual = Instantiate(prefabTrampaVisual, posicion, Quaternion.identity);
                trap.visual.SetActive(true);
            }
        }

        Debug.Log($"[DungeonMasterController TODOS] Trampa en X = {x}, Z = {z} activada por {rolHeroe}. Ahora es visible.");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_DesvanecerTrampaVisual(int x, int z)
    {
        for (int i = trampasActivas.Count - 1; i >= 0; i--)
        {
            TrapInfo trap = trampasActivas[i];
            if (trap == null)
                continue;

            if (trap.x == x && trap.z == z)
            {
                if (trap.visual != null)
                {
                    Destroy(trap.visual);
                    trap.visual = null;
                }
                trampasActivas.RemoveAt(i);
            }
        }

        Debug.Log($"[DungeonMasterController TODOS] Trampa en X = {x}, Z = {z} destruida y eliminada.");
    }

    private BoardPiece ObtenerBoardPiece(Component component)
    {

        if (component == null)
            return null;

        BoardPiece pieza = component.GetComponent<BoardPiece>();

        if (pieza == null)
            pieza = component.GetComponentInParent<BoardPiece>();

        if (pieza == null)
            pieza = component.GetComponentInChildren<BoardPiece>();

        return pieza;
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
        return EsCasillaBaseValida(casilla);
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
        Debug.Log($"[DungeonMasterController HOST] Petición recibida para invocar enemigo. Requester = {requester} | X = {x} | Z = {z}");

        if (!ValidarPeticionDungeonMaster(requester, "Invocar Enemigo"))
            return;

        if (!TryObtenerCasilla(x, z, out CasillaComponent casilla))
            return;

        if (!EsCasillaValidaParaInvocacion(casilla))
        {
            Debug.LogWarning(
                $"[DungeonMasterController HOST] Casilla inválida para invocar. Ocupada = {casilla.estaOcupada} | Obstáculo = {casilla.esObstaculo} | EnNiebla = {casilla.estaEnNiebla}"
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

        Debug.Log($"[DungeonMasterController HOST] Enemigo spawneado en red. NetworkObject = {enemigo.name} | X = {x} | Z = {z}");

        if (GridManager.Instance != null)
        {
            GridManager.Instance.ActualizarNieblaDeGuerraGlobal();
        }

        RPC_NotificarInvocacionRealizada(x, z);
    }

    private bool ValidarPeticionDungeonMaster(PlayerRef requester, string accion)
    {
        if (!EsTurnoDePlayer(requester))
        {
            Debug.LogWarning($"[DungeonMasterController HOST] Petición de {accion} rechazada. No es turno de {requester}.");
            return false;
        }

        string rolSolicitante = ObtenerRolDePlayer(requester);
        if (rolSolicitante != "Dungeon Master")
        {
            Debug.LogWarning($"[DungeonMasterController HOST] Petición de {accion} rechazada. El requester no es Dungeon Master. Rol = {rolSolicitante}");
            return false;
        }

        return true;
    }

    private bool TryObtenerCasilla(int x, int z, out CasillaComponent casilla)
    {
        casilla = null;
        if (GridManager.Instance == null)
        {
            Debug.LogError("[DungeonMasterController HOST] No existe GridManager.");
            return false;
        }

        Vector2Int coord = new Vector2Int(x, z);
        if (!GridManager.Instance.DiccionarioTablero.TryGetValue(coord, out casilla))
        {
            Debug.LogWarning($"[DungeonMasterController HOST] No existe casilla en coordenada: {coord}");
            return false;
        }

        return true;
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