using TMPro;
using UnityEngine;

public class PlayerSlotUI : MonoBehaviour
{
    public TMP_Text nombre;
    public TMP_Text rol;

    public void ActualizarSlot(string jugador, string rolJugador)
    {
        if (nombre == null)
        {
            Debug.LogError("PlayerSlotUI: falta asignar el TMP_Text de nombre en " + gameObject.name);
            return;
        }

        if (rol == null)
        {
            Debug.LogError("PlayerSlotUI: falta asignar el TMP_Text de rol en " + gameObject.name);
            return;
        }

        nombre.text = jugador;
        rol.text = rolJugador;
    }
}