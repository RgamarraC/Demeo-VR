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
    [SerializeField] private Color colorPrevisualizacion = Color.green;
    
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
            // Lanza el Raycast hacia abajo buscando casillas del tablero
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 10f, capaTablero))
            {
                CasillaComponent casillaDetectada = hit.collider.GetComponent<CasillaComponent>();

                bool permitida = false;
                if (gridManager != null && casillaDetectada != null)
                {
                    permitida = gridManager.EsCasillaValida(casillaDetectada);
                }

                if (casillaDetectada != null && permitida)
                {
                    if (casillaPrevisualizada != null && casillaPrevisualizada != casillaDetectada)
                    {
                        casillaPrevisualizada.CambiarColor(gridManager.ColorRangoIluminado);
                    }

                    casillaPrevisualizada = casillaDetectada;
                    casillaPrevisualizada.CambiarColor(colorPrevisualizacion);
                }
                else
                {
                    // Si salimos de la zona de casillas, el raycast no golpea nada, o la casilla ESTÁ OCUPADA
                    if (casillaPrevisualizada != null)
                    {
                        casillaPrevisualizada.CambiarColor(gridManager.ColorRangoIluminado);
                        casillaPrevisualizada = null;
                    }
                }
            }
            else
            {
                // Si el raycast no golpea absolutamente nada
                if (casillaPrevisualizada != null)
                {
                    casillaPrevisualizada.CambiarColor(gridManager.ColorRangoIluminado);
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
        if (GameplayManager.Instance != null && TurnManager.Instance != null)
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
        casillaAnterior = casillaActual;

        if (casillaActual != null)
        {
            casillaActual.EstaOcupada = false;
            casillaActual = null;
        }

        if (GridManager.Instance != null && casillaAnterior != null)
        {
            GridManager.Instance.MostrarRangoMovimiento(casillaAnterior, rangoMovimiento);
        }
    }

    [ContextMenu("Soltar Ficha (Simular)")]
    public void AlSerSoltada()
    {
        estaSiendoSostenida = false;

        // CANDADO DE SEGURIDAD: Guardar la validación ANTES de ocultar el rango (porque Ocultar limpia la lista)
        bool casillaInvalida = true;
        if (gridManager != null && casillaPrevisualizada != null)
        {
            casillaInvalida = !gridManager.EsCasillaValida(casillaPrevisualizada);
        }

        // Apagamos el tablero visualmente
        if (GridManager.Instance != null)
        {
            GridManager.Instance.OcultarRangoMovimiento();
        }

        // Si es inválida, ocupada, soltada en el aire o en su propio sitio, vuelve al origen
        if (casillaPrevisualizada == casillaAnterior || casillaPrevisualizada == null || casillaPrevisualizada.EstaOcupada || casillaInvalida)
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
