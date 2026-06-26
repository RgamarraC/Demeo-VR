using UnityEngine;

public class FichaRPG : MonoBehaviour
{
    [Header("Posicionamiento y Memoria")]
    [SerializeField] private CasillaComponent casillaActual;
    [SerializeField] private CasillaComponent casillaAnterior;
    [SerializeField] private CasillaComponent casillaPrevisualizada;

    [Header("Interacción y Física VR")]
    [SerializeField] private bool estaSiendoSostenida;
    [SerializeField] private LayerMask capaTablero;
    [SerializeField] private Color colorPrevisualizacion = Color.yellow;
    
    [Header("Restricciones")]
    public int rangoMovimiento = 3;

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
            // Lanza el Raycast hacia abajo buscando casillas del tablero
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 10f, capaTablero))
            {
                CasillaComponent casillaDetectada = hit.collider.GetComponent<CasillaComponent>();

                bool enRango = false;
                if (gridManager != null && casillaAnterior != null && casillaDetectada != null)
                {
                    enRango = gridManager.EsCasillaEnRangoCircularGrid(casillaAnterior.CoordenadaGrid, casillaDetectada.CoordenadaGrid, rangoMovimiento);
                }

                if (casillaDetectada != null && !casillaDetectada.EstaOcupada && enRango)
                {
                    if (casillaPrevisualizada != null && casillaPrevisualizada != casillaDetectada)
                    {
                        casillaPrevisualizada.RestablecerColorOriginal();
                    }

                    casillaPrevisualizada = casillaDetectada;
                    casillaPrevisualizada.CambiarColor(colorPrevisualizacion);
                }
                else
                {
                    // Si salimos de la zona de casillas, el raycast no golpea nada, o la casilla ESTÁ OCUPADA
                    if (casillaPrevisualizada != null)
                    {
                        casillaPrevisualizada.RestablecerColorOriginal();
                        casillaPrevisualizada = null;
                    }
                }
            }
            else
            {
                // Si el raycast no golpea absolutamente nada
                if (casillaPrevisualizada != null)
                {
                    casillaPrevisualizada.RestablecerColorOriginal();
                    casillaPrevisualizada = null;
                }
            }
        }
    }

    [ContextMenu("Levantar Ficha (Simular)")]
    public void AlSerLevantada()
    {
        if (rb != null) rb.isKinematic = false;

        estaSiendoSostenida = true;
        casillaAnterior = casillaActual;

        if (casillaActual != null)
        {
            casillaActual.EstaOcupada = false;
            casillaActual = null;
        }
    }

    [ContextMenu("Soltar Ficha (Simular)")]
    public void AlSerSoltada()
    {
        estaSiendoSostenida = false;

        // Limpiamos el feedback visual previsualizado
        if (casillaPrevisualizada != null)
        {
            casillaPrevisualizada.RestablecerColorOriginal();
        }

        // CANDADO DE SEGURIDAD: Evita gasto de AP por soltado en el mismo lugar o fuera del tablero
        if (casillaPrevisualizada == casillaAnterior || casillaPrevisualizada == null)
        {
            if (casillaAnterior != null)
            {
                transform.position = casillaAnterior.ObtenerCentro();
                transform.rotation = Quaternion.identity; // Enderezar ficha al volver
                if (rb != null) rb.isKinematic = true;    // Anular fuerzas físicas residuales
                
                casillaActual = casillaAnterior;
                casillaActual.EstaOcupada = true;
            }
            
            casillaPrevisualizada = null;
            return; // Interrumpe y finaliza la lógica inmediatamente
        }

        // CANDADO DE SEGURIDAD (Extra): Si la casilla destino ya está ocupada o fuera de rango
        bool fueraDeRango = false;
        if (gridManager != null && casillaAnterior != null && casillaPrevisualizada != null)
        {
            fueraDeRango = !gridManager.EsCasillaEnRangoCircularGrid(casillaAnterior.CoordenadaGrid, casillaPrevisualizada.CoordenadaGrid, rangoMovimiento);
        }

        if (casillaPrevisualizada.EstaOcupada || fueraDeRango)
        {
            if (casillaAnterior != null)
            {
                transform.position = casillaAnterior.ObtenerCentro();
                transform.rotation = Quaternion.identity; // Enderezar ficha al volver
                if (rb != null) rb.isKinematic = true;    // Anular fuerzas físicas residuales
                
                casillaActual = casillaAnterior;
                casillaActual.EstaOcupada = true;
            }
            
            casillaPrevisualizada = null;
            return;
        }

        // Si es una casilla válida, aplicamos el movimiento final
        ColocarEnCasilla(casillaPrevisualizada);
        casillaPrevisualizada = null;
    }

    public void ColocarEnCasilla(CasillaComponent nuevaCasilla)
    {
        if (nuevaCasilla == null) return;

        if (casillaActual != null)
        {
            casillaActual.EstaOcupada = false;
        }

        casillaActual = nuevaCasilla;
        casillaActual.EstaOcupada = true;
        
        transform.position = casillaActual.ObtenerCentro();
        transform.rotation = Quaternion.identity; // Restablecer rotación para que quede perfectamente derecha

        if (rb != null) rb.isKinematic = true;
    }
}
