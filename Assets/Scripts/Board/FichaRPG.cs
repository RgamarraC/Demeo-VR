using UnityEngine;

public class FichaRPG : MonoBehaviour
{
    [Header("Posicionamiento y Memoria")]
    public CasillaComponent casillaActual;
    [SerializeField] private CasillaComponent casillaPrevisualizada;

    [Header("Interacción y Física VR")]
    [SerializeField] private bool estaSiendoSostenida;
    [SerializeField] private LayerMask capaTablero;
    
    [Header("Pruebas Locales")]
    [SerializeField] private bool ignorarValidacionMultijugador = false;
    
    [Header("Restricciones")]
    public int rangoMovimiento = 3;

    [Header("Propiedad y Turnos")]
    [Tooltip("Valores válidos: 'Heroe 1', 'Heroe 2', 'Dungeon Master'")]
    [SerializeField] private string rolPropietario;

    private Rigidbody rb;
    private GridManager gridManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        gridManager = FindFirstObjectByType<GridManager>();
    }

    private void Update()
    {
        if (estaSiendoSostenida)
        {
            // Lanza el Raycast hacia abajo buscando casillas del tablero (con distancia infinita)
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, capaTablero))
            {
                CasillaComponent casillaDetectada = hit.collider.GetComponent<CasillaComponent>();

                bool permitida = false;
                if (gridManager != null && casillaDetectada != null)
                {
                    permitida = gridManager.EsCasillaValida(casillaDetectada);
                }

                if (casillaDetectada != null && permitida && (!casillaDetectada.estaOcupada || casillaDetectada == casillaActual))
                {
                    if (casillaPrevisualizada != null && casillaPrevisualizada != casillaDetectada)
                    {
                        casillaPrevisualizada.SetearEstadoVisual("EnRango");
                    }

                    casillaPrevisualizada = casillaDetectada;
                    casillaPrevisualizada.SetearEstadoVisual("Hover");
                }
                else
                {
                    // Si salimos de la zona de casillas, el raycast no golpea nada, o la casilla ESTÁ OCUPADA
                    if (casillaPrevisualizada != null)
                    {
                        casillaPrevisualizada.SetearEstadoVisual("EnRango");
                        casillaPrevisualizada = null;
                    }
                }
            }
            else
            {
                // Si el raycast no golpea absolutamente nada
                if (casillaPrevisualizada != null)
                {
                    casillaPrevisualizada.SetearEstadoVisual("EnRango");
                    casillaPrevisualizada = null;
                }
            }
        }
    }

    [ContextMenu("Levantar Ficha (Simular)")]
    public void AlSerLevantada()
    {
        // === VALIDACIÓN MULTIJUGADOR ===
        // Bypass para Test Local: Si existe GameplayManager y TurnManager, verificamos las reglas de red
        if (!ignorarValidacionMultijugador && GameplayManager.Instance != null && TurnManager.Instance != null)
        {
            // 1. ¿Es mi turno?
            if (!TurnManager.Instance.IsMyTurn())
            {
                Debug.LogWarning($"[FichaRPG] Movimiento cancelado. No es tu turno.");
                return;
            }

            // 2. ¿Soy el dueño de esta ficha?
            if (rolPropietario != GameplayManager.Instance.LocalPlayerRole)
            {
                Debug.LogWarning($"[FichaRPG] Agarre cancelado. Esta ficha le pertenece al rol: {rolPropietario}. Tu rol es: {GameplayManager.Instance.LocalPlayerRole}.");
                return;
            }
        }
        // ===============================

        if (rb != null) rb.isKinematic = false;

        estaSiendoSostenida = true;
        // Ocupación Atómica: NO alteramos casillaActual.estaOcupada, sigue siendo de nuestra propiedad.

        Debug.Log($"[FichaRPG] Ficha levantada. GridManager: {(GridManager.Instance != null ? "OK" : "NULL")}, casillaActual: {(casillaActual != null ? casillaActual.name : "NULL")}");

        if (GridManager.Instance != null && casillaActual != null)
        {
            // Calculamos el rango partiendo desde nuestra propia casillaActual
            GridManager.Instance.MostrarRangoMovimiento(casillaActual, rangoMovimiento);
        }
    }

    [ContextMenu("Soltar Ficha (Simular)")]
    public void AlSerSoltada()
    {
        estaSiendoSostenida = false;

        bool casillaEnRango = false;
        if (gridManager != null && casillaPrevisualizada != null)
        {
            casillaEnRango = gridManager.EsCasillaValida(casillaPrevisualizada);
        }

        // Apagamos el tablero visualmente DESPUÉS de validar, porque OcultarRango limpia la lista de casillas válidas
        if (GridManager.Instance != null)
        {
            GridManager.Instance.OcultarRangoMovimiento();
        }

        // Caso Éxito (Nueva Casilla Válida): En rango, libre y distinta a la actual
        if (casillaPrevisualizada != null && casillaEnRango && !casillaPrevisualizada.estaOcupada && casillaPrevisualizada != casillaActual)
        {
            // ColocarEnCasilla se encarga de liberar la actual, ocupar la nueva, y magnetizar.
            ColocarEnCasilla(casillaPrevisualizada);
        }
        // Caso Fallido / Cancelado (Fuera del tablero, obstáculo o soltada sobre sí misma)
        else
        {
            // Mantiene casillaActual.estaOcupada = true y regresa al origen sin penalización
            if (casillaActual != null)
            {
                transform.position = casillaActual.ObtenerCentro();
                transform.rotation = Quaternion.identity;
                if (rb != null) rb.isKinematic = true;
            }
        }
        
        casillaPrevisualizada = null;
    }

    public void ColocarEnCasilla(CasillaComponent nuevaCasilla)
    {
        if (nuevaCasilla == null) return;

        if (casillaActual != null)
        {
            casillaActual.estaOcupada = false;
        }

        casillaActual = nuevaCasilla;
        casillaActual.estaOcupada = true;
        
        transform.position = casillaActual.ObtenerCentro();
        transform.rotation = Quaternion.identity; // Restablecer rotación para que quede perfectamente derecha

        if (rb != null) rb.isKinematic = true;
    }

    public void ColocarEnCasillaInicial(CasillaComponent nuevaCasilla)
    {
        ColocarEnCasilla(nuevaCasilla);
    }
}
