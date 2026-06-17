using UnityEngine;
using TMPro;

public class JoinLobbyUI : MonoBehaviour
{
    public TMP_InputField nombreInput;
    public TMP_InputField codigoInput;

    public static string nombreJugador;
    public static string codigoIngresado;

    public void EntrarSala()
    {
        nombreJugador = nombreInput.text.Trim();
        codigoIngresado = codigoInput.text.Trim();

        if (nombreJugador == "")
        {
            Debug.Log("Falta nombre");
            return;
        }

        if (codigoIngresado.Length != 4)
        {
            Debug.Log("Código inválido");
            return;
        }

        Debug.Log("Nombre: " + nombreJugador);
        Debug.Log("Código: " + codigoIngresado);

        // después aquí conectaremos Photon
    }
}