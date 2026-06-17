using UnityEngine;
using TMPro;
using Fusion;
using System.Collections.Generic;

public class LobbyGenerator : MonoBehaviour
{
    public GameObject menuPrincipal;
    public GameObject menuLobby;

    public TMP_InputField nombreInput;

    public TMP_Text textoCodigo;
    public TMP_Text textoJugadores;

    public PlayerSlotUI slot1;
    public PlayerSlotUI slot2;
    public PlayerSlotUI slot3;

    public static string codigoLobby;
    public static string hostName;

    private NetworkRunner runner;
    private LobbyCallbacks callbacks;

    private Dictionary<PlayerRef, string> playerNames = new Dictionary<PlayerRef, string>();

    public async void CrearSala()
    {
        hostName = nombreInput.text.Trim();

        if (string.IsNullOrEmpty(hostName))
        {
            Debug.Log("Falta nombre del host");
            return;
        }

        codigoLobby = Random.Range(1000, 10000).ToString();

        if (runner == null)
            runner = gameObject.AddComponent<NetworkRunner>();

        if (callbacks == null)
            callbacks = new LobbyCallbacks(this);

        runner.AddCallbacks(callbacks);

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = codigoLobby,
            PlayerCount = 3
        });

        menuPrincipal.SetActive(false);
        menuLobby.SetActive(true);

        textoCodigo.text = codigoLobby;

        playerNames[runner.LocalPlayer] = hostName;

        UpdateUI();
    }

    public void UpdateUI()
    {
        // 🔥 FIX REAL FUSION
        if (runner != null && runner.SessionInfo != null)
            textoJugadores.text = runner.SessionInfo.PlayerCount + "/3";
        else
            textoJugadores.text = "1/3";

        PlayerRef[] players = new PlayerRef[3];
        int i = 0;

        foreach (var p in runner.ActivePlayers)
        {
            players[i++] = p;
            if (i >= 3) break;
        }

        if (i > 0)
            slot1.ActualizarSlot(GetName(players[0]), "Sin rol");
        else
            slot1.ActualizarSlot("Vacío", "-");

        if (i > 1)
            slot2.ActualizarSlot(GetName(players[1]), "Sin rol");
        else
            slot2.ActualizarSlot("Vacío", "-");

        if (i > 2)
            slot3.ActualizarSlot(GetName(players[2]), "Sin rol");
        else
            slot3.ActualizarSlot("Vacío", "-");
    }

    string GetName(PlayerRef player)
    {
        if (playerNames.ContainsKey(player))
            return playerNames[player];

        return "Player";
    }

    public void RefreshUI()
    {
        UpdateUI();
    }
}