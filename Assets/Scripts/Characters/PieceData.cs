using UnityEngine;

namespace DemeoVR.Gameplay
{
    public enum PieceType
    {
        Heroe1,
        Heroe2,
        Enemigo,
        Trampa
    }

    /// <summary>
    /// ScriptableObject que define las estadísticas puras y fijas de diseño de una pieza de tablero.
    /// </summary>
    [CreateAssetMenu(fileName = "NewPieceData", menuName = "DemeoVR/PieceData")]
    public class PieceData : ScriptableObject
    {
        [Header("Tipo de Pieza")]
        public PieceType pieceType;

        [Header("Estadísticas Base")]
        [Tooltip("Vida máxima (HP) de la pieza.")]
        [SerializeField] public int maxHealth = 100;

        [Tooltip("Daño físico base de la pieza.")]
        [SerializeField] public int physicalDamage = 10;

        [Tooltip("Daño mágico base de la pieza.")]
        [SerializeField] public int magicDamage = 5;

        [Header("Mitigación de Daño")]
        [Tooltip("Armadura física (reducción plana de daño físico entrante).")]
        [SerializeField] public int armor = 2;

        [Tooltip("Resistencia mágica (reducción plana de daño mágico entrante).")]
        [SerializeField] public int magicResistance = 1;

        [Header("Economía de Turno")]
        [Tooltip("Puntos de Acción (AP) máximos que recibe el personaje al iniciar su turno.")]
        [SerializeField] public int maxAP = 2;

        [Header("Sistema de Progresión (Héroes)")]
        [Tooltip("Crecimiento de Vida máxima por nivel.")]
        [SerializeField] public int hpGrowth = 20;

        [Tooltip("Crecimiento de Daño físico por nivel.")]
        [SerializeField] public int physicalDamageGrowth = 3;

        [Tooltip("Crecimiento de Daño mágico por nivel.")]
        [SerializeField] public int magicDamageGrowth = 2;

        [Tooltip("Crecimiento de Armadura por nivel.")]
        [SerializeField] public int armorGrowth = 1;

        [Tooltip("Crecimiento de Resistencia mágica por nivel.")]
        [SerializeField] public int magicResistanceGrowth = 1;

        [Tooltip("Requisitos de XP para subir de nivel (El índice 0 es para pasar a Nivel 2).")]
        [SerializeField] public int[] xpRequirements = { 100, 200, 400, 800 };

        [Header("Sistema de Recompensas (Enemigos)")]
        [Tooltip("Experiencia que otorga este personaje al ser derrotado.")]
        [SerializeField] public int xpReward = 50;
    }
}
