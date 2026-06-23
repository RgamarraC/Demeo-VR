using System.Collections.Generic;
using UnityEngine;

namespace DemeoVR.Gameplay
{
    /// <summary>
    /// Controlador para los jugadores que manejan a los héroes del tablero.
    /// Administra la mano de cartas físicas y dicta acciones a la miniatura que controla.
    /// </summary>
    public class HeroPlayerController : PlayerController
    {
        [Header("Personaje Controlado")]
        [Tooltip("La miniatura o ficha del tablero que este jugador humano maneja.")]
        [SerializeField] private CharacterBase controlledCharacter;

        [Header("Mano del Jugador")]
        [Tooltip("Listado de cartas que el jugador tiene actualmente en su mano virtual.")]
        [SerializeField] private List<CardData> handOfCards = new List<CardData>();

        #region Propiedades Públicas
        public CharacterBase ControlledCharacter
        {
            get => controlledCharacter;
            set => controlledCharacter = value;
        }

        public List<CardData> HandOfCards => handOfCards;
        #endregion

        #region Sobrescritura de Turno
        /// <summary>
        /// Inicia el turno del jugador y delega la recarga de AP a la miniatura controlada.
        /// </summary>
        public override void StartTurn()
        {
            // Cambia el isMyTurn a true usando la clase base
            base.StartTurn();

            // Indicar a la ficha/miniatura que inicia su turno (recarga sus AP)
            if (controlledCharacter != null)
            {
                controlledCharacter.StartTurn();
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] ¡Alerta! El jugador héroe no tiene asignada ninguna miniatura (CharacterBase) al iniciar su turno.");
            }
        }
        #endregion

        #region Lógica de Interacción con Cartas
        /// <summary>
        /// Intenta jugar una carta desde la mano VR sobre un personaje objetivo en el tablero.
        /// </summary>
        public bool PlayCard(CardData card, CharacterBase target)
        {
            if (!isMyTurn)
            {
                Debug.LogWarning($"[{gameObject.name}] Debes esperar tu turno para jugar cartas.");
                return false;
            }

            if (controlledCharacter == null) return false;
            if (!handOfCards.Contains(card)) return false;
            if (target == null) return false;

            // 1. Validar y descontar AP del personaje en el tablero
            if (!controlledCharacter.ConsumeAP(card.ApCost))
            {
                Debug.LogWarning($"[{gameObject.name}] La ficha '{controlledCharacter.name}' no tiene AP suficiente.");
                return false;
            }

            // 2. Ejecutar efectos de la carta
            ApplyCardEffects(card, target);

            // 3. Descartar la carta de la mano visual/lógica
            handOfCards.Remove(card);

            return true;
        }

        private void ApplyCardEffects(CardData card, CharacterBase target)
        {
            // Daño
            if (card.PhysicalDamage > 0 || card.MagicDamage > 0)
            {
                int finalMagicDmg = card.MagicDamage;
                if (controlledCharacter is MageCharacter mage && card.MagicDamage > 0)
                {
                    finalMagicDmg = mage.GetModifiedMagicDamage();
                }
                target.TakeDamage(card.PhysicalDamage, finalMagicDmg);
            }

            // Soporte/Escudo (Paladín)
            if (card.ShieldAmount > 0 && target is PaladinCharacter paladin)
            {
                paladin.GainShield(card.ShieldAmount);
            }
        }
        #endregion
    }
}
