using System.Collections.Generic;
using UnityEngine;
using Fusion;
using DemeoVR.Gameplay;

public class KeyWinManager : NetworkBehaviour
{
    public static KeyWinManager Instance;

    [Header("Objeto visual de la llave")]
    [SerializeField] private GameObject keyObject;

    [Header("Configuración")]
    [SerializeField] private float alturaLlave = 0.5f;
    [SerializeField] private float intervaloRevision = 0.25f;

    [Networked] private int KeyX { get; set; }
    [Networked] private int KeyZ { get; set; }
    [Networked] private bool KeyPlaced { get; set; }

    private bool networkReady = false;
    private bool victoriaProcesada = false;
    private float timerRevision = 0f;

    private void Awake()
    {
        Instance = this;
    }

    public override void Spawned()
    {
        networkReady = true;

        if (keyObject != null)
            keyObject.SetActive(false);

        if (Object.HasStateAuthority)
        {
            KeyPlaced = false;
            victoriaProcesada = false;

            GenerarLlaveAleatoria();
        }

        Debug.Log(
            "[KeyWinManager] Spawned. StateAuthority = " +
            Object.HasStateAuthority
        );
    }

    private void Update()
    {
        if (!networkReady)
            return;

        if (!Object.HasStateAuthority)
            return;

        if (!KeyPlaced)
            return;

        if (victoriaProcesada)
            return;

        if (GameEndManager.Instance != null &&
            GameEndManager.Instance.JuegoTerminado)
            return;

        timerRevision += Time.deltaTime;

        if (timerRevision < intervaloRevision)
            return;

        timerRevision = 0f;

        RevisarSiHeroeTocoLlave();
    }

    private void GenerarLlaveAleatoria()
    {
        CasillaComponent[] casillas =
            FindObjectsByType<CasillaComponent>(FindObjectsSortMode.None);

        List<CasillaComponent> casillasValidas =
            new List<CasillaComponent>();

        foreach (CasillaComponent casilla in casillas)
        {
            if (casilla == null)
                continue;

            if (casilla.esObstaculo)
                continue;

            if (casilla.estaOcupada)
                continue;

            casillasValidas.Add(casilla);
        }

        if (casillasValidas.Count == 0)
        {
            Debug.LogError("[KeyWinManager HOST] No hay casillas válidas para colocar la llave.");
            return;
        }

        int index = Random.Range(0, casillasValidas.Count);
        CasillaComponent casillaElegida = casillasValidas[index];

        KeyX = casillaElegida.coordenadaX;
        KeyZ = casillaElegida.coordenadaZ;
        KeyPlaced = true;

        Vector3 posicionLlave =
            casillaElegida.ObtenerCentro() + Vector3.up * alturaLlave;

        RPC_MostrarLlave(posicionLlave);

        Debug.Log(
            "[KeyWinManager HOST] Llave generada automáticamente. " +
            "X = " + KeyX +
            " | Z = " + KeyZ +
            " | Posición = " + posicionLlave
        );
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_MostrarLlave(Vector3 posicion)
    {
        if (keyObject != null)
        {
            keyObject.transform.position = posicion;
            keyObject.SetActive(true);
        }

        Debug.Log(
            "[KeyWinManager TODOS] Llave mostrada en posición = " +
            posicion
        );
    }

    private void RevisarSiHeroeTocoLlave()
    {
        FichaRPG[] fichas =
            FindObjectsByType<FichaRPG>(FindObjectsSortMode.None);

        foreach (FichaRPG ficha in fichas)
        {
            if (ficha == null)
                continue;

            if (!ficha.esHeroe)
                continue;

            if (!EsHeroeActivo(ficha.RolPropietario))
                continue;

            if (ficha.casillaActual == null)
                continue;

            BoardPiece statsHeroe = ObtenerBoardPiece(ficha);

            if (statsHeroe != null && statsHeroe.CurrentHealth <= 0)
                continue;

            bool llegoALaLlave =
                ficha.casillaActual.coordenadaX == KeyX &&
                ficha.casillaActual.coordenadaZ == KeyZ;

            if (llegoALaLlave)
            {
                victoriaProcesada = true;

                Debug.Log(
                    "[KeyWinManager HOST] Héroe llegó a la llave. " +
                    "Ficha = " + ficha.name +
                    " | Rol = " + ficha.RolPropietario
                );

                RPC_OcultarLlave();

                if (GameEndManager.Instance != null)
                {
                    GameEndManager.Instance.TerminarJuegoHeroesGanan();
                }
                else
                {
                    Debug.LogWarning("[KeyWinManager HOST] No existe GameEndManager.");
                }

                return;
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OcultarLlave()
    {
        if (keyObject != null)
            keyObject.SetActive(false);

        Debug.Log("[KeyWinManager TODOS] Llave ocultada por victoria de héroes.");
    }

    private bool EsHeroeActivo(string rol)
    {
        if (GameplayManager.Instance == null)
            return false;

        if (GameplayManager.Instance.TurnOrder == null)
            return false;

        foreach (GameplayRoleCache.PlayerInfo info in GameplayManager.Instance.TurnOrder)
        {
            if (info.PlayerRole == rol)
                return true;
        }

        return false;
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
}