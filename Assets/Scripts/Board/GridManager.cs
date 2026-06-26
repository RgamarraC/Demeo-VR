using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Referencias del Tablero")]
    [SerializeField] private Transform padreTablero;

    [Header("Configuración de Debug")]
    [SerializeField] private bool modoDebugSpawns;
    [SerializeField] private Color colorSpawn = Color.green;

    // Estructura de datos indexada (Diccionario) para acceder rápidamente por coordenadas
    private Dictionary<Vector2Int, CasillaComponent> diccionarioCasillas = new Dictionary<Vector2Int, CasillaComponent>();
    public Dictionary<Vector2Int, CasillaComponent> DiccionarioCasillas => diccionarioCasillas;

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
        ActualizarColoresDebug();
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
