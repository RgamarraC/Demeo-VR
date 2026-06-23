using UnityEngine;

namespace DemeoVR.Gameplay
{
    /// <summary>
    /// Clase base abstracta para cualquier controlador de jugador humano en VR.
    /// Administra el flujo básico del turno y sienta las bases para el Input de Realidad Virtual.
    /// </summary>
    public abstract class PlayerController : MonoBehaviour
    {
        [Header("Estado del Jugador")]
        [Tooltip("Indica si es actualmente el turno de este jugador, permitiéndole interactuar con el tablero.")]
        [SerializeField] protected bool isMyTurn = false;

        public bool IsMyTurn => isMyTurn;

        #region Ciclo de Turnos
        /// <summary>
        /// Inicia el turno del jugador. Permite que el jugador empiece a ejecutar acciones.
        /// Debe ser sobrescrito por las clases hijas para añadir mecánicas específicas.
        /// </summary>
        public virtual void StartTurn()
        {
            isMyTurn = true;
            Debug.Log($"<color=green>[{gameObject.name}] Inicio de turno del jugador.</color>");
            // Aquí se activaría la UI del turno o indicadores visuales frente al jugador en VR.
        }

        /// <summary>
        /// Finaliza el turno del jugador bloqueando nuevas interacciones.
        /// </summary>
        public virtual void EndTurn()
        {
            isMyTurn = false;
            Debug.Log($"<color=yellow>[{gameObject.name}] Fin de turno del jugador.</color>");
            // Aquí se notificaría al TurnManager que este jugador terminó, para pasar a la siguiente fase o jugador.
        }
        #endregion

        #region Interacción VR (Stubs)
        // ====================================================================================
        // LÓGICA PARA INTERACCIÓN EN REALIDAD VIRTUAL (XR Interaction Toolkit)
        // ====================================================================================
        // Cuando implementes los controles físicos (las "manos" del jugador), aquí irían
        // las referencias y eventos para:
        // 
        // 1. Ray Interactor (Puntero láser): 
        //    Permite apuntar a las casillas del tablero, seleccionar personajes objetivos 
        //    y señalar dónde se jugará una carta o se invocará un monstruo.
        //
        // 2. Direct/Grab Interactor: 
        //    Para agarrar físicamente las cartas de la mano (que flotarían cerca del pecho o muñeca) 
        //    y "tirarlas" al tablero simulando un juego de mesa real.
        //
        // 3. Input Actions: 
        //    Suscripciones a los botones de los mandos (ej. Trigger para confirmar acción, 
        //    Grip para agarrar, Joystick para rotar la vista del tablero).
        // ====================================================================================
        #endregion
    }
}
