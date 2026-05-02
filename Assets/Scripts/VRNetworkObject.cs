using UnityEngine;
using Fusion;

/// <summary>
/// Script maestro para sincronizar objetos VR esquivando los problemas del Addon de físicas.
/// </summary>
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(Rigidbody))]
public class VRNetworkObject : NetworkBehaviour, IStateAuthorityChanged
{
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Conectar esto al evento "Select Entered" del XR Grab Interactable en el Inspector.
    /// </summary>
    public void RequestNetworkAuthority()
    {
        if (Object != null && !HasStateAuthority)
        {
            // Pedimos permiso al servidor
            Object.RequestStateAuthority();
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Si NO somos los dueños (el objeto está lejos o lo tiene nuestro amigo)
        if (!HasStateAuthority)
        {
            // APAGAMOS nuestra gravedad local. 
            // Esto evita que nuestra computadora tire el objeto al suelo mientras nuestro amigo lo levanta.
            if (!_rb.isKinematic)
            {
                _rb.isKinematic = true;
            }
        }
    }

    /// <summary>
    /// Este evento se dispara automáticamente cuando el servidor nos concede o nos quita el permiso.
    /// </summary>
    public void StateAuthorityChanged()
    {
        // Si acabamos de recibir la autoridad (acabamos de agarrar el objeto)
        if (HasStateAuthority)
        {
            // ENCENDEMOS las físicas locales de nuevo para que el XR Interaction Toolkit
            // pueda agarrarlo y moverlo con normalidad en nuestra mano.
            _rb.isKinematic = false;
        }
    }
}
