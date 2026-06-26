using UnityEngine;

namespace DemeoVR.Gameplay
{
    /// <summary>
    /// Enfocado en daño a distancia y control de área.
    /// Hereda de BoardPiece y añade mecánicas de amplificación del daño mágico.
    /// </summary>
    public class MageCharacter : BoardPiece
    {
        [Header("Mecánicas del Mago")]
        [Tooltip("Multiplicador actual del daño mágico (1.0 = 100%).")]
        [SerializeField] private float spellPowerMultiplier = 1.0f;

        public float SpellPowerMultiplier
        {
            get => spellPowerMultiplier;
            private set => spellPowerMultiplier = Mathf.Max(0f, value);
        }

        /// <summary>
        /// Sobrescribe el inicio de turno para limpiar cualquier bonificación temporal de poder de hechizo.
        /// </summary>
        public override void StartTurn()
        {
            base.StartTurn();
            // Restablecer el multiplicador de hechizos al valor base del mago al iniciar su turno
            SpellPowerMultiplier = 1.0f;
            Debug.Log($"[{gameObject.name}] Turno iniciado. Poder de hechizos restablecido a 100%.");
        }

        /// <summary>
        /// Retorna el daño mágico base de las estadísticas incrementado por el multiplicador de hechizos actual.
        /// </summary>
        public int GetModifiedMagicDamage()
        {
            int baseMagicDmg = baseData != null ? baseData.magicDamage : 0;
            return Mathf.RoundToInt(baseMagicDmg * SpellPowerMultiplier);
        }

        /// <summary>
        /// Permite amplificar el poder de hechizo de la siguiente carta jugada canalizando AP.
        /// </summary>
        public void AmplifySpellPower(int apCost, float bonusPercentage)
        {
            if (ConsumeAP(apCost))
            {
                SpellPowerMultiplier += bonusPercentage;
                Debug.Log($"[{gameObject.name}] Canalización exitosa. Poder de hechizos aumentado a: {SpellPowerMultiplier * 100}%");
            }
        }
    }
}
