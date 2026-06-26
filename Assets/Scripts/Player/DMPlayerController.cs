using UnityEngine;

namespace DemeoVR.Gameplay
{
    /// <summary>
    /// Controlador para el "Dungeon Master" en el modo asimétrico (2vs1).
    /// El DM tiene recursos globales y gestiona el tablero, invocando enemigos o colocando trampas.
    /// </summary>
    public class DMPlayerController : PlayerController
    {
        [Header("Economía del Dungeon Master")]
        [Tooltip("Puntos de Maná Oscuro disponibles para gastar en este turno.")]
        [SerializeField] private int darkMana = 5;

        [Tooltip("Cantidad de Maná Oscuro que regenera automáticamente en cada turno.")]
        [SerializeField] private int darkManaRegeneration = 3;

        [Tooltip("Límite máximo de Maná Oscuro que el DM puede acumular.")]
        [SerializeField] private int maxDarkMana = 10;

        [Header("Sistema de Tiers (Desbloqueo de Monstruos)")]
        [Tooltip("Niveles que deben alcanzar los héroes para desbloquear cada Tier. El índice 0 es el Tier 1 (siempre disponible), el índice 1 es el Tier 2, etc.")]
        [SerializeField] public int[] tierUnlockLevels = { 1, 3, 5, 8 };

        public int DarkMana => darkMana;

        #region Sobrescritura de Turno
        /// <summary>
        /// Inicia el turno del DM y recarga sus recursos de Maná Oscuro.
        /// </summary>
        public override void StartTurn()
        {
            // Marca isMyTurn = true desde la clase base
            base.StartTurn();

            // Regenerar economía del DM
            darkMana += darkManaRegeneration;
            if (darkMana > maxDarkMana)
            {
                darkMana = maxDarkMana;
            }

            Debug.Log($"<color=purple>[{gameObject.name}] Turno del Dungeon Master. Maná Oscuro actual: {darkMana}/{maxDarkMana}</color>");
        }
        #endregion

        #region Lógica de Interacción del DM (Stubs)
        /// <summary>
        /// Permite al Dungeon Master gastar recursos para invocar una nueva criatura en el tablero.
        /// </summary>
        public void SpawnMonster()
        {
            if (!isMyTurn) return;

            // ====================================================================================
            // LÓGICA PARA INVOCAR MONSTRUOS:
            // ====================================================================================
            // 1. Costo: Validar que `darkMana` >= costo de la unidad seleccionada. Restar el maná.
            // 2. Apuntar (VR): Usar el puntero para elegir una casilla (`Vector3Int`).
            // 3. GridManager: Validar que la casilla no tiene obstáculos ni otras miniaturas.
            // 4. Fog of War: (Regla clave) Validar que la casilla esté cubierta por Niebla de Guerra. 
            //    Los héroes no deben ver aparecer a los monstruos mágicamente frente a ellos.
            // 5. Instanciar: Crear el prefab del Enemigo (`BoardPiece`), asignarle sus Stats
            //    y colocarlo visual y lógicamente en esa coordenada y registrarlo en el `TurnManager`.
            // ====================================================================================

            Debug.Log($"[{gameObject.name}] Acción del DM: Intentando invocar monstruo... (Pendiente de Lógica de Tablero)");
        }

        /// <summary>
        /// Permite al Dungeon Master armar o colocar trampas en el entorno.
        /// </summary>
        public void ActivateTrap()
        {
            if (!isMyTurn) return;

            // ====================================================================================
            // LÓGICA PARA TRAMPAS DEL MAPA:
            // ====================================================================================
            // 1. Costo: Descontar `darkMana`.
            // 2. Apuntar (VR): Apuntar hacia un objeto interactuable (ej. un barril explosivo o baldosas falsas).
            // 3. Armado: Cambiar el estado del objeto a "Armado/Activo". El objeto podría 
            //    volverse invisible o cambiar de color sutilmente.
            // 4. Detonación diferida: El GridManager suscribirá a un evento. Cuando la ficha
            //    de un Héroe (`BoardPiece`) pise esa coordenada en un futuro turno, la 
            //    trampa se activará aplicando efectos (daño, root, etc.) en área.
            // ====================================================================================

            Debug.Log($"[{gameObject.name}] Acción del DM: Intentando armar trampa... (Pendiente de Lógica de Tablero)");
        }
        #endregion

        #region Sistema de Tiers
        /// <summary>
        /// Escanea el tablero para encontrar el nivel máximo de los héroes y determina qué Tier de monstruos puede invocar el DM.
        /// </summary>
        public int GetCurrentTier()
        {
            int highestHeroLevel = 1;

            // Busca todos los BoardPieces en la escena y toma el nivel más alto de los héroes
            BoardPiece[] allPieces = FindObjectsByType<BoardPiece>(FindObjectsSortMode.None);
            foreach (BoardPiece piece in allPieces)
            {
                if (piece.Type == PieceType.Heroe1 || piece.Type == PieceType.Heroe2)
                {
                    if (piece.CurrentLevel > highestHeroLevel)
                    {
                        highestHeroLevel = piece.CurrentLevel;
                    }
                }
            }

            int currentTier = 1;
            // Evaluamos en qué Tier cae este nivel
            for (int i = 0; i < tierUnlockLevels.Length; i++)
            {
                if (highestHeroLevel >= tierUnlockLevels[i])
                {
                    currentTier = i + 1; // Tier 1, Tier 2, Tier 3...
                }
                else
                {
                    break;
                }
            }

            return currentTier;
        }
        #endregion
    }
}
