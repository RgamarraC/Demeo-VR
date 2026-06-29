using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameplayUIManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelHeroe;
    public GameObject panelDM;

    [Header("Texto")]
    public TMP_Text textoTurno;

    [Header("Botones Heroe")]
    public Button botonAtacar;
    public Button botonEndTurnHeroe;

    [Header("Botones Dungeon Master")]
    public Button botonInvocarEnemigo;
    public Button botonInvocarTrampa;
    public Button botonEndTurnDM;

    private GameplayManager gameplayManager;
    private TurnManager turnManager;
    public bool juegoTerminado = false;
    private void Start()
    {
        StartCoroutine(IniciarUI());
    }

    private IEnumerator IniciarUI()
    {
        // Esperar a que existan los managers
        while (GameplayManager.Instance == null || TurnManager.Instance == null)
        {
            yield return null;
        }

        gameplayManager = GameplayManager.Instance;
        turnManager = TurnManager.Instance;

        // Esperar a que GameplayManager ya haya leído el rol local
        while (string.IsNullOrEmpty(gameplayManager.LocalPlayerRole) ||
               gameplayManager.LocalPlayerRole == "Sin rol")
        {
            yield return null;
        }

        ConfigurarBotones();
        MostrarUIPorRol();
        ActualizarBotones();

        Debug.Log(
            "GAMEPLAY UI: UI iniciada para " +
            gameplayManager.LocalPlayerName +
            " | Rol = " +
            gameplayManager.LocalPlayerRole
        );
    }

    private void Update()
    {
        if (juegoTerminado)
        {
            DesactivarTodosLosBotones();
            return;
        }

        if (gameplayManager == null || turnManager == null)
            return;

        ActualizarTextoTurno();
        ActualizarBotones();
    }

    private void ConfigurarBotones()
    {
        if (botonEndTurnHeroe != null)
            botonEndTurnHeroe.onClick.AddListener(OnEndTurnPressed);

        if (botonEndTurnDM != null)
            botonEndTurnDM.onClick.AddListener(OnEndTurnPressed);

        if (botonAtacar != null)
            botonAtacar.onClick.AddListener(OnAtacarPressed);

        if (botonInvocarEnemigo != null)
            botonInvocarEnemigo.onClick.AddListener(OnInvocarEnemigoPressed);

        if (botonInvocarTrampa != null)
            botonInvocarTrampa.onClick.AddListener(OnInvocarTrampaPressed);
    }

    private void MostrarUIPorRol()
    {
        bool soyHeroe = EsHeroe();
        bool soyDM = EsDM();

        if (panelHeroe != null)
            panelHeroe.SetActive(soyHeroe);

        if (panelDM != null)
            panelDM.SetActive(soyDM);
    }

    private void ActualizarTextoTurno()
    {
        if (textoTurno == null)
            return;

        if (gameplayManager.TurnOrder == null || gameplayManager.TurnOrder.Count == 0)
        {
            textoTurno.text = "Turno de ...";
            return;
        }

        int index = turnManager.CurrentTurnIndex;

        if (index < 0 || index >= gameplayManager.TurnOrder.Count)
            index = 0;

        string nombreTurno = gameplayManager.TurnOrder[index].PlayerName;

        textoTurno.text = "Turno de " + nombreTurno;
    }

    private void ActualizarBotones()
    {
        bool esMiTurno = turnManager.IsMyTurn();

        if (EsHeroe())
        {
            SetButtonInteractable(botonAtacar, esMiTurno);
            SetButtonInteractable(botonEndTurnHeroe, esMiTurno);
        }

        if (EsDM())
        {
            SetButtonInteractable(botonInvocarEnemigo, esMiTurno);
            SetButtonInteractable(botonInvocarTrampa, esMiTurno);
            SetButtonInteractable(botonEndTurnDM, esMiTurno);
        }
    }

    private void SetButtonInteractable(Button boton, bool estado)
    {
        if (boton != null)
            boton.interactable = estado;
    }

    private bool EsHeroe()
    {
        return gameplayManager.LocalPlayerRole == "Heroe 1" ||
               gameplayManager.LocalPlayerRole == "Heroe 2";
    }

    private bool EsDM()
    {
        return gameplayManager.LocalPlayerRole == "Dungeon Master";
    }

    public void OnEndTurnPressed()
    {
        if (juegoTerminado)
        {
            Debug.Log("GAMEPLAY UI: No puedes finalizar turno. El juego ya terminó.");
            return;
        }

        if (!turnManager.IsMyTurn())
        {
            Debug.Log("GAMEPLAY UI: No es tu turno.");
            return;
        }

        Debug.Log("GAMEPLAY UI: End Turn presionado por " + gameplayManager.LocalPlayerName);

        if (EsDM() && BoardCombatManager.Instance != null)
        {
            BoardCombatManager.Instance.RequestEnemyAttacksBeforeDMEndTurn();
        }

        turnManager.EndTurn();

        ActualizarBotones();
    }

    public void OnAtacarPressed()
    {
        if (juegoTerminado)
        {
            Debug.Log("GAMEPLAY UI: No puedes atacar. El juego ya terminó.");
            return;
        }

        if (!turnManager.IsMyTurn())
        {
            Debug.Log("GAMEPLAY UI: No puedes atacar porque no es tu turno.");
            return;
        }

        Debug.Log("GAMEPLAY UI: Atacar presionado.");

        if (BoardCombatManager.Instance != null)
        {
            BoardCombatManager.Instance.RequestHeroAttackFromButton();
        }
        else
        {
            Debug.LogWarning("GAMEPLAY UI: No existe BoardCombatManager en la escena.");
        }
    }

    public void OnInvocarEnemigoPressed()
    {
        if (!turnManager.IsMyTurn())
        {
            Debug.Log("GAMEPLAY UI: No puedes invocar porque no es tu turno.");
            return;
        }

        Debug.Log("GAMEPLAY UI: Invocar enemigo presionado.");
    }

    public void OnInvocarTrampaPressed()
    {
        if (!turnManager.IsMyTurn())
        {
            Debug.Log("GAMEPLAY UI: No puedes invocar trampa porque no es tu turno.");
            return;
        }

        Debug.Log("GAMEPLAY UI: Invocar trampa presionado.");
    }
    public void BloquearUIFinJuego()
    {
        juegoTerminado = true;
        DesactivarTodosLosBotones();

        Debug.Log("GAMEPLAY UI: Botones bloqueados por fin de juego.");
    }

    private void DesactivarTodosLosBotones()
    {
        SetButtonInteractable(botonAtacar, false);
        SetButtonInteractable(botonEndTurnHeroe, false);

        SetButtonInteractable(botonInvocarEnemigo, false);
        SetButtonInteractable(botonInvocarTrampa, false);
        SetButtonInteractable(botonEndTurnDM, false);
    }
}
