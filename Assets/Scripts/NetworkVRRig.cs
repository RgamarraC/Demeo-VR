using UnityEngine;
using Fusion;

public class NetworkVRRig : NetworkBehaviour
{
    [Header("Visuales de Red (Lo que verán los demás)")]
    [Tooltip("El objeto que representa la cabeza en la red (ej. un cubo)")]
    [SerializeField] private Transform _networkHeadVisual;
    [Tooltip("El objeto que representa la mano izquierda en la red")]
    [SerializeField] private Transform _networkLeftHandVisual;
    [Tooltip("El objeto que representa la mano derecha en la red")]
    [SerializeField] private Transform _networkRightHandVisual;

    // Referencias a los componentes físicos de nuestro propio XR Origin
    private Transform _localHead;
    private Transform _localLeftHand;
    private Transform _localRightHand;

    // Variables sincronizadas por la red usando Fusion
    [Networked] private Vector3 HeadPosition { get; set; }
    [Networked] private Quaternion HeadRotation { get; set; }
    
    [Networked] private Vector3 LeftHandPosition { get; set; }
    [Networked] private Quaternion LeftHandRotation { get; set; }
    
    [Networked] private Vector3 RightHandPosition { get; set; }
    [Networked] private Quaternion RightHandRotation { get; set; }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            // SOMOS EL DUEÑO DE ESTE AVATAR.
            // Vamos a buscar nuestro XR Origin real en la escena para copiar sus movimientos.
            
            // 1. Buscar la cámara principal (Cabeza)
            GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            if (mainCamera != null) 
                _localHead = mainCamera.transform;
            else 
                Debug.LogError("NetworkVRRig: No se encontró un objeto con la etiqueta 'MainCamera'.");

            // 2. Buscar las manos (por nombre estándar del XR Interaction Toolkit)
            GameObject leftHand = GameObject.Find("Left Controller");
            if (leftHand == null) leftHand = GameObject.Find("LeftHand Controller");
            if (leftHand != null) _localLeftHand = leftHand.transform;

            GameObject rightHand = GameObject.Find("Right Controller");
            if (rightHand == null) rightHand = GameObject.Find("RightHand Controller");
            if (rightHand != null) _localRightHand = rightHand.transform;

            // 3. Ocultar nuestros propios visuales para no ver bloques flotando frente a nuestra cara
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach(var rend in renderers)
            {
                rend.enabled = false;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Solo el dueño (quien mueve el casco real) puede actualizar las variables de red
        if (HasStateAuthority)
        {
            if (_localHead != null)
            {
                HeadPosition = _localHead.position;
                HeadRotation = _localHead.rotation;
            }
            if (_localLeftHand != null)
            {
                LeftHandPosition = _localLeftHand.position;
                LeftHandRotation = _localLeftHand.rotation;
            }
            if (_localRightHand != null)
            {
                RightHandPosition = _localRightHand.position;
                RightHandRotation = _localRightHand.rotation;
            }
        }
    }

    public override void Render()
    {
        // Render se ejecuta cada frame. Usamos Lerp para que el movimiento de los demás se vea fluido (interpolación)
        float lerpSpeed = Time.deltaTime * 15f;

        if (_networkHeadVisual != null)
        {
            _networkHeadVisual.position = Vector3.Lerp(_networkHeadVisual.position, HeadPosition, lerpSpeed);
            _networkHeadVisual.rotation = Quaternion.Slerp(_networkHeadVisual.rotation, HeadRotation, lerpSpeed);
        }
        
        if (_networkLeftHandVisual != null)
        {
            _networkLeftHandVisual.position = Vector3.Lerp(_networkLeftHandVisual.position, LeftHandPosition, lerpSpeed);
            _networkLeftHandVisual.rotation = Quaternion.Slerp(_networkLeftHandVisual.rotation, LeftHandRotation, lerpSpeed);
        }

        if (_networkRightHandVisual != null)
        {
            _networkRightHandVisual.position = Vector3.Lerp(_networkRightHandVisual.position, RightHandPosition, lerpSpeed);
            _networkRightHandVisual.rotation = Quaternion.Slerp(_networkRightHandVisual.rotation, RightHandRotation, lerpSpeed);
        }
    }
}
