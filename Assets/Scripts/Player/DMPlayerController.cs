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
            // 5. Instanciar: Crear el prefab del Enemigo (`CharacterBase`), asignarle sus Stats
            //    y registrarlo en la lista de enemigos del `TurnManager`.
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
            //    de un Héroe (`CharacterBase`) pise esa coordenada en un futuro turno, la 
            //    trampa explotará aplicando `TakeDamage()` en área.
            // ====================================================================================

            Debug.Log($"[{gameObject.name}] Acción del DM: Intentando armar trampa... (Pendiente de Lógica de Tablero)");
        }
        #endregion
    }
}
