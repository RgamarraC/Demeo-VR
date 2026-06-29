using UnityEngine;
using DemeoVR.Gameplay;

public class StatsDebugTester : MonoBehaviour
{
    [Header("Objetivos de prueba")]
    public BoardPiece piezaObjetivo;

    [Header("Daño de prueba")]
    public int physicalDamage = 10;
    public int magicDamage = 0;

    public void ProbarDanio()
    {
        if (piezaObjetivo == null)
        {
            Debug.LogWarning("[StatsDebugTester] No hay piezaObjetivo asignada.");
            return;
        }

        Debug.Log(
            "[StatsDebugTester] Antes del daño: " +
            piezaObjetivo.name +
            " | HP = " + piezaObjetivo.CurrentHealth +
            " | AP = " + piezaObjetivo.CurrentAP
        );

        piezaObjetivo.TakeDamage(physicalDamage, magicDamage);

        Debug.Log(
            "[StatsDebugTester] Después del daño: " +
            piezaObjetivo.name +
            " | HP = " + piezaObjetivo.CurrentHealth +
            " | AP = " + piezaObjetivo.CurrentAP
        );
    }

    public void ProbarInicioTurno()
    {
        if (piezaObjetivo == null)
        {
            Debug.LogWarning("[StatsDebugTester] No hay piezaObjetivo asignada.");
            return;
        }

        piezaObjetivo.StartTurn();

        Debug.Log(
            "[StatsDebugTester] StartTurn aplicado a " +
            piezaObjetivo.name +
            " | AP = " + piezaObjetivo.CurrentAP
        );
    }
}