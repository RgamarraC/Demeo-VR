using System.Collections.Generic;
using UnityEngine;
using Fusion;
using DemeoVR.Gameplay;

public enum HeroCardId
{
    None = 0,

    PaladinEmbestida = 1,
    PaladinGolpeEscudo = 2,
    PaladinJuramentoSagrado = 3,

    MagoBolaFuego = 4,
    MagoAtaqueRayo = 5,
    MagoNevisca = 6
}

public class HeroCardManager : NetworkBehaviour
{
    public static HeroCardManager Instance;

    [Header("Configuración")]
    [SerializeField] private int costoAPCarta = 2;
    [SerializeField] private int maxCartasMano = 4;

    [Header("Mano Heroe 1")]
    [Networked] public int Heroe1CardCount { get; set; }
    [Networked] public int Heroe1Card0 { get; set; }
    [Networked] public int Heroe1Card1 { get; set; }
    [Networked] public int Heroe1Card2 { get; set; }
    [Networked] public int Heroe1Card3 { get; set; }

    [Header("Mano Heroe 2")]
    [Networked] public int Heroe2CardCount { get; set; }
    [Networked] public int Heroe2Card0 { get; set; }
    [Networked] public int Heroe2Card1 { get; set; }
    [Networked] public int Heroe2Card2 { get; set; }
    [Networked] public int Heroe2Card3 { get; set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Heroe1CardCount = 0;
            Heroe2CardCount = 0;

            Heroe1Card0 = 0;
            Heroe1Card1 = 0;
            Heroe1Card2 = 0;
            Heroe1Card3 = 0;

            Heroe2Card0 = 0;
            Heroe2Card1 = 0;
            Heroe2Card2 = 0;
            Heroe2Card3 = 0;
        }

