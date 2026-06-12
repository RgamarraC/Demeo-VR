using UnityEngine;

public class VRManager : MonoBehaviour
{
    public GameObject move;

    public Transform player;
    public Transform spawnMenu;

    void Start()
    {
        // al iniciar: desactivar movimiento
        move.SetActive(false);

        ResetPlayer();
    }

    public void EntrarSala()
    {
        // activar movimiento
        move.SetActive(true);
    }

    public void VolverMenu()
    {
        // quitar movimiento
        move.SetActive(false);

        // regresar jugador al menú
        ResetPlayer();
    }

    void ResetPlayer()
    {
        player.position = spawnMenu.position;
        player.rotation = spawnMenu.rotation;
    }
}