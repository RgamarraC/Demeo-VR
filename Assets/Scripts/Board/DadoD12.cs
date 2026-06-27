using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DadoD12 : MonoBehaviour
{
    [System.Serializable]
    public struct FaceSensor 
    {
        public int numeroCara;
        public Transform transformSensor;
    }

    [Header("Configuración de Caras")]
    [Tooltip("Asigna aquí los 12 Transforms correspondientes al centro de cada cara del dado.")]
    public FaceSensor[] sensoresCaras = new FaceSensor[12];

    [Header("Estado Actual")]
    public bool enMovimiento = false;
    public int resultadoActual = 0;

    private Rigidbody rb;

    private void Awake()
    {
        // Obtenemos la referencia al Rigidbody automáticamente
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Solo monitoreamos la física si sabemos que el dado fue lanzado y está en movimiento
        if (enMovimiento)
        {
            // Verificamos si la traslación y rotación han caído por debajo del umbral de reposo
            if (rb.linearVelocity.sqrMagnitude < 0.001f && rb.angularVelocity.sqrMagnitude < 0.001f)
            {
                // El dado se ha detenido por completo
                enMovimiento = false;
                CalcularNumeroGanador();
            }
        }
    }

    /// <summary>
    /// Recorre todos los sensores asignados y determina cuál está más alto en el eje Y global.
    /// </summary>
    private void CalcularNumeroGanador()
    {
        if (sensoresCaras == null || sensoresCaras.Length == 0)
        {
            Debug.LogWarning("[DadoD12] No hay sensores configurados en el dado.");
            return;
        }

        float maxPosY = float.MinValue;
        int ganador = 0;

        // Iteramos sobre todos los sensores para encontrar el que tenga mayor altura en el mundo
        foreach (var sensor in sensoresCaras)
        {
            if (sensor.transformSensor != null)
            {
                if (sensor.transformSensor.position.y > maxPosY)
                {
                    maxPosY = sensor.transformSensor.position.y;
                    ganador = sensor.numeroCara;
                }
            }
            else
            {
                Debug.LogWarning($"[DadoD12] Falta asignar el Transform para la cara {sensor.numeroCara}.");
            }
        }

        resultadoActual = ganador;
        Debug.Log($"El dado se detuvo. Salió el número: {resultadoActual}");
    }

    /// <summary>
    /// Método público para iniciar el lanzamiento del dado en la mesa.
    /// </summary>
    /// <param name="fuerza">Dirección e intensidad del lanzamiento.</param>
    /// <param name="torque">Giro aleatorio inicial.</param>
    public void LanzarDado(Vector3 fuerza, Vector3 torque)
    {
        if (rb == null) return;

        // Reiniciamos el estado para comenzar el monitoreo en el Update
        enMovimiento = true;
        resultadoActual = 0;

        // Aseguramos que responda a la física si estaba bloqueado
        rb.isKinematic = false;
        
        // Despertamos el Rigidbody por si Unity lo puso a dormir (Sleep)
        rb.WakeUp();

        // Aplicamos la fuerza y el giro en modo de impulso (instantáneo)
        rb.AddForce(fuerza, ForceMode.Impulse);
        rb.AddTorque(torque, ForceMode.Impulse);
    }

    // ==========================================
    // MÉTODOS PARA REALIDAD VIRTUAL (VR - XR TOOLKIT)
    // ==========================================

    /// <summary>
    /// Vincúlalo al evento "Select Entered" del XRGrabInteractable.
    /// </summary>
    public void AlSerAgarradoVR()
    {
        enMovimiento = false;
        resultadoActual = 0;
    }

    /// <summary>
    /// Vincúlalo al evento "Select Exited" del XRGrabInteractable.
    /// </summary>
    public void AlSerSoltadoVR()
    {
        // Al soltarlo con la mano VR, le decimos al script que empiece a rastrear cuándo se detiene.
        enMovimiento = true;
        if (rb != null)
        {
            rb.WakeUp();
        }
    }
}
