using UnityEngine;
using TMPro;
using System;

public class RoleSelectorUI : MonoBehaviour
{
    public TMP_Dropdown dropdown;

    public void ConfirmarRol()
    {
        if (dropdown == null)
        {
            Debug.LogError("RoleSelectorUI: falta asignar el Dropdown.");
            return;
        }

        string rolSeleccionado = dropdown.options[dropdown.value].text.Trim();

        if (string.IsNullOrEmpty(rolSeleccionado))
            rolSeleccionado = "Sin rol";

        LobbyPlayerData jugadorLocal = BuscarJugadorLocal();

        if (jugadorLocal == null)
        {
            Debug.LogWarning("No se encontró el jugador local para cambiar rol.");
            return;
        }

        if (RolYaTomadoPorOtroJugador(rolSeleccionado, jugadorLocal))
        {
            Debug.LogWarning("Ese rol ya está ocupado: " + rolSeleccionado);
            return;
        }

        jugadorLocal.CambiarRolLocal(rolSeleccionado);

        Debug.Log("Rol seleccionado: " + rolSeleccionado);
    }

    private LobbyPlayerData BuscarJugadorLocal()
    {
        LobbyPlayerData[] players =
            FindObjectsByType<LobbyPlayerData>(FindObjectsSortMode.None);

        foreach (LobbyPlayerData player in players)
        {
            if (player == null)
                continue;

            if (player.Object == null)
                continue;

            if (player.Object.HasInputAuthority)
                return player;
        }

        return null;
    }

    private bool RolYaTomadoPorOtroJugador(string rolSeleccionado, LobbyPlayerData jugadorLocal)
    {
        if (string.IsNullOrEmpty(rolSeleccionado))
            return false;

        if (rolSeleccionado == "Sin rol")
            return false;

        LobbyPlayerData[] players =
            FindObjectsByType<LobbyPlayerData>(FindObjectsSortMode.None);

        foreach (LobbyPlayerData player in players)
        {
            if (player == null)
                continue;

            if (player == jugadorLocal)
                continue;

            string rolActual = "";

            try
            {
                rolActual = player.PlayerRole.ToString().Trim();
            }
            catch (InvalidOperationException)
            {
                continue;
            }

            if (string.IsNullOrEmpty(rolActual))
                continue;

            if (rolActual == "Sin rol")
                continue;

            if (string.Equals(rolActual, rolSeleccionado, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}