using UnityEngine;

namespace DemeoVR.Gameplay
{
    /// <summary>
    /// ScriptableObject que define las estadísticas puras y fijas de diseño de un personaje.
    /// No contiene lógica de posicionamiento ni del tablero físico.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacterStats", menuName = "DemeoVR/CharacterStatsBase")]
    public class CharacterStatsBase : ScriptableObject
    {
        [Header("Vida y Economía de Turno")]
        [Tooltip("Vida máxima (HP) del personaje.")]
        [SerializeField] private int maxHP = 100;

        [Tooltip("Puntos de Acción (AP) máximos que recibe el personaje al iniciar su turno.")]
        [SerializeField] private int maxAP = 2;

        [Header("Atributos de Daño")]
        [Tooltip("Daño físico base del personaje.")]
        [SerializeField] private int physicalDamage = 10;

        [Tooltip("Daño mágico base del personaje.")]
        [SerializeField] private int magicDamage = 5;

        [Header("Mitigación de Daño")]
        [Tooltip("Armadura física (reducción plana de daño físico entrante).")]
        [SerializeField] private int armor = 2;

        [Tooltip("Resistencia mágica (reducción plana de daño mágico entrante).")]
        [SerializeField] private int magicResistance = 1;

        #region Propiedades Públicas de Solo Lectura (Getters)
        public int MaxHP => maxHP;
        public int MaxAP => maxAP;
        public int PhysicalDamage => physicalDamage;
        public int MagicDamage => magicDamage;
        public int Armor => armor;
        public int MagicResistance => magicResistance;
        #endregion
    }
}
