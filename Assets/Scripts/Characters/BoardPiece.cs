using UnityEngine;
using Fusion;

namespace DemeoVR.Gameplay
{
    /// <summary>
    /// Clase base de la que heredan todas las piezas del tablero (héroes, enemigos, trampas).
    /// Controla el estado local de vida (HP) sincronizado en red mediante Photon Fusion 2,
    /// y expone estadísticas base desde PieceData.
    /// </summary>
    public class BoardPiece : NetworkBehaviour
    {
        #region Configuración de Estadísticas
        [Header("Estadísticas de Diseño")]
        [Tooltip("Referencia al ScriptableObject de estadísticas puras de la pieza.")]
        public PieceData baseData;
        #endregion

        #region Estado en Tiempo Real (Networked)
        [Header("Estado Actual (Sincronizado en Red)")]
        [Networked] public int CurrentHealth { get; set; }
        [Networked] public int CurrentAP { get; set; }
        [Networked] public int CurrentLevel { get; set; }
        [Networked] public int CurrentXP { get; set; }
        #endregion

        #region Propiedades de Estadísticas
        // Expone el daño y las defensas mitigadas directamente desde este script leyendo de baseData y escalando por nivel
        public int MaxHealth => baseData != null ? baseData.maxHealth + ((CurrentLevel - 1) * baseData.hpGrowth) : 0;
        public int PhysicalDamage => baseData != null ? baseData.physicalDamage + ((CurrentLevel - 1) * baseData.physicalDamageGrowth) : 0;
        public int MagicDamage => baseData != null ? baseData.magicDamage + ((CurrentLevel - 1) * baseData.magicDamageGrowth) : 0;
        public int Armor => baseData != null ? baseData.armor + ((CurrentLevel - 1) * baseData.armorGrowth) : 0;
        public int MagicResistance => baseData != null ? baseData.magicResistance + ((CurrentLevel - 1) * baseData.magicResistanceGrowth) : 0;
        public PieceType Type => baseData != null ? baseData.pieceType : PieceType.Enemigo;
        #endregion

        #region Inicialización y Ciclo de Turnos
        public override void Spawned()
        {
            CurrentLevel = 1;
            CurrentXP = 0;

            if (baseData != null)
            {
                CurrentHealth = MaxHealth;
                CurrentAP = baseData.maxAP;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Falta asignar el PieceData en baseData.");
            }
        }

        /// <summary>
        /// Comienza el turno restableciendo los puntos de acción (AP) según la estadística base.
        /// </summary>
        public virtual void StartTurn()
        {
            if (baseData != null)
            {
                CurrentAP = baseData.maxAP;
                Debug.Log($"[{gameObject.name}] Inicio de turno: AP restablecidos a {CurrentAP}.");
            }
        }
        #endregion

        #region Lógica de Daño y Mitigación
        /// <summary>
        /// Aplica daño al personaje calculando reducciones por Armadura y Resistencia Mágica.
        /// </summary>
        /// <param name="physDmg">Daño físico entrante bruto.</param>
        /// <param name="magicDmg">Daño mágico entrante bruto.</param>
        public virtual void TakeDamage(int physDmg, int magicDmg)
        {
            // Mitigación plana estándar (el daño no puede ser negativo)
            int finalPhysDmg = Mathf.Max(0, physDmg - Armor);
            int finalMagicDmg = Mathf.Max(0, magicDmg - MagicResistance);
            int totalDamage = finalPhysDmg + finalMagicDmg;

            CurrentHealth -= totalDamage;
            Debug.Log($"[{gameObject.name}] Daño recibido: {totalDamage} (Físico: {finalPhysDmg}, Mágico: {finalMagicDmg}). HP actual: {CurrentHealth}");

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Lógica cuando el personaje se queda sin salud.
        /// </summary>
        protected virtual void Die()
        {
            Debug.LogWarning($"[{gameObject.name}] Ha muerto.");
            // Aquí se pueden disparar eventos de muerte, desactivar físicas o activar animaciones.
        }
        #endregion

        #region Lógica de Habilidades y Economía de Acciones (Cartas)
        /// <summary>
        /// Consume Puntos de Acción (AP) al jugar cartas de habilidad u otras acciones.
        /// </summary>
        /// <param name="cost">Costo de AP de la acción.</param>
        /// <returns>True si el consumo fue exitoso; False si no hay suficientes AP.</returns>
        public virtual bool ConsumeAP(int cost)
        {
            if (CurrentAP >= cost)
            {
                CurrentAP -= cost;
                Debug.Log($"[{gameObject.name}] Consumió {cost} AP. AP restantes: {CurrentAP}");
                return true;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] AP insuficientes. Requiere: {cost}, Disponible: {CurrentAP}");
                return false;
            }
        }
        #endregion

        #region Sistema de Experiencia
        /// <summary>
        /// Otorga XP al personaje. Si supera el umbral, sube de nivel y cura su vida al nuevo máximo.
        /// (Solo el Host / StateAuthority debe ejecutar esto)
        /// </summary>
        public void EarnXP(int amount)
        {
            if (!HasStateAuthority || baseData == null) return;

            CurrentXP += amount;
            Debug.Log($"[{gameObject.name}] Ganó {amount} XP. Total XP: {CurrentXP}");

            // Verificar si sube de nivel (mientras haya requisitos configurados y se supere el umbral)
            while (CurrentLevel <= baseData.xpRequirements.Length && CurrentXP >= baseData.xpRequirements[CurrentLevel - 1])
            {
                CurrentXP -= baseData.xpRequirements[CurrentLevel - 1];
                CurrentLevel++;
                CurrentHealth = MaxHealth; // Curar por completo al nuevo máximo escalado

                Debug.Log($"<color=yellow>[{gameObject.name}] ¡Subió de nivel! Ahora es nivel {CurrentLevel}. Vida restaurada a {CurrentHealth}.</color>");
            }
        }
        #endregion
    }
}
