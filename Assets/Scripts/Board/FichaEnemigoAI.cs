using UnityEngine;
using DemeoVR.Gameplay;
using System.Linq;
using Fusion;

public class FichaEnemigoAI : NetworkBehaviour
{
    [Header("Estadísticas (ScriptableObject)")]
    [SerializeField] private PieceData statsData;

    [Header("Restricciones")]
    [Tooltip("Define tanto el rango de visión como el movimiento del enemigo.")]
    public int rangoMovimiento = 3;

    [Header("Estado en Red")]
    [Networked] public int CoordenadaX { get; set; }
    [Networked] public int CoordenadaZ { get; set; }
    [Networked] public int VidaActual { get; set; }
    [Networked] public bool Inicializado { get; set; }

    // Compatibilidad con scripts que aún usan nombres antiguos
    public int coordenadaX
    {
        get { return CoordenadaX; }
        set { CoordenadaX = value; }
    }

    public int coordenadaZ
    {
        get { return CoordenadaZ; }
        set { CoordenadaZ = value; }
    }

    private bool tieneCoordRegistrada = false;
    private Vector2Int ultimaCoordRegistrada;

    public override void Spawned()
    {
        Debug.Log(
            "[FichaEnemigoAI] Spawned en red. " +
            "Nombre = " + gameObject.name +
            " | StateAuthority = " + Object.HasStateAuthority +
            " | InputAuthority = " + Object.HasInputAuthority
        );

        if (Object.HasStateAuthority && VidaActual <= 0)
        {
            VidaActual = ObtenerVidaBase();
        }

        SincronizarRegistroLocalConGrid();
    }

    private void Update()
    {
        if (!Inicializado)
            return;

        SincronizarRegistroLocalConGrid();
    }

    public void ConfigurarInicialEnRed(int x, int z)
    {
        CoordenadaX = x;
        CoordenadaZ = z;
        VidaActual = ObtenerVidaBase();
        Inicializado = true;

        Debug.Log(
            "[FichaEnemigoAI HOST] Configuración inicial asignada. " +
            "X = " + CoordenadaX +
            " | Z = " + CoordenadaZ +
            " | Vida = " + VidaActual
        );
    }

    private int ObtenerVidaBase()
    {
        if (statsData != null)
            return statsData.maxHealth;

        return 6;
    }