        Debug.Log(
            "[HeroCardManager] Spawned. StateAuthority = " +
            Object.HasStateAuthority
        );
    }

    // =====================================================
    // RECOMPENSA AL MATAR ENEMIGO
    // =====================================================

    public void TryRewardCardIfEnemyKilled(BoardPiece enemyStats, string rolHeroe)
    {
        if (!Object.HasStateAuthority)
            return;

        if (enemyStats == null)
            return;

        if (enemyStats.CurrentHealth > 0)
            return;

        if (!enemyStats.TryMarkRewardGiven())
            return;

        TryAddRandomCardForRole(rolHeroe);
    }

    public void TryAddRandomCardForRole(string rolHeroe)
    {
        if (!Object.HasStateAuthority)
            return;

        if (!EsRolHeroe(rolHeroe))
            return;

        int cantidadActual = GetCardCount(rolHeroe);

        if (cantidadActual >= maxCartasMano)
        {
            Debug.Log(
                "[HeroCardManager HOST] No se agregó carta. Mano llena. Rol = " +
                rolHeroe
            );

            return;
        }

        HeroCardId nuevaCarta = ObtenerCartaAleatoriaParaRol(rolHeroe);

        SetCardAt(rolHeroe, cantidadActual, (int)nuevaCarta);
        SetCardCount(rolHeroe, cantidadActual + 1);

        Debug.Log(
            "[HeroCardManager HOST] Carta agregada. " +
            "Rol = " + rolHeroe +
            " | Carta = " + GetCardName(nuevaCarta) +
            " | Cantidad = " + (cantidadActual + 1)
        );
    }

    private HeroCardId ObtenerCartaAleatoriaParaRol(string rolHeroe)
    {
        if (rolHeroe == "Heroe 1")
        {
            int random = Random.Range(1, 4);

            if (random == 1) return HeroCardId.PaladinEmbestida;
            if (random == 2) return HeroCardId.PaladinGolpeEscudo;

            return HeroCardId.PaladinJuramentoSagrado;
        }

        if (rolHeroe == "Heroe 2")
        {
            int random = Random.Range(1, 4);

            if (random == 1) return HeroCardId.MagoBolaFuego;
            if (random == 2) return HeroCardId.MagoAtaqueRayo;

            return HeroCardId.MagoNevisca;
        }

        return HeroCardId.None;
    }

    // =====================================================
    // USAR CARTA
    // =====================================================

    public void RequestUseCard(int cardIndex)
    {
        if (GameplayManager.Instance == null || TurnManager.Instance == null)
        {
            Debug.LogWarning("[HeroCardManager] No hay GameplayManager o TurnManager.");
            return;
        }

        if (GameEndManager.Instance != null && GameEndManager.Instance.JuegoTerminado)
        {
            Debug.LogWarning("[HeroCardManager] No puedes usar cartas. El juego terminó.");
            return;
        }

        if (!TurnManager.Instance.IsMyTurn())
        {
            Debug.LogWarning("[HeroCardManager] No puedes usar carta porque no es tu turno.");
            return;
        }

        string rolLocal = GameplayManager.Instance.LocalPlayerRole;

        if (!EsRolHeroe(rolLocal))
        {
            Debug.LogWarning("[HeroCardManager] Solo héroes pueden usar cartas.");
            return;
        }

        RPC_RequestUseCard(Runner.LocalPlayer, cardIndex);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestUseCard(PlayerRef requester, int cardIndex)
    {
        string rolRequester = ObtenerRolDePlayer(requester);

        Debug.Log(
            "[HeroCardManager HOST] Solicitud de carta. " +
            "Requester = " + requester +
            " | Rol = " + rolRequester +
            " | Index = " + cardIndex
        );

        if (!EsRolHeroe(rolRequester))
        {
            Debug.LogWarning("[HeroCardManager HOST] Rechazado. No es héroe.");
            return;
        }

        if (!EsTurnoDePlayer(requester))
        {
            Debug.LogWarning("[HeroCardManager HOST] Rechazado. No es su turno.");
            return;
        }

        int cantidad = GetCardCount(rolRequester);

        if (cardIndex < 0 || cardIndex >= cantidad)
        {
            Debug.LogWarning("[HeroCardManager HOST] Rechazado. Índice de carta inválido.");
            return;
        }

        HeroCardId carta = (HeroCardId)GetCardAt(rolRequester, cardIndex);

        if (carta == HeroCardId.None)
        {
            Debug.LogWarning("[HeroCardManager HOST] Rechazado. Slot vacío.");
            return;
        }

        FichaRPG fichaHeroe = BuscarHeroePorRol(rolRequester);

        if (fichaHeroe == null)
        {
            Debug.LogWarning("[HeroCardManager HOST] No se encontró ficha del héroe.");
            return;
        }

        BoardPiece statsHeroe = ObtenerBoardPiece(fichaHeroe);

        if (statsHeroe == null)
        {
            Debug.LogWarning("[HeroCardManager HOST] La ficha del héroe no tiene BoardPiece.");
            return;
        }

        if (statsHeroe.CurrentAP < costoAPCarta)
        {
            Debug.LogWarning(
                "[HeroCardManager HOST] AP insuficiente. " +
                "AP actual = " + statsHeroe.CurrentAP +
                " | Costo carta = " + costoAPCarta
            );

            return;
        }

        if (!CartaTieneObjetivoValido(carta, fichaHeroe))
        {
            Debug.LogWarning(
                "[HeroCardManager HOST] No hay objetivo válido para " +
                GetCardName(carta)
            );

            return;
        }

        if (!statsHeroe.ConsumeAP(costoAPCarta))
            return;

        RemoveCardAt(rolRequester, cardIndex);

        EjecutarCarta(carta, fichaHeroe, statsHeroe, rolRequester);
        statsHeroe.MarkAttackUsed();

        Debug.Log(
            "[HeroCardManager HOST] Carta usada. " +
            "Rol = " + rolRequester +
            " | Carta = " + GetCardName(carta)
        );
    }

    private bool CartaTieneObjetivoValido(HeroCardId carta, FichaRPG fichaHeroe)
    {
        if (carta == HeroCardId.PaladinJuramentoSagrado)
            return true;

        if (carta == HeroCardId.PaladinEmbestida)
            return BuscarEnemigosEnLineaRecta(fichaHeroe, 2).Count > 0;

        if (carta == HeroCardId.PaladinGolpeEscudo)
            return BuscarEnemigoMasCercano(fichaHeroe, 1) != null;

        if (carta == HeroCardId.MagoBolaFuego)
            return BuscarEnemigoMasCercano(fichaHeroe, 3) != null;

        if (carta == HeroCardId.MagoAtaqueRayo)
            return BuscarEnemigoMasCercano(fichaHeroe, 2) != null;

        if (carta == HeroCardId.MagoNevisca)
            return BuscarEnemigoMasCercano(fichaHeroe, 2) != null;

        return false;
    }

    private void EjecutarCarta(
        HeroCardId carta,
        FichaRPG fichaHeroe,
        BoardPiece statsHeroe,
        string rolHeroe
    )
    {
        if (carta == HeroCardId.PaladinEmbestida)
        {
            List<FichaEnemigoAI> enemigos = BuscarEnemigosEnLineaRecta(fichaHeroe, 2);

            foreach (FichaEnemigoAI enemigo in enemigos)
            {
                BoardPiece statsEnemigo = ObtenerBoardPiece(enemigo);

                if (statsEnemigo == null)
                    continue;

                statsEnemigo.TakeDamage(40, 0);
                TryRewardCardIfEnemyKilled(statsEnemigo, rolHeroe);
            }

            Debug.Log("[HeroCardManager HOST] Embestida ejecutada.");
        }
        else if (carta == HeroCardId.PaladinGolpeEscudo)
        {
            FichaEnemigoAI enemigo = BuscarEnemigoMasCercano(fichaHeroe, 1);

            BoardPiece statsEnemigo = ObtenerBoardPiece(enemigo);

            statsEnemigo.TakeDamage(20, 0);
            statsEnemigo.ApplyStun();

            TryRewardCardIfEnemyKilled(statsEnemigo, rolHeroe);

            Debug.Log("[HeroCardManager HOST] Golpe de escudo ejecutado.");
        }
        else if (carta == HeroCardId.PaladinJuramentoSagrado)
        {
            statsHeroe.ApplyDamageReduction(40);

            Debug.Log("[HeroCardManager HOST] Juramento sagrado ejecutado.");
        }
        else if (carta == HeroCardId.MagoBolaFuego)
        {
            FichaEnemigoAI objetivo = BuscarEnemigoMasCercano(fichaHeroe, 3);

            BoardPiece statsObjetivo = ObtenerBoardPiece(objetivo);
            statsObjetivo.TakeDamage(0, 80);
            TryRewardCardIfEnemyKilled(statsObjetivo, rolHeroe);

            FichaEnemigoAI[] enemigos =
                FindObjectsByType<FichaEnemigoAI>(FindObjectsSortMode.None);

            foreach (FichaEnemigoAI enemigo in enemigos)
            {
                if (enemigo == objetivo)
                    continue;

                if (EstaAdyacenteAEnemigo(enemigo, objetivo))
                {
                    BoardPiece statsEnemigo = ObtenerBoardPiece(enemigo);

                    if (statsEnemigo == null)
                        continue;

                    statsEnemigo.TakeDamage(0, 30);
                    TryRewardCardIfEnemyKilled(statsEnemigo, rolHeroe);
                }
            }

            Debug.Log("[HeroCardManager HOST] Bola de fuego ejecutada.");
        }
        else if (carta == HeroCardId.MagoAtaqueRayo)
        {
            FichaEnemigoAI enemigo = BuscarEnemigoMasCercano(fichaHeroe, 2);

            BoardPiece statsEnemigo = ObtenerBoardPiece(enemigo);

            statsEnemigo.TakeDamage(0, 100);
            TryRewardCardIfEnemyKilled(statsEnemigo, rolHeroe);

            Debug.Log("[HeroCardManager HOST] Ataque de rayo ejecutado.");
        }
        else if (carta == HeroCardId.MagoNevisca)
        {
            FichaEnemigoAI centro = BuscarEnemigoMasCercano(fichaHeroe, 2);

            BoardPiece statsCentro = ObtenerBoardPiece(centro);

            if (statsCentro != null)
                statsCentro.ApplyFrozen();

            FichaEnemigoAI[] enemigos =
                FindObjectsByType<FichaEnemigoAI>(FindObjectsSortMode.None);

            foreach (FichaEnemigoAI enemigo in enemigos)
            {
                if (enemigo == centro)
                    continue;

                if (EstaAdyacenteAEnemigo(enemigo, centro))
                {
                    BoardPiece statsEnemigo = ObtenerBoardPiece(enemigo);

                    if (statsEnemigo != null)
                        statsEnemigo.ApplyFrozen();
                }
            }

            Debug.Log("[HeroCardManager HOST] Nevisca ejecutada.");
        }
    }

    // =====================================================
    // BÚSQUEDAS DE OBJETIVOS
    // =====================================================

    private FichaEnemigoAI BuscarEnemigoMasCercano(FichaRPG fichaHeroe, int rango)
    {
        if (fichaHeroe == null || fichaHeroe.casillaActual == null)
            return null;

        FichaEnemigoAI[] enemigos =
            FindObjectsByType<FichaEnemigoAI>(FindObjectsSortMode.None);

        FichaEnemigoAI mejor = null;
        int mejorDistancia = int.MaxValue;

        foreach (FichaEnemigoAI enemigo in enemigos)
        {
            BoardPiece statsEnemigo = ObtenerBoardPiece(enemigo);

            if (statsEnemigo == null || statsEnemigo.CurrentHealth <= 0)
                continue;

            int distancia = DistanciaManhattan(
                fichaHeroe.casillaActual.coordenadaX,
                fichaHeroe.casillaActual.coordenadaZ,
                enemigo.coordenadaX,
                enemigo.coordenadaZ
            );

            if (distancia <= rango && distancia < mejorDistancia)
            {
                mejorDistancia = distancia;
                mejor = enemigo;
            }
        }

        return mejor;
    }

    private List<FichaEnemigoAI> BuscarEnemigosEnLineaRecta(FichaRPG fichaHeroe, int rango)
    {
        List<FichaEnemigoAI> resultado = new List<FichaEnemigoAI>();

        if (fichaHeroe == null || fichaHeroe.casillaActual == null)
            return resultado;

        int heroX = fichaHeroe.casillaActual.coordenadaX;
        int heroZ = fichaHeroe.casillaActual.coordenadaZ;

        FichaEnemigoAI[] enemigos =
            FindObjectsByType<FichaEnemigoAI>(FindObjectsSortMode.None);

        foreach (FichaEnemigoAI enemigo in enemigos)
        {
            BoardPiece statsEnemigo = ObtenerBoardPiece(enemigo);

            if (statsEnemigo == null || statsEnemigo.CurrentHealth <= 0)
                continue;

            int dx = enemigo.coordenadaX - heroX;
            int dz = enemigo.coordenadaZ - heroZ;

            bool mismaFila = dz == 0 && Mathf.Abs(dx) > 0 && Mathf.Abs(dx) <= rango;
            bool mismaColumna = dx == 0 && Mathf.Abs(dz) > 0 && Mathf.Abs(dz) <= rango;

            if (mismaFila || mismaColumna)
                resultado.Add(enemigo);
        }

        return resultado;
    }

    private bool EstaAdyacenteAEnemigo(FichaEnemigoAI a, FichaEnemigoAI b)
    {
        if (a == null || b == null)
            return false;

        int dx = Mathf.Abs(a.coordenadaX - b.coordenadaX);
        int dz = Mathf.Abs(a.coordenadaZ - b.coordenadaZ);

        return dx + dz == 1;
    }

    private int DistanciaManhattan(int x1, int z1, int x2, int z2)
    {
        return Mathf.Abs(x1 - x2) + Mathf.Abs(z1 - z2);
    }

    // =====================================================
    // MANO / INVENTARIO
    // =====================================================

    public int GetCardCountForRole(string rolHeroe)
    {
        return GetCardCount(rolHeroe);
    }

    public int GetCardAtForRole(string rolHeroe, int index)
    {
        return GetCardAt(rolHeroe, index);
    }

    private int GetCardCount(string rolHeroe)
    {
        if (rolHeroe == "Heroe 1")
            return Heroe1CardCount;

        if (rolHeroe == "Heroe 2")
            return Heroe2CardCount;

        return 0;
    }

    private void SetCardCount(string rolHeroe, int value)
    {
        if (rolHeroe == "Heroe 1")
            Heroe1CardCount = value;
        else if (rolHeroe == "Heroe 2")
            Heroe2CardCount = value;
    }

    private int GetCardAt(string rolHeroe, int index)
    {
        if (rolHeroe == "Heroe 1")
        {
            if (index == 0) return Heroe1Card0;
            if (index == 1) return Heroe1Card1;
            if (index == 2) return Heroe1Card2;
            if (index == 3) return Heroe1Card3;
        }

        if (rolHeroe == "Heroe 2")
        {
            if (index == 0) return Heroe2Card0;
            if (index == 1) return Heroe2Card1;
            if (index == 2) return Heroe2Card2;
            if (index == 3) return Heroe2Card3;
        }

        return 0;
    }

    private void SetCardAt(string rolHeroe, int index, int cardId)
    {
        if (rolHeroe == "Heroe 1")
        {
            if (index == 0) Heroe1Card0 = cardId;
            else if (index == 1) Heroe1Card1 = cardId;
            else if (index == 2) Heroe1Card2 = cardId;
            else if (index == 3) Heroe1Card3 = cardId;
        }
        else if (rolHeroe == "Heroe 2")
        {
            if (index == 0) Heroe2Card0 = cardId;
            else if (index == 1) Heroe2Card1 = cardId;
            else if (index == 2) Heroe2Card2 = cardId;
            else if (index == 3) Heroe2Card3 = cardId;
        }
    }

    private void RemoveCardAt(string rolHeroe, int index)
    {
        int cantidad = GetCardCount(rolHeroe);

        for (int i = index; i < cantidad - 1; i++)
        {
            int siguiente = GetCardAt(rolHeroe, i + 1);
            SetCardAt(rolHeroe, i, siguiente);
        }

        SetCardAt(rolHeroe, cantidad - 1, 0);
        SetCardCount(rolHeroe, cantidad - 1);
    }

    // =====================================================
    // AUXILIARES
    // =====================================================

    public string GetCardName(HeroCardId card)
    {
        if (card == HeroCardId.PaladinEmbestida) return "Embestida";
        if (card == HeroCardId.PaladinGolpeEscudo) return "Golpe de escudo";
        if (card == HeroCardId.PaladinJuramentoSagrado) return "Juramento sagrado";

        if (card == HeroCardId.MagoBolaFuego) return "Bola de fuego";
        if (card == HeroCardId.MagoAtaqueRayo) return "Ataque de rayo";
        if (card == HeroCardId.MagoNevisca) return "Nevisca";

        return "";
    }

    private bool EsRolHeroe(string rol)
    {
        return rol == "Heroe 1" || rol == "Heroe 2";
    }

    private FichaRPG BuscarHeroePorRol(string rol)
    {
        FichaRPG[] fichas =
            FindObjectsByType<FichaRPG>(FindObjectsSortMode.None);

        foreach (FichaRPG ficha in fichas)
        {
            if (!ficha.esHeroe)
                continue;

            if (ficha.RolPropietario == rol)
                return ficha;
        }

        return null;
    }

    private BoardPiece ObtenerBoardPiece(Component component)
    {
        if (component == null)
            return null;

        BoardPiece pieza = component.GetComponent<BoardPiece>();

        if (pieza == null)
            pieza = component.GetComponentInParent<BoardPiece>();

        if (pieza == null)
            pieza = component.GetComponentInChildren<BoardPiece>();

        return pieza;
    }

    private bool EsTurnoDePlayer(PlayerRef player)
    {
        if (GameplayManager.Instance == null || TurnManager.Instance == null)
            return false;

        if (GameplayManager.Instance.TurnOrder == null ||
            GameplayManager.Instance.TurnOrder.Count == 0)
            return false;

        int index = TurnManager.Instance.CurrentTurnIndex;

        if (index < 0 || index >= GameplayManager.Instance.TurnOrder.Count)
            index = 0;

        return GameplayManager.Instance.TurnOrder[index].PlayerRef == player;
    }

    private string ObtenerRolDePlayer(PlayerRef player)
    {
        if (GameplayManager.Instance != null &&
            GameplayManager.Instance.TurnOrder != null)
        {
            foreach (GameplayRoleCache.PlayerInfo info in GameplayManager.Instance.TurnOrder)
            {
                if (info.PlayerRef == player)
                    return info.PlayerRole;
            }
        }

        foreach (GameplayRoleCache.PlayerInfo info in GameplayRoleCache.Players)
        {
            if (info.PlayerRef == player)
                return info.PlayerRole;
        }

        return "Desconocido";
    }
}