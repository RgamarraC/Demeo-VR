using UnityEngine;
using TMPro;
using Fusion;
using System.Threading.Tasks;

public class JoinLobbyUI : MonoBehaviour
{
    public TMP_InputField nombreInput;
    public TMP_InputField codigoInput;

    public LobbyGenerator lobbyGenerator;

    public static string nombreJugador;
    public static string codigoIngresado;

    private NetworkRunner runner;
    private GameObject runnerObject;
    private bool conectando = false;

    public async void EntrarSala()
    {
        if (conectando)
        {
            Debug.LogWarning("Ya se está intentando entrar a una sala.");
            return;
        }

        nombreJugador = nombreInput.text.Trim();
        codigoIngresado = codigoInput.text.Trim();

        if (string.IsNullOrEmpty(nombreJugador))
        {
            Debug.LogWarning("Falta nombre");
            return;
        }

        if (codigoIngresado.Length != 4)
        {
            Debug.LogWarning("Código inválido");
            return;
        }

        if (lobbyGenerator == null)
        {
            Debug.LogError("JoinLobbyUI: falta asignar LobbyGenerator en el inspector.");
            return;
        }

        conectando = true;

        try
        {
            // Siempre limpiamos cualquier runner viejo antes de entrar.
            await LimpiarRunnerLocal();

            // Creamos un objeto separado SOLO para el NetworkRunner del invitado.
            runnerObject = new GameObject("LobbyNetworkRunner_Client");

            // IMPORTANTE: que sobreviva al cambio de escena hacia Test.
            DontDestroyOnLoad(runnerObject);

            runner = runnerObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = true;

            // El SceneManager de Fusion también debe vivir junto al runner.
            NetworkSceneManagerDefault fusionSceneManager =
                runnerObject.AddComponent<NetworkSceneManagerDefault>();

            Debug.Log("Intentando entrar a sala: " + codigoIngresado);

            var result = await runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Client,
                SessionName = codigoIngresado,
                SceneManager = fusionSceneManager
            });

            if (result.Ok)
            {
                Debug.Log("Entró a sala: " + codigoIngresado);

                // Guardamos el runner conectado en una variable temporal.
                NetworkRunner runnerConectado = runner;

                // IMPORTANTE:
                // JoinLobbyUI ya no se queda con este runner.
                // Desde ahora lo manejará LobbyGenerator.
                runner = null;
                runnerObject = null;

                lobbyGenerator.ConfigurarComoInvitado(
                    runnerConectado,
                    nombreJugador,
                    codigoIngresado
                );
            }
            else
            {
                Debug.LogWarning("No pudo entrar a la sala: " + result.ErrorMessage);

                await LimpiarRunnerLocal();
            }
        }
        finally
        {
            conectando = false;
        }
    }

    private async Task LimpiarRunnerLocal()
    {
        if (runner != null)
        {
            NetworkRunner runnerParaCerrar = runner;
            runner = null;

            await runnerParaCerrar.Shutdown();
        }

        if (runnerObject != null)
        {
            Destroy(runnerObject);
            runnerObject = null;
        }
    }
}