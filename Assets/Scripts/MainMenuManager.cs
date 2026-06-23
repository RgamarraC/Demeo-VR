using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneles de Interfaz")]
    public GameObject mainMenuPanel;
    public GameObject roomMenuPanel;
    
    [Header("Elementos de Salas")]
    public TMP_InputField roomNameInput;
    public Button connectButton;
    public TextMeshProUGUI statusText;

    [Header("Configuración de Photon Fusion")]
    public NetworkRunner networkRunnerPrefab;

    [Header("Transición y Efectos")]
    [Tooltip("El material de la esfera negra o el CanvasGroup que hace de oscuridad.")]
    public CanvasGroup darknessCanvasGroup; 
    public float fadeDuration = 2f;

    [Header("Eventos de Jugador")]
    [Tooltip("Scripts de movimiento de tu XR Rig (ej: ContinuousMoveProvider) que quieres desactivar/activar.")]
    public MonoBehaviour[] locomotionScripts;

    [Tooltip("Eventos que se ejecutan cuando el juego inicia (puedes activar luces desde el inspector aquí).")]
    public UnityEvent OnGameJoined;

    private NetworkRunner _runnerInstance;

    void Start()
    {
        // 1. Empezamos a oscuras y sin movimiento
        SetPlayerLobbyState();

        ShowMainMenu();

        if (connectButton != null)
        {
            connectButton.onClick.AddListener(OnConnectButtonClicked);
        }
    }

    private void SetPlayerLobbyState()
    {
        // Desactivar scripts de movimiento
        foreach (var script in locomotionScripts)
        {
            if (script != null) script.enabled = false;
        }

        // Asegurarnos de que está totalmente oscuro
        if (darknessCanvasGroup != null)
        {
            darknessCanvasGroup.alpha = 1f;
            darknessCanvasGroup.gameObject.SetActive(true);
        }
    }

    private void SetPlayerGameState()
    {
        // Activar scripts de movimiento
        foreach (var script in locomotionScripts)
        {
            if (script != null) script.enabled = true;
        }

        // Iniciar el Fade Out de la oscuridad
        if (darknessCanvasGroup != null)
        {
            StartCoroutine(FadeOutDarkness());
        }

        // Ejecutar cualquier otro evento extra que pongas en el inspector (encender luces, sonidos, etc)
        OnGameJoined?.Invoke();
    }

    private IEnumerator FadeOutDarkness()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            darknessCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        
        darknessCanvasGroup.alpha = 0f;
        darknessCanvasGroup.gameObject.SetActive(false);
    }

    // --- Navegación ---

    public void ShowRoomMenu()
    {
        mainMenuPanel.SetActive(false);
        roomMenuPanel.SetActive(true);
        if (statusText != null) statusText.text = "Ingresa el nombre de la sala";
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        roomMenuPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // --- Lógica de Conexión ---

    private async void OnConnectButtonClicked()
    {
        if (roomNameInput == null) return;
        string roomName = roomNameInput.text;

        if (string.IsNullOrEmpty(roomName))
        {
            if (statusText != null) statusText.text = "Error: El nombre está vacío";
            return;
        }

        if (statusText != null) statusText.text = "Conectando a la sala...";
        if (connectButton != null) connectButton.interactable = false;

        await StartPhotonSession(roomName);
    }

    private async Task StartPhotonSession(string sessionName)
    {
        if (_runnerInstance == null)
        {
            _runnerInstance = Instantiate(networkRunnerPrefab);
            _runnerInstance.name = "Network Runner";
        }

        // Ocultar la UI del menú
        roomMenuPanel.SetActive(false);

        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
            // Como ya estamos en la escena, cargamos la escena activa actualmente
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = _runnerInstance.gameObject.AddComponent<NetworkSceneManagerDefault>()
        };

        StartGameResult result = await _runnerInstance.StartGame(startGameArgs);

        if (!result.Ok)
        {
            Debug.LogError($"Error al conectar: {result.ShutdownReason}");
            if (statusText != null) statusText.text = "Fallo al conectar. Intenta de nuevo.";
            roomMenuPanel.SetActive(true);
            if (connectButton != null) connectButton.interactable = true;
        }
        else
        {
            Debug.Log("Conectado con éxito. Transición a la sala...");
            
            // Ya nos conectamos exitosamente a la sala compartida
            SetPlayerGameState();
        }
    }
}
