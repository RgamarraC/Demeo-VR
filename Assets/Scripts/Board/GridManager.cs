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
