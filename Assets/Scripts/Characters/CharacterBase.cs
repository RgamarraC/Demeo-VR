using UnityEngine;

namespace DemeoVR.Gameplay
{
    /// <summary>
    /// Clase base de la que heredan todos los personajes (héroes y enemigos).
    /// Controla el estado local de vida (HP), puntos de acción (AP), mitigación de daño y consumo de recursos.
    /// 
    /// NOTA PARA RED (PHOTON FUSION):
    /// Para sincronizar las variables en multijugador:
    /// 1. Hereda de 'NetworkBehaviour' en lugar de 'MonoBehaviour'.
    /// 2. Sustituye las variables locales 'currentHP' y 'currentAP' por propiedades auto-implementadas:
    ///    [Networked] public int CurrentHP { get; set; }
    ///    [Networked] public int CurrentAP { get; set; }
    /// </summary>
    public class CharacterBase : MonoBehaviour
    {
        #region Configuración de Estadísticas
        [Header("Estadísticas de Diseño")]
        [Tooltip("Referencia al ScriptableObject de estadísticas puras del personaje.")]
        [SerializeField] private CharacterStatsBase statsBase;
        #endregion

        #region Estado en Tiempo Real
        [Header("Estado Actual")]
        [SerializeField] private int currentHP;
        [SerializeField] private int currentAP;

        public int CurrentHP
        {
            get => currentHP;
            protected set => currentHP = Mathf.Clamp(value, 0, statsBase != null ? statsBase.MaxHP : int.MaxValue);
        }

        public int CurrentAP
        {
            get => currentAP;
            protected set => currentAP = Mathf.Clamp(value, 0, statsBase != null ? statsBase.MaxAP : int.MaxValue);
        }

        public CharacterStatsBase StatsBase => statsBase;
        #endregion

        #region Inicialización y Ciclo de Turnos
        protected virtual void Start()
        {
            ResetStats();
        }

        /// <summary>
        /// Inicializa o restablece la salud y los puntos de acción basándose en las estadísticas base.
        /// </summary>
        public virtual void ResetStats()
        {
            if (statsBase != null)
            {
                CurrentHP = statsBase.MaxHP;
                CurrentAP = statsBase.MaxAP;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Falta asignar el CharacterStatsBase.");
            }
        }

        /// <summary>
        /// Comienza el turno restableciendo los puntos de acción (AP) según la estadística base.
        /// </summary>
        public virtual void StartTurn()
        {
            if (statsBase != null)
            {
                CurrentAP = statsBase.MaxAP;
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
            int armor = statsBase != null ? statsBase.Armor : 0;
            int magicRes = statsBase != null ? statsBase.MagicResistance : 0;

            // Mitigación plana estándar (el daño no puede ser negativo)
            int finalPhysDmg = Mathf.Max(0, physDmg - armor);
            int finalMagicDmg = Mathf.Max(0, magicDmg - magicRes);
            int totalDamage = finalPhysDmg + finalMagicDmg;

            CurrentHP -= totalDamage;
            Debug.Log($"[{gameObject.name}] Daño recibido: {totalDamage} (Físico: {finalPhysDmg}, Mágico: {finalMagicDmg}). HP actual: {CurrentHP}");

            if (CurrentHP <= 0)
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
    }
}