    private void SincronizarRegistroLocalConGrid()
    {
        if (GridManager.Instance == null)
            return;

        Vector2Int coordActual = new Vector2Int(CoordenadaX, CoordenadaZ);

        if (tieneCoordRegistrada && ultimaCoordRegistrada == coordActual)
            return;

        if (tieneCoordRegistrada)
        {
            if (GridManager.Instance.DiccionarioTablero.TryGetValue(
                    ultimaCoordRegistrada,
                    out CasillaComponent casillaAnterior))
            {
                casillaAnterior.estaOcupada = false;
            }
        }

        if (GridManager.Instance.DiccionarioTablero.TryGetValue(
                coordActual,
                out CasillaComponent nuevaCasilla))
        {
            nuevaCasilla.estaOcupada = true;

            transform.position = nuevaCasilla.ObtenerCentro();
            transform.rotation = Quaternion.identity;

            ultimaCoordRegistrada = coordActual;
            tieneCoordRegistrada = true;

            Debug.Log(
                "[FichaEnemigoAI] Registro local actualizado en grid. " +
                "Coord = " + coordActual +
                " | Vida = " + VidaActual +
                " | StateAuthority = " + Object.HasStateAuthority
            );

            GridManager.Instance.ActualizarNieblaDeGuerraGlobal();
        }
        else
        {
            Debug.LogWarning("[FichaEnemigoAI] No se encontró casilla para coord: " + coordActual);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (GridManager.Instance != null && tieneCoordRegistrada)
        {
            if (GridManager.Instance.DiccionarioTablero.TryGetValue(
                    ultimaCoordRegistrada,
                    out CasillaComponent casilla))
            {
                casilla.estaOcupada = false;
            }

            GridManager.Instance.ActualizarNieblaDeGuerraGlobal();
        }

        Debug.Log("[FichaEnemigoAI] Despawned. Enemigo eliminado de la red.");
    }

    [ContextMenu("Simular Turno IA")]
    public void EjecutarTurnoIA()
    {
        if (Object != null && !Object.HasStateAuthority)
        {
            Debug.Log("[FichaEnemigoAI] IA ignorada en cliente. Solo el host ejecuta la IA.");
            return;
        }

        Debug.Log("[FichaEnemigoAI HOST] Turno de IA iniciado para " + gameObject.name);

        if (GridManager.Instance == null)
        {
            Debug.LogWarning("[FichaEnemigoAI HOST] No hay GridManager.");
            return;
        }

        FichaRPG[] todosLosHeroes =
            FindObjectsByType<FichaRPG>(FindObjectsSortMode.None)
            .Where(f => f.esHeroe && f.casillaActual != null)
            .ToArray();

        if (todosLosHeroes.Length == 0)
        {
            Debug.Log("[FichaEnemigoAI HOST] No se encontraron héroes.");
            return;
        }

        FichaRPG heroeObjetivo = null;
        int distanciaMinima = int.MaxValue;

        foreach (FichaRPG heroe in todosLosHeroes)
        {
            int distancia =
                Mathf.Abs(CoordenadaX - heroe.casillaActual.coordenadaX) +
                Mathf.Abs(CoordenadaZ - heroe.casillaActual.coordenadaZ);

            if (distancia <= rangoMovimiento && distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                heroeObjetivo = heroe;
            }
        }

        if (heroeObjetivo == null)
        {
            Debug.Log("[FichaEnemigoAI HOST] Ningún héroe en rango de visión.");
            return;
        }

        Debug.Log(
            "[FichaEnemigoAI HOST] Héroe objetivo: " +
            heroeObjetivo.name +
            " | Distancia = " + distanciaMinima
        );

        Vector2Int coordHeroe =
            new Vector2Int(
                heroeObjetivo.casillaActual.coordenadaX,
                heroeObjetivo.casillaActual.coordenadaZ
            );

        Vector2Int[] direccionesAdyacentes = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };

        CasillaComponent casillaElegida = null;
        int distanciaHaciaCasillaElegida = int.MaxValue;

        foreach (Vector2Int dir in direccionesAdyacentes)
        {
            Vector2Int coordAdyacente = coordHeroe + dir;

            if (GridManager.Instance.DiccionarioTablero.TryGetValue(
                    coordAdyacente,
                    out CasillaComponent casillaVecina))
            {
                if (!casillaVecina.esObstaculo && !casillaVecina.estaOcupada)
                {
                    int distanciaOrcoACasilla =
                        Mathf.Abs(CoordenadaX - casillaVecina.coordenadaX) +
                        Mathf.Abs(CoordenadaZ - casillaVecina.coordenadaZ);

                    if (distanciaOrcoACasilla < distanciaHaciaCasillaElegida)
                    {
                        distanciaHaciaCasillaElegida = distanciaOrcoACasilla;
                        casillaElegida = casillaVecina;
                    }
                }
            }
        }

        if (casillaElegida != null && distanciaHaciaCasillaElegida <= rangoMovimiento)
        {
            CoordenadaX = casillaElegida.coordenadaX;
            CoordenadaZ = casillaElegida.coordenadaZ;

            transform.position = casillaElegida.ObtenerCentro();

            SincronizarRegistroLocalConGrid();

            Debug.Log(
                "[FichaEnemigoAI HOST] Enemigo movido en red a " +
                "(" + CoordenadaX + ", " + CoordenadaZ + ")"
            );
        }
        else
        {
            Debug.Log("[FichaEnemigoAI HOST] No hay casilla válida para moverse.");
        }
    }

    public void RequestDamage(int damage)
    {
        RPC_RequestDamage(damage);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestDamage(int damage)
    {
        VidaActual -= damage;

        Debug.Log(
            "[FichaEnemigoAI HOST] Enemigo recibió daño. " +
            "Daño = " + damage +
            " | Vida restante = " + VidaActual
        );

        if (VidaActual <= 0)
        {
            VidaActual = 0;

            Debug.Log("[FichaEnemigoAI HOST] Vida llegó a 0. Despawneando enemigo.");

            Runner.Despawn(Object);
        }
    }
}