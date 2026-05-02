using UnityEngine;
using Fusion;

public class NetworkManager : MonoBehaviour
{
    private NetworkRunner _runner;
    
    [Tooltip("El prefab de red que representa al jugador (NetworkVRRig)")]
    [SerializeField] private NetworkPrefabRef _playerPrefab;

    async void Start()
    {
        // Añade el NetworkRunner si no existe
        _runner = gameObject.GetComponent<NetworkRunner>();
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
        }

        // Permite que Fusion procese los inputs locales
        _runner.ProvideInput = true;
        
        // El SceneManager es necesario para que Fusion sepa qué escena cargar/sincronizar
        var sceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null)
        {
            sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        // Configuramos los parámetros para iniciar la sesión multijugador
        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Shared, // Shared Mode es ideal para demos sin servidor dedicado
            SessionName = "DemoVR_Sala1", // Nombre fijo para que ambos se conecten a la misma sala
            SceneManager = sceneManager
        };

        Debug.Log("Intentando conectar a Photon Fusion...");
        
        // Iniciamos el runner
        var result = await _runner.StartGame(startGameArgs);

        if (result.Ok)
        {
            Debug.Log("¡Conexión exitosa a Photon Fusion!");
            
            // Si nos conectamos con éxito, hacemos Spawn (creamos) nuestro avatar en la red.
            // Lo creamos en (0,0,0) porque el script NetworkVRRig se encargará de moverlo a donde esté nuestro casco real.
            _runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
        }
        else
        {
            Debug.LogError($"Error al iniciar Photon Fusion: {result.ShutdownReason}");
        }
    }
}
