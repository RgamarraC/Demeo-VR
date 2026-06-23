using System.Collections.Generic;
using UnityEngine;

namespace DemeoVR.Gameplay
{
    /// <summary>
    /// Fases estándar del juego asimétrico 2vs1.
    /// </summary>
    public enum TurnPhase
    {
        HeroesPhase,
        DungeonMasterPhase
    }

    /// <summary>
    /// Administra el flujo global del juego, alternando los turnos entre los jugadores (Héroes) 
    /// y el antagonista (Dungeon Master).
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        [Header("Fase de Turno")]
        [SerializeField] private TurnPhase currentPhase = TurnPhase.HeroesPhase;

        [Header("Jugadores en la Partida")]
        [Tooltip("Lista de controladores de los jugadores que manejan héroes.")]
        [SerializeField] private List<HeroPlayerController> heroPlayers = new List<HeroPlayerController>();

        [Tooltip("El controlador del jugador que actúa como Dungeon Master.")]
        [SerializeField] private DMPlayerController dungeonMasterPlayer;

        public TurnPhase CurrentPhase => currentPhase;

        #region Inicialización
        private void Start()
        {
            // Inicializar la primera fase al arrancar el juego
            StartPhase(currentPhase);
        }
        #endregion

        #region Control de Fases y Turnos
        /// <summary>
        /// Cambia de fase (de Héroes a Dungeon Master, y viceversa).
        /// </summary>
        public void NextPhase()
        {
            if (currentPhase == TurnPhase.HeroesPhase)
            {
                // Terminar turno de los héroes
                foreach (var hero in heroPlayers)
                {
                    if (hero != null) hero.EndTurn();
                }
                StartPhase(TurnPhase.DungeonMasterPhase);
            }
            else
            {
                // Terminar turno del DM
                if (dungeonMasterPlayer != null) dungeonMasterPlayer.EndTurn();
                StartPhase(TurnPhase.HeroesPhase);
            }
        }

        /// <summary>
        /// Inicia una fase activando a los jugadores correspondientes.
        /// </summary>
        private void StartPhase(TurnPhase newPhase)
        {
            currentPhase = newPhase;
            Debug.Log($"<color=cyan><b>=== INICIANDO FASE: {currentPhase} ===</b></color>");

            if (currentPhase == TurnPhase.HeroesPhase)
            {
                // Activar a todos los jugadores héroes
                foreach (HeroPlayerController heroPlayer in heroPlayers)
                {
                    if (heroPlayer != null)
                    {
                        // Esto cambia su isMyTurn a true y le dice a su miniatura que recargue AP
                        heroPlayer.StartTurn(); 
                    }
                }
            }
            else if (currentPhase == TurnPhase.DungeonMasterPhase)
            {
                // Activar al Dungeon Master
                if (dungeonMasterPlayer != null)
                {
                    // Esto cambia su isMyTurn a true y recarga su Maná Oscuro
                    dungeonMasterPlayer.StartTurn(); 
                }
                else
                {
                    Debug.LogWarning("No hay un DMPlayerController asignado en el TurnManager.");
                    // Si no hay jugador DM físico (ej. modo PvE), aquí se ejecutaría la IA.
                    RunEnemyAI(); 
                }
            }
        }

        /// <summary>
        /// Stub para controlar el comportamiento de enemigos controlados por el sistema (PvE).
        /// </summary>
        private void RunEnemyAI()
        {
            Debug.Log("TurnManager: Ejecutando el comportamiento de los enemigos (IA)...");
            // Al terminar, la IA llamaría a NextPhase() para devolver el turno a los héroes.
        }
        #endregion

        #region Registro de Jugadores
        public void RegisterHeroPlayer(HeroPlayerController heroPlayer)
        {
            if (!heroPlayers.Contains(heroPlayer))
            {
                heroPlayers.Add(heroPlayer);
            }
        }

        public void SetDungeonMaster(DMPlayerController dmPlayer)
        {
            dungeonMasterPlayer = dmPlayer;
        }
        #endregion
    }
}
