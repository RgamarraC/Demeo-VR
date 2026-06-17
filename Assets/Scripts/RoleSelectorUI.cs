using UnityEngine;
using TMPro;

public class RoleSelectorUI : MonoBehaviour
{
    public TMP_Dropdown dropdown;

    public PlayerSlotUI player1;

    public void ConfirmarRol()
    {
        string rol = dropdown.options[dropdown.value].text;

        player1.ActualizarSlot(
            LobbyGenerator.hostName,
            rol
        );
    }
}
