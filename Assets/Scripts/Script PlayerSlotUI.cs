using TMPro;
using UnityEngine;

public class PlayerSlotUI : MonoBehaviour
{
    public TMP_Text nombre;
    public TMP_Text rol;

    public void ActualizarSlot(
        string jugador,
        string rolJugador
    )
    {
        nombre.text = jugador;
        rol.text = rolJugador;
    }
}