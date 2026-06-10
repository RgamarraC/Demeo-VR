using UnityEngine;

namespace DemeoVR.Gameplay
{
    /// <summary>
    /// Define los datos y efectos de una carta de habilidad.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCard", menuName = "DemeoVR/CardData")]
    public class CardData : ScriptableObject
    {
        [Header("Información de la Carta")]
        [Tooltip("Nombre de la habilidad.")]
        [SerializeField] private string cardName;

        [TextArea]
        [Tooltip("Descripción del efecto de la carta.")]
        [SerializeField] private string description;

        [Header("Costo de Puntos de Acción")]
        [Range(0, 3)]
        [Tooltip("Puntos de Acción (AP) requeridos por el personaje para jugar esta carta.")]
        [SerializeField] private int apCost = 1;

        [Header("Efectos de Daño")]
        [Tooltip("Daño físico bruto infligido al objetivo.")]
        [SerializeField] private int physicalDamage;

        [Tooltip("Daño mágico bruto infligido al objetivo.")]
        [SerializeField] private int magicDamage;

        [Header("Efectos de Soporte")]
        [Tooltip("Puntos de salud restaurados al objetivo.")]
        [SerializeField] private int healAmount;

        [Tooltip("Escudo de absorción otorgado al objetivo (ej. habilidades de Paladín).")]
        [SerializeField] private int shieldAmount;

        #region Propiedades Públicas
        public string CardName => cardName;
        public string Description => description;
        public int ApCost => apCost;
        public int PhysicalDamage => physicalDamage;
        public int MagicDamage => magicDamage;
        public int HealAmount => healAmount;
        public int ShieldAmount => shieldAmount;
        #endregion
    }
}
