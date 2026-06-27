using UnityEngine;
using UnityEngine.InputSystem;

public class DungeonMasterController : MonoBehaviour
{
    [Header("Configuración de Invocación")]
    [Tooltip("Prefab del monstruo que el DM va a invocar en las sombras.")]
    [SerializeField] private GameObject prefabEnemigo;
    
    [Header("Configuración VR")]
    [Tooltip("El Transform del mando de VR (derecho o izquierdo) desde donde sale el láser de apuntado.")]
    [SerializeField] private Transform punteroVR;

    [Tooltip("La acción de Input System vinculada al gatillo de tu mando para confirmar la invocación.")]
    [SerializeField] private InputActionReference botonConfirmar;

    [Header("Estado Interno")]
    [SerializeField] private bool estaEnModoInvocacion = false;

    private CasillaComponent ultimaCasillaApuntada = null;

    private void OnEnable()
    {
        if (botonConfirmar != null)
        {
            botonConfirmar.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (botonConfirmar != null)
        {
            botonConfirmar.action.Disable();
        }
    }

    private void Update()
    {
        if (!estaEnModoInvocacion) return;
        
        if (punteroVR == null)
        {
            Debug.LogWarning("[DungeonMaster] Falta asignar el Transform del punteroVR en el inspector.");
            return;
        }

        // Lanzamos raycast físicamente desde la punta de tu mano en VR hacia donde estás apuntando
        Ray ray = new Ray(punteroVR.position, punteroVR.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            CasillaComponent casillaDetectada = hit.collider.GetComponentInParent<CasillaComponent>();

            // Si apuntamos a una casilla distinta a la del fotograma anterior, limpiamos la vieja
            if (ultimaCasillaApuntada != null && ultimaCasillaApuntada != casillaDetectada)
            {
                LimpiarCasillaAnterior();
            }

            if (casillaDetectada != null)
            {
                // Validaciones estrictas de negocio para la invocación
                bool esValida = !casillaDetectada.esObstaculo && 
                                !casillaDetectada.estaOcupada && 
                                casillaDetectada.estaEnNiebla;

                if (esValida)
                {
                    // Efecto visual de apuntado (Hover)
                    if (ultimaCasillaApuntada != casillaDetectada)
                    {
                        casillaDetectada.SetearEstadoVisual("DM_Target");
                        ultimaCasillaApuntada = casillaDetectada;
                    }

                    // Confirmación e Instanciación al apretar el gatillo del control de VR
                    bool gatilloApretado = botonConfirmar != null && botonConfirmar.action.WasPressedThisFrame();
                    
                    if (gatilloApretado)
                    {
                        InvocarEnemigo(casillaDetectada);
                    }
                }
                else
                {
                    // Si apuntamos a una casilla pero no es válida, limpiamos el hover si había uno
                    LimpiarCasillaAnterior();
                }
            }
        }
        else
        {
            // Si el raycast sale del tablero o no golpea nada, limpiamos
            LimpiarCasillaAnterior();
        }
    }

    /// <summary>
    /// Limpia el estado visual de la última casilla apuntada, devolviéndola a la niebla.
    /// </summary>
    private void LimpiarCasillaAnterior()
    {
        if (ultimaCasillaApuntada != null)
        {
            // Como las invocaciones solo se pueden hacer en casillas con niebla, 
            // siempre devolvemos el estado visual a "Niebla".
            ultimaCasillaApuntada.SetearEstadoVisual("Niebla");
            ultimaCasillaApuntada = null;
        }
    }

    /// <summary>
    /// Invoca el monstruo en la casilla objetivo y actualiza el estado del juego.
    /// </summary>
    private void InvocarEnemigo(CasillaComponent casilla)
    {
        if (prefabEnemigo == null)
        {
            Debug.LogError("[DungeonMaster] No hay Prefab Enemigo asignado en el DungeonMasterController.");
            return;
        }

        // 1. Instanciamos el prefab físicamente
        GameObject nuevoEnemigo = Instantiate(prefabEnemigo, casilla.ObtenerCentro(), Quaternion.identity);

        // 1.5 Le inyectamos sus coordenadas iniciales para que la IA sepa dónde nació
        FichaEnemigoAI ia = nuevoEnemigo.GetComponent<FichaEnemigoAI>();
        if (ia != null)
        {
            ia.coordenadaX = casilla.coordenadaX;
            ia.coordenadaZ = casilla.coordenadaZ;
        }

        // 2. Ocupamos la casilla a nivel de arquitectura
        casilla.estaOcupada = true;

        // 3. Salimos del modo invocación y limpiamos rastro visual
        estaEnModoInvocacion = false;
        LimpiarCasillaAnterior();

        Debug.Log($"<color=red>[DungeonMaster] ¡Enemigo invocado en las sombras de la casilla {casilla.name}!</color>");

        // 4. Refrescamos la niebla de guerra global
        if (GridManager.Instance != null)
        {
            GridManager.Instance.ActualizarNieblaDeGuerraGlobal();
        }
    }

    /// <summary>
    /// Llama a este método desde el evento OnClick de tu UI.
    /// Funciona como un interruptor (Toggle): lo enciende si estaba apagado, y viceversa.
    /// </summary>
    public void AlternarModoInvocacion()
    {
        estaEnModoInvocacion = !estaEnModoInvocacion;
        
        if (estaEnModoInvocacion)
        {
            Debug.Log("<color=magenta>[DungeonMaster] Modo Invocación ACTIVADO.</color>");
        }
        else
        {
            Debug.Log("<color=magenta>[DungeonMaster] Modo Invocación CANCELADO.</color>");
            LimpiarCasillaAnterior(); // Limpiamos la luz roja si se arrepintió
        }
    }
}
