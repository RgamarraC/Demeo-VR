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
    private NetworkTransform _netTransform;
    private bool _isGrabbed = false;
    private UnityEngine.Behaviour _grabInteractable;

    private float _ultimoEnvioRPC = 0f;
    private const float _intervaloEnvioRPC = 0.05f; // Sincroniza cada 50ms para fluidez en red (20 updates/segundo)

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _netTransform = GetComponent<NetworkTransform>();
    }

    private void Start()
    {
        _grabInteractable = BuscarXRGrabInteractable();
    }

    private void Update()
    {
        if (_grabInteractable != null)
        {
            bool grabbedNow = CheckIsSelected(_grabInteractable);
            if (grabbedNow != _isGrabbed)
            {
                if (grabbedNow)
                {
                    OnGrabStart();
                }
                else
                {
                    OnGrabEnd();
                }
            }
        }

        // Si lo tenemos agarrado y NO somos el host (dueño oficial del estado),
        // enviamos actualizaciones de posición periódicas al host mediante RPC.
        if (_isGrabbed && !HasStateAuthority)
        {
            if (Time.time - _ultimoEnvioRPC >= _intervaloEnvioRPC)
            {
                _ultimoEnvioRPC = Time.time;
                RPC_ActualizarPosicionServidor(transform.position, transform.rotation, true);
            }
        }
    }

    private bool CheckIsSelected(UnityEngine.Behaviour interactable)
    {
        if (interactable == null)
            return false;

        var isSelectedProp = interactable.GetType().GetProperty("isSelected");
        if (isSelectedProp != null)
        {
            return (bool)isSelectedProp.GetValue(interactable);
        }
        return false;
    }

    private void OnGrabStart()
    {
        _isGrabbed = true;

        if (!HasStateAuthority)
        {
            if (_netTransform != null)
            {
                _netTransform.enabled = false; // Apagar localmente para evitar tirones de red mientras movemos
            }

            // Aún pedimos autoridad como fallback (por si estamos en Shared Mode)
            if (Object != null)
            {
                Object.RequestStateAuthority();
            }
        }

        Debug.Log($"[VRNetworkObject] Agarre iniciado en {gameObject.name}. HasStateAuthority: {HasStateAuthority}");
    }

    private void OnGrabEnd()
    {
        _isGrabbed = false;

        if (_netTransform != null)
        {
            _netTransform.enabled = true; // Rehabilitar al soltar
        }

        // Enviamos el último RPC informando que se soltó para reactivar físicas en el Host
        if (!HasStateAuthority)
        {
            RPC_ActualizarPosicionServidor(transform.position, transform.rotation, false);
        }
        else
        {
            // Si somos la autoridad y el objeto no es una ficha de tablero (que maneja su propia cinemática),
            // restauramos las físicas del Rigidbody localmente.
            if (GetComponent<FichaRPG>() == null && _rb != null)
            {
                _rb.isKinematic = false;
                _rb.WakeUp();
            }
        }

        Debug.Log($"[VRNetworkObject] Agarre terminado en {gameObject.name}. HasStateAuthority: {HasStateAuthority}");
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_ActualizarPosicionServidor(Vector3 posicion, Quaternion rotacion, bool estaSostenido)
    {
        // El host actualiza la posición del objeto (que se replicará automáticamente a todos por NetworkTransform)
        transform.position = posicion;
        transform.rotation = rotacion;

        if (_rb != null)
        {
            _rb.position = posicion;
            _rb.rotation = rotacion;

            if (estaSostenido)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                if (!_rb.isKinematic)
                {
                    _rb.isKinematic = true; // Desactivar físicas en el Host mientras el cliente mueve el objeto
                }
            }
            else
            {
                // Al soltar el objeto, el Host (dueño) vuelve a encender físicas (gravedad, colisión, lanzamiento de dados)
                _rb.isKinematic = false;
                _rb.WakeUp();
            }
        }
    }

    /// <summary>
    /// Conectar esto al evento "Select Entered" del XR Grab Interactable en el Inspector (opcional, ahora es automático).
    /// </summary>
    public void RequestNetworkAuthority()
    {
        if (!_isGrabbed)
        {
            OnGrabStart();
        }
    }

    /// <summary>
    /// Conectar esto al evento "Select Exited" del XR Grab Interactable en el Inspector (opcional, ahora es automático).
    /// </summary>
    public void ReleaseNetworkAuthority()
    {
        if (_isGrabbed)
        {
            OnGrabEnd();
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Si NO somos los dueños y NO está agarrado localmente
        if (!HasStateAuthority && !_isGrabbed)
        {
            // APAGAMOS nuestra gravedad local
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
        if (HasStateAuthority)
        {
            // Solo desactivamos kinematic si no es una ficha de tablero, ya que las fichas
            // controlan su propio estado de física y colocación.
            if (GetComponent<FichaRPG>() == null && _rb != null)
            {
                _rb.isKinematic = false;
            }

            // Si ya somos los dueños (por ejemplo, en Shared Mode) reactivamos NetworkTransform para sincro en tiempo real
            if (_isGrabbed && _netTransform != null)
            {
                _netTransform.enabled = true;
            }
        }
    }

    private UnityEngine.Behaviour BuscarXRGrabInteractable()
    {
        MonoBehaviour[] componentes = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour componente in componentes)
        {
            if (componente == null)
                continue;

            if (componente.GetType().Name == "XRGrabInteractable")
                return componente;
        }
        return null;
    }
}
