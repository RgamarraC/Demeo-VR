using UnityEngine;
using UnityEngine.InputSystem;

namespace DemeoVR.Gameplay.Testing
{
    /// <summary>
    /// Script de depuración local para probar la lógica del tablero (casillas, BFS, muros y físicas)
    /// sin necesidad de iniciar Photon Fusion ni el servidor.
    /// Instrucciones: 
    /// 1. Arrastra este script a la Cámara Principal o a un objeto vacío (ej. "TestHarness").
    /// 2. Ajusta la capaTablero para que el raycast sepa dónde está el suelo.
    /// 3. Dale a Play en el Editor, haz clic izquierdo sobre una ficha y arrástrala.
    /// </summary>
    public class LocalTestHarness : MonoBehaviour
    {
        [Header("Configuración de Prueba")]
        [Tooltip("Cámara principal para lanzar el raycast del mouse.")]
        [SerializeField] private Camera mainCamera;
        
        [Tooltip("La capa física donde se encuentran las casillas (para saber dónde mover la ficha).")]
        [SerializeField] private LayerMask capaTablero;

        private FichaRPG fichaSostenida;
        private DadoD12 dadoSostenido;
        private float distanciaAgarre = 1.5f; // Altura a la que flota la ficha

        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (mainCamera == null || Mouse.current == null) return;

            // 1. Detectar clic izquierdo para AGARRAR
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                {
                    // Verificamos si tocamos una ficha
                    FichaRPG ficha = hit.collider.GetComponentInParent<FichaRPG>();
                    if (ficha != null)
                    {
                        fichaSostenida = ficha;
                        fichaSostenida.AlSerLevantada();
                        Debug.Log($"<color=green>[TestHarness] Levantando ficha: {fichaSostenida.name}</color>");
                    }
                    else
                    {
                        // Si no tocamos una ficha, vemos si tocamos el Dado
                        DadoD12 dado = hit.collider.GetComponentInParent<DadoD12>();
                        if (dado != null)
                        {
                            dadoSostenido = dado;
                            Rigidbody rb = dadoSostenido.GetComponent<Rigidbody>();
                            if (rb != null) rb.isKinematic = true; // Lo congelamos en el aire
                            dadoSostenido.enMovimiento = false;
                            Debug.Log($"<color=green>[TestHarness] Agarrando Dado...</color>");
                        }
                    }
                }
            }

            // 2. Mover el objeto sostenido mientras se mantiene el clic
            if (Mouse.current.leftButton.isPressed)
            {
                if (fichaSostenida != null)
                {
                    Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                    if (Physics.Raycast(ray, out RaycastHit hit, 100f, capaTablero))
                    {
                        Vector3 posicionDeseada = hit.point + Vector3.up * distanciaAgarre;
                        fichaSostenida.transform.position = Vector3.Lerp(fichaSostenida.transform.position, posicionDeseada, Time.deltaTime * 20f);
                    }
                }
                else if (dadoSostenido != null)
                {
                    Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                    if (Physics.Raycast(ray, out RaycastHit hit, 100f, capaTablero))
                    {
                        // Levantamos el dado en el aire
                        Vector3 posicionDeseada = hit.point + Vector3.up * (distanciaAgarre * 1.5f);
                        dadoSostenido.transform.position = Vector3.Lerp(dadoSostenido.transform.position, posicionDeseada, Time.deltaTime * 20f);
                    }
                }
            }

            // 3. Detectar soltar el clic izquierdo para SOLTAR
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (fichaSostenida != null)
                {
                    Debug.Log($"<color=orange>[TestHarness] Soltando ficha: {fichaSostenida.name}</color>");
                    fichaSostenida.AlSerSoltada();
                    fichaSostenida = null;
                }
                else if (dadoSostenido != null)
                {
                    Debug.Log($"<color=orange>[TestHarness] Soltando Dado...</color>");
                    Rigidbody rb = dadoSostenido.GetComponent<Rigidbody>();
                    
                    if (rb != null)
                    {
                        rb.isKinematic = false;
                        rb.WakeUp();
                        
                        // Añadimos un pequeño giro aleatorio para que ruede bien al caer de la mano
                        Vector3 torque = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f));
                        rb.AddTorque(torque, ForceMode.Impulse);
                    }

                    dadoSostenido.enMovimiento = true;
                    dadoSostenido.resultadoActual = 0;
                    dadoSostenido = null;
                }
            }
        }
    }
}
