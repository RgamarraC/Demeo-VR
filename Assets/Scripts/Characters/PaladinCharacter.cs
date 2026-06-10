using UnityEngine;

namespace DemeoVR.Gameplay
{
    /// <summary>
    /// Especialista en resistencia y combate cuerpo a cuerpo.
    /// Hereda de CharacterBase y añade soporte para mitigación de daño mejorada mediante escudos de absorción.
    /// </summary>
    public class PaladinCharacter : CharacterBase
    {
        [Header("Mecánicas del Paladín")]
        [Tooltip("Escudo de absorción de daño actual. Actúa como puntos de salud temporales.")]
        [SerializeField] private int shieldPoints;

        public int ShieldPoints
        {
            get => shieldPoints;
            private set => shieldPoints = Mathf.Max(0, value);
        }

        /// <summary>
        /// Sobrescribe la reducción de daño para que las mitigaciones físicas/mágicas
        /// se apliquen primero y el daño final sea absorbido prioritariamente por el escudo del Paladín.
        /// </summary>
        public override void TakeDamage(int physDmg, int magicDmg)
        {
            int armor = statsBase != null ? statsBase.Armor : 0;
            int magicRes = statsBase != null ? statsBase.MagicResistance : 0;

            int finalPhysDmg = Mathf.Max(0, physDmg - armor);
            int finalMagicDmg = Mathf.Max(0, magicDmg - magicRes);
            int totalDamage = finalPhysDmg + finalMagicDmg;

            if (ShieldPoints > 0)
            {
                if (ShieldPoints >= totalDamage)
                {
                    ShieldPoints -= totalDamage;
                    Debug.Log($"[{gameObject.name}] El escudo absorbió todo el daño ({totalDamage}). Escudo restante: {ShieldPoints}");
                    return;
                }
                else
                {
                    totalDamage -= ShieldPoints;
                    Debug.Log($"[{gameObject.name}] El escudo absorbió {ShieldPoints} de daño y se rompió.");
                    ShieldPoints = 0;
                }
            }

            // Aplicar el daño restante a la vida
            CurrentHP -= totalDamage;
            Debug.Log($"[{gameObject.name}] Daño directo recibido: {totalDamage}. HP actual: {CurrentHP}");

            if (CurrentHP <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Otorga escudo temporal al Paladín (ej. al jugar una carta defensiva).
        /// </summary>
        public void GainShield(int amount)
        {
            ShieldPoints += amount;
            Debug.Log($"[{gameObject.name}] Ganó {amount} puntos de escudo. Escudo actual: {ShieldPoints}");
        }
    }
}
