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

            // 1. Detectar clic izquierdo para AGARRAR una ficha
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                // Disparamos un raycast general para ver a qué le dimos
                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                {
                    FichaRPG ficha = hit.collider.GetComponentInParent<FichaRPG>();
                    if (ficha != null)
                    {
                        fichaSostenida = ficha;
                        fichaSostenida.AlSerLevantada();
                        Debug.Log($"<color=green>[TestHarness] Levantando ficha: {fichaSostenida.name}</color>");
                    }
                }
            }

            // 2. Mover la ficha mientras se mantiene el clic
            if (Mouse.current.leftButton.isPressed && fichaSostenida != null)
            {
                Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                // Aquí solo queremos golpear el tablero para saber a dónde apuntamos
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, capaTablero))
                {
                    // Hacer que la ficha siga el punto donde el raycast toca el tablero, pero elevada
                    Vector3 posicionDeseada = hit.point + Vector3.up * distanciaAgarre;
                    fichaSostenida.transform.position = Vector3.Lerp(fichaSostenida.transform.position, posicionDeseada, Time.deltaTime * 20f);
                }
            }

            // 3. Detectar soltar el clic izquierdo para SOLTAR la ficha
            if (Mouse.current.leftButton.wasReleasedThisFrame && fichaSostenida != null)
            {
                Debug.Log($"<color=orange>[TestHarness] Soltando ficha: {fichaSostenida.name}</color>");
                fichaSostenida.AlSerSoltada();
                fichaSostenida = null;
            }
        }
    }
}
