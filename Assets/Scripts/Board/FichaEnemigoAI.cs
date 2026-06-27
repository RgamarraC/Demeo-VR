using UnityEngine;
using DemeoVR.Gameplay; // Por si usas PieceData aquí
using System.Linq;

public class FichaEnemigoAI : MonoBehaviour
{
    [Header("Estadísticas (ScriptableObject)")]
    // NOTA: Usé PieceData porque es el script que me mostraste anteriormente. 
    // Si creaste uno nuevo llamado PersonajeData, simplemente cambia el tipo aquí.
    [SerializeField] private PieceData statsData;
    
    [Header("Restricciones")]
    [Tooltip("Define tanto el rango de visión (aggro) como el límite de casillas que puede saltar.")]
    public int rangoMovimiento = 3;

    [Header("Estado en la Grilla")]
    public int coordenadaX;
    public int coordenadaZ; // Uso Z para mantener consistencia con la refactorización anterior del tablero
    [SerializeField] private int vidaActual;

    private void Start()
    {
        if (statsData != null)
        {
            vidaActual = statsData.maxHealth; // Asumo maxHealth basado en tu PieceData real
        }
    }

    /// <summary>
    /// Método principal llamado por el Gestor de Turnos (TurnManager).
    /// </summary>
    [ContextMenu("Simular Turno (Test)")]
    public void EjecutarTurnoIA()
    {
        Debug.Log($"<color=red>[IA] Turno de {gameObject.name} iniciado.</color>");

        if (GridManager.Instance == null) return;

        // 1. Buscar Héroes en el tablero
        FichaRPG[] todosLosHeroes = Object.FindObjectsByType<FichaRPG>(FindObjectsSortMode.None)
                                          .Where(f => f.esHeroe && f.casillaActual != null).ToArray();

        if (todosLosHeroes.Length == 0) return;

        FichaRPG heroeObjetivo = null;
        int distanciaMinima = int.MaxValue;

        // 2. Calcular Distancia de Visión y fijar Aggro al más cercano
        // El rango de visión es igual al rango de movimiento, según tu indicación.
        foreach (var heroe in todosLosHeroes)
        {
            int distancia = Mathf.Abs(coordenadaX - heroe.casillaActual.coordenadaX) + 
                            Mathf.Abs(coordenadaZ - heroe.casillaActual.coordenadaZ);

            if (distancia <= rangoMovimiento && distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                heroeObjetivo = heroe;
            }
        }

        if (heroeObjetivo == null)
        {
            Debug.Log("[IA] Ningún héroe en rango de visión.");
            return;
        }

        Debug.Log($"[IA] Héroe fijado: {heroeObjetivo.name} a distancia {distanciaMinima}");

        // 3. Buscar Casilla de Ataque Adyacente al héroe
        Vector2Int coordHeroe = new Vector2Int(heroeObjetivo.casillaActual.coordenadaX, heroeObjetivo.casillaActual.coordenadaZ);
        Vector2Int[] direccionesAdyacentes = new Vector2Int[]
        {
            new Vector2Int(0, 1),  // Norte
            new Vector2Int(0, -1), // Sur
            new Vector2Int(1, 0),  // Este
            new Vector2Int(-1, 0)  // Oeste
        };

        CasillaComponent casillaElegida = null;
        int distanciaHaciaCasillaElegida = int.MaxValue;

        foreach (var dir in direccionesAdyacentes)
        {
            Vector2Int coordAdyacente = coordHeroe + dir;

            if (GridManager.Instance.DiccionarioTablero.TryGetValue(coordAdyacente, out CasillaComponent casillaVecina))
            {
                if (!casillaVecina.esObstaculo && !casillaVecina.estaOcupada)
                {
                    // Calculamos a qué distancia está esta casilla de la posición actual del Orco
                    int distOrcoACasilla = Mathf.Abs(coordenadaX - casillaVecina.coordenadaX) + 
                                           Mathf.Abs(coordenadaZ - casillaVecina.coordenadaZ);

                    // Nos quedamos con la casilla libre que nos quede más cerca para caminar
                    if (distOrcoACasilla < distanciaHaciaCasillaElegida)
                    {
                        distanciaHaciaCasillaElegida = distOrcoACasilla;
                        casillaElegida = casillaVecina;
                    }
                }
            }
        }

        // 4. Ejecutar Desplazamiento Físico y Lógico
        // Como validamos que el héroe estuviera dentro de rangoMovimiento, 
        // y nos colocamos en una casilla adyacente a él, 
        // sabemos que también está dentro del rango de salto físico del enemigo.
        if (casillaElegida != null && distanciaHaciaCasillaElegida <= rangoMovimiento)
        {
            // Liberar casilla actual
            Vector2Int coordActual = new Vector2Int(coordenadaX, coordenadaZ);
            if (GridManager.Instance.DiccionarioTablero.TryGetValue(coordActual, out CasillaComponent casillaVieja))
            {
                casillaVieja.estaOcupada = false;
            }

            // Actualizar coordenadas internas del Orco
            coordenadaX = casillaElegida.coordenadaX;
            coordenadaZ = casillaElegida.coordenadaZ;

            // Bloquear nueva casilla
            casillaElegida.estaOcupada = true;

            // Teletransportar al centro
            transform.position = casillaElegida.ObtenerCentro();
            
            Debug.Log($"[IA] Orco movido a la casilla ({coordenadaX}, {coordenadaZ}) y preparado para atacar.");
            
            // Opcional: Actualizar niebla si quieres que el Orco revele zonas, aunque usualmente solo los héroes revelan.
        }
        else
        {
            Debug.Log("[IA] No hay casillas adyacentes libres o están fuera del rango de movimiento.");
        }
    }
}
