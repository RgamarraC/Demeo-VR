using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Referencias del Tablero")]
    [SerializeField] private Transform padreTablero;
    [SerializeField] private LayerMask capaParedes;

    [Header("Configuración de Debug")]
    [SerializeField] private bool modoDebugSpawns;
    [SerializeField] private Color colorSpawn = Color.green;
    [SerializeField] private Color colorRangoIluminado = Color.yellow; // Rango Válido
    
    [Header("Detección de Paredes (Bake Geométrico)")]
    [SerializeField] private float tamañoCasilla = 2f;
    [SerializeField] private float alturaRayoDeteccion = 0.5f;
    [SerializeField] private Vector3 tamañoSensorBorde = new Vector3(0.8f, 1f, 0.1f);

    public Color ColorRangoIluminado => colorRangoIluminado;

    // Estructura de datos indexada (Diccionario) para acceder rápidamente por coordenadas
    private Dictionary<Vector2Int, CasillaComponent> diccionarioCasillas = new Dictionary<Vector2Int, CasillaComponent>();
    public Dictionary<Vector2Int, CasillaComponent> DiccionarioCasillas => diccionarioCasillas;

    private List<CasillaComponent> casillasIluminadas = new List<CasillaComponent>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        EscanearTablero();
        DesplegarHeroesEnSpawns();
    }

    /// <summary>
    /// Devuelve true si el destino está dentro de un círculo matemático en una grilla cuadrada.
    /// Usa la fórmula de la distancia al cuadrado (Pitágoras) usando solo enteros para evitar problemas de precisión flotante.
    /// </summary>
    public bool EsCasillaEnRangoCircularGrid(Vector2Int origen, Vector2Int destino, int rangoMaximo)
    {
        int dx = Mathf.Abs(origen.x - destino.x);
        int dy = Mathf.Abs(origen.y - destino.y);
        return (dx * dx) + (dy * dy) <= (rangoMaximo * rangoMaximo);
    }

    public void MostrarRangoMovimiento(CasillaComponent origen, int rango)
    {
        OcultarRangoMovimiento(); // Limpiamos primero por seguridad

        if (origen == null) return;

        Queue<CasillaComponent> cola = new Queue<CasillaComponent>();
        HashSet<CasillaComponent> visitadas = new HashSet<CasillaComponent>();

        cola.Enqueue(origen);
        visitadas.Add(origen);

        Vector2Int[] direcciones = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // Arriba
            new Vector2Int(0, -1),  // Abajo
            new Vector2Int(1, 0),   // Derecha
            new Vector2Int(-1, 0),  // Izquierda
            new Vector2Int(1, 1),   // Diagonal Arriba-Derecha
            new Vector2Int(1, -1),  // Diagonal Abajo-Derecha
            new Vector2Int(-1, 1),  // Diagonal Arriba-Izquierda
            new Vector2Int(-1, -1)  // Diagonal Abajo-Izquierda
        };

        while (cola.Count > 0)
        {
            CasillaComponent actual = cola.Dequeue();

            foreach (var dir in direcciones)
            {
                // Validación Estricta de Muros (Evitar traspasar paredes)
                if (dir == new Vector2Int(0, 1) && actual.muroAlNorte) continue;
                if (dir == new Vector2Int(0, -1) && actual.muroAlSur) continue;
                if (dir == new Vector2Int(1, 0) && actual.muroAlEste) continue;
                if (dir == new Vector2Int(-1, 0) && actual.muroAlOeste) continue;

                // Validación de Muros en Diagonales (No permite cortar esquinas a través de un muro)
                if (dir == new Vector2Int(1, 1) && (actual.muroAlNorte || actual.muroAlEste)) continue;
                if (dir == new Vector2Int(1, -1) && (actual.muroAlSur || actual.muroAlEste)) continue;
                if (dir == new Vector2Int(-1, 1) && (actual.muroAlNorte || actual.muroAlOeste)) continue;
                if (dir == new Vector2Int(-1, -1) && (actual.muroAlSur || actual.muroAlOeste)) continue;

                Vector2Int coordVecina = actual.CoordenadaGrid + dir;

                if (diccionarioCasillas.TryGetValue(coordVecina, out CasillaComponent vecino))
                {
                    // Validación bidireccional preventiva (el vecino tampoco debe tener un muro hacia nosotros)
                    if (dir == new Vector2Int(0, 1) && vecino.muroAlSur) continue;
                    if (dir == new Vector2Int(0, -1) && vecino.muroAlNorte) continue;
                    if (dir == new Vector2Int(1, 0) && vecino.muroAlOeste) continue;
                    if (dir == new Vector2Int(-1, 0) && vecino.muroAlEste) continue;

                    if (!visitadas.Contains(vecino))
                    {
                        // 1. Verificar si está dentro del rango circular
                        if (EsCasillaEnRangoCircularGrid(origen.CoordenadaGrid, coordVecina, rango))
                        {
                            visitadas.Add(vecino);
                            cola.Enqueue(vecino);

                            // Si está libre de personajes, la agregamos como válida e iluminamos
                            if (!vecino.EstaOcupada && !casillasIluminadas.Contains(vecino))
                            {
                                vecino.CambiarColor(colorRangoIluminado);
                                casillasIluminadas.Add(vecino);
                            }
                        }
                    }
                }
            }
        }
    }

    public void OcultarRangoMovimiento()
    {
        foreach (var casilla in casillasIluminadas)
        {
            casilla.DesactivarEfectoLuz();
            casilla.RestablecerColorOriginal();
        }
        casillasIluminadas.Clear();
    }

    public bool EsCasillaValida(CasillaComponent casilla)
    {
        return casillasIluminadas.Contains(casilla);
    }

    private void DesplegarHeroesEnSpawns()
    {
        // En Unity 2023+, FindObjectsByType es la forma optimizada
        FichaRPG[] heroes = Object.FindObjectsByType<FichaRPG>(FindObjectsSortMode.None);
        
        // Obtenemos solo las casillas marcadas como spawn de héroes
        var casillasSpawn = diccionarioCasillas.Values.Where(c => c.EsSpawnHeroe).ToList();

        for (int i = 0; i < heroes.Length; i++)
        {
            if (i < casillasSpawn.Count)
            {
                heroes[i].ColocarEnCasilla(casillasSpawn[i]);
            }
            else
            {
                Debug.LogWarning($"[GridManager] No hay suficientes casillas de spawn para el héroe {heroes[i].gameObject.name}.");
            }
        }
    }

    [ContextMenu("Escanear Tablero")]
    public void EscanearTablero()
    {
        if (padreTablero == null)
        {
            Debug.LogWarning("[GridManager] No se ha asignado el Transform 'padreTablero'.");
            return;
        }

        CasillaComponent[] casillas = padreTablero.GetComponentsInChildren<CasillaComponent>();

        if (casillas.Length == 0) return;

        diccionarioCasillas.Clear();

        var posicionesX = casillas
            .Select(c => Mathf.RoundToInt(padreTablero.InverseTransformPoint(c.transform.position).x * 10f))
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var posicionesZ = casillas
            .Select(c => Mathf.RoundToInt(padreTablero.InverseTransformPoint(c.transform.position).z * 10f))
            .Distinct()
            .OrderBy(z => z)
            .ToList();

        foreach (var casilla in casillas)
        {
            Vector3 posRelativa = padreTablero.InverseTransformPoint(casilla.transform.position);
            
            int gridX = posicionesX.IndexOf(Mathf.RoundToInt(posRelativa.x * 10f));
            int gridY = posicionesZ.IndexOf(Mathf.RoundToInt(posRelativa.z * 10f));
            
            casilla.CoordenadaGrid = new Vector2Int(gridX, gridY);

            // Guardamos la casilla en el diccionario
            if (!diccionarioCasillas.ContainsKey(casilla.CoordenadaGrid))
            {
                diccionarioCasillas.Add(casilla.CoordenadaGrid, casilla);
            }
        }

        Debug.Log($"[GridManager] Tablero escaneado exitosamente. Se indexaron {diccionarioCasillas.Count} casillas.");
        
        BakarParedesDelTablero(); // Llamada al nuevo sistema geométrico de bordes
        
        ActualizarColoresDebug();
    }

    private void BakarParedesDelTablero()
    {
        foreach (var kvp in diccionarioCasillas)
        {
            CasillaComponent casilla = kvp.Value;
            Vector3 centro = casilla.ObtenerCentro();

            // Norte (+Z)
            Vector3 posNorte = centro + new Vector3(0, alturaRayoDeteccion, tamañoCasilla / 2f);
            casilla.muroAlNorte = DetectarMuroEnFrontera(posNorte, Quaternion.identity);

            // Sur (-Z)
            Vector3 posSur = centro + new Vector3(0, alturaRayoDeteccion, -tamañoCasilla / 2f);
            casilla.muroAlSur = DetectarMuroEnFrontera(posSur, Quaternion.identity);

            // Este (+X)
            Vector3 posEste = centro + new Vector3(tamañoCasilla / 2f, alturaRayoDeteccion, 0);
            casilla.muroAlEste = DetectarMuroEnFrontera(posEste, Quaternion.Euler(0, 90, 0));

            // Oeste (-X)
            Vector3 posOeste = centro + new Vector3(-tamañoCasilla / 2f, alturaRayoDeteccion, 0);
            casilla.muroAlOeste = DetectarMuroEnFrontera(posOeste, Quaternion.Euler(0, 90, 0));
        }

        Debug.Log($"[GridManager] Bordes de casillas (Muros) calculados geométricamente con éxito.");
    }

    private bool DetectarMuroEnFrontera(Vector3 posicion, Quaternion rotacion)
    {
        Collider[] choques = Physics.OverlapBox(posicion, tamañoSensorBorde / 2f, rotacion, capaParedes);
        
        foreach (Collider hit in choques)
        {
            // Filtro de Puertas: Si es una puerta, no se considera muro bloqueante
            if (hit.CompareTag("Door"))
            {
                continue; 
            }
            
            // Filtro adicional de seguridad para evitar detectar suelo o fichas
            if (hit.GetComponentInParent<CasillaComponent>() == null && hit.GetComponentInParent<FichaRPG>() == null)
            {
                return true; // Es un muro real bloqueante
            }
        }
        
        return false;
    }

    private void OnValidate()
    {
        ActualizarColoresDebug();
    }

    private void ActualizarColoresDebug()
    {
        if (padreTablero == null) return;

        CasillaComponent[] casillas = padreTablero.GetComponentsInChildren<CasillaComponent>();

        foreach (var casilla in casillas)
        {
            if (modoDebugSpawns && casilla.EsSpawnHeroe)
            {
                casilla.CambiarColor(colorSpawn);
            }
            else
            {
                casilla.RestablecerColorOriginal();
            }
        }
    }
}
