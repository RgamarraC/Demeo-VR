using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class HeroCardUI : MonoBehaviour
{
    [Header("Botones principales")]
    public Button botonVerCartas;
    public Button botonCerrarCartas;

    [Header("Menú de cartas")]
    public GameObject menuCartas;
    public TMP_Text textoSinCartas;

    [Header("Botones de cartas")]
    public Button[] botonesCarta = new Button[4];

    [Header("Imágenes de cartas")]
    public Image[] imagenesCarta = new Image[4];

    [Header("Sprites Paladín")]
    public Sprite spritePaladinEmbestida;
    public Sprite spritePaladinGolpeEscudo;
    public Sprite spritePaladinJuramentoSagrado;

    [Header("Sprites Mago")]
    public Sprite spriteMagoBolaFuego;
    public Sprite spriteMagoAtaqueRayo;
    public Sprite spriteMagoNevisca;

    [Header("Estado Local (Debug)")]
    [SerializeField] private string rolLocal = "";
    [SerializeField] private bool listo = false;
    [SerializeField] private bool juegoTerminado = false;

    private void Start()
    {
        StartCoroutine(IniciarUI());
    }

    private IEnumerator IniciarUI()
    {
        string rol = GameplayRoleCache.LocalRole;

        if (string.IsNullOrEmpty(rol) || rol == "Sin rol")
        {
            if (GameplayManager.Instance != null && !string.IsNullOrEmpty(GameplayManager.Instance.LocalPlayerRole) && GameplayManager.Instance.LocalPlayerRole != "Sin rol")
            {
                rol = GameplayManager.Instance.LocalPlayerRole;
            }
            else
            {
                rol = "Heroe 1";
            }
        }

        rolLocal = rol.Trim();

        bool soyHeroe = rolLocal == "Heroe 1" || rolLocal == "Heroe 2";

        if (menuCartas != null)
            menuCartas.SetActive(false);

        if (!soyHeroe)
        {
            listo = false;
            Debug.Log("[HeroCardUI] El rol local '" + rolLocal + "' no es un héroe. La UI de cartas no se activará para este jugador.");
            yield break;
        }

        while (HeroCardManager.Instance == null)
        {
            HeroCardManager.Instance = FindFirstObjectByType<HeroCardManager>();
            if (HeroCardManager.Instance != null)
                break;

            yield return null;
        }

        try
        {
            ConfigurarBotones();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[HeroCardUI] Error al configurar botones: " + ex.Message);
        }

        listo = true;

        try
        {
            ActualizarCartas();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[HeroCardUI] Error al actualizar cartas: " + ex.Message);
        }

        Debug.Log("[HeroCardUI] UI de cartas iniciada. Rol local = " + rolLocal);
    }

    private void Update()
    {
        if (!listo)
            return;

        ActualizarCartas();
    }

    private void ConfigurarBotones()
    {
        if (botonVerCartas != null)
        {
            botonVerCartas.onClick.RemoveAllListeners();
            botonVerCartas.onClick.AddListener(AbrirMenuCartas);
        }
        else
        {
            Debug.LogWarning("[HeroCardUI] 'botonVerCartas' no está asignado en el Inspector.");
        }

        if (botonCerrarCartas != null)
        {
            botonCerrarCartas.onClick.RemoveAllListeners();
            botonCerrarCartas.onClick.AddListener(CerrarMenuCartas);
        }
        else
        {
            Debug.LogWarning("[HeroCardUI] 'botonCerrarCartas' no está asignado en el Inspector.");
        }

        if (botonesCarta != null)
        {
            for (int i = 0; i < botonesCarta.Length; i++)
            {
                int index = i;

                if (botonesCarta[i] != null)
                {
                    botonesCarta[i].onClick.RemoveAllListeners();
                    botonesCarta[i].onClick.AddListener(() => OnCardPressed(index));
                }
                else
                {
                    Debug.LogWarning("[HeroCardUI] botonesCarta[" + i + "] no está asignado en el Inspector.");
                }
            }
        }
        else
        {
            Debug.LogWarning("[HeroCardUI] El arreglo 'botonesCarta' no está asignado en el Inspector.");
        }

        Debug.Log("[HeroCardUI] Botones de cartas configurados.");
    }

    public void AbrirMenuCartas()
    {
        if (juegoTerminado)
            return;

        if (menuCartas != null)
            menuCartas.SetActive(true);

        ActualizarCartas();

        Debug.Log("[HeroCardUI] Menú de cartas abierto.");
    }

    public void CerrarMenuCartas()
    {
        if (menuCartas != null)
            menuCartas.SetActive(false);

        Debug.Log("[HeroCardUI] Menú de cartas cerrado.");
    }

    private void ActualizarCartas()
    {
        if (HeroCardManager.Instance == null)
            return;

        int cantidad = HeroCardManager.Instance.GetCardCountForRole(rolLocal);

        if (textoSinCartas != null)
            textoSinCartas.gameObject.SetActive(cantidad == 0);

        int maxBotones = botonesCarta != null ? botonesCarta.Length : 0;
        int maxImagenes = imagenesCarta != null ? imagenesCarta.Length : 0;
        int total = Mathf.Max(maxBotones, maxImagenes);

        for (int i = 0; i < total; i++)
        {
            bool tieneCarta = i < cantidad;

            if (botonesCarta != null && i < maxBotones && botonesCarta[i] != null)
            {
                botonesCarta[i].gameObject.SetActive(tieneCarta);
                botonesCarta[i].interactable = tieneCarta && !juegoTerminado;
            }

            if (imagenesCarta != null && i < maxImagenes && imagenesCarta[i] != null)
            {
                imagenesCarta[i].gameObject.SetActive(tieneCarta);

                if (tieneCarta)
                {
                    int cardId = HeroCardManager.Instance.GetCardAtForRole(rolLocal, i);
                    HeroCardId carta = (HeroCardId)cardId;

                    Sprite spriteCarta = ObtenerSpriteCarta(carta);
                    imagenesCarta[i].sprite = spriteCarta;
                    imagenesCarta[i].preserveAspect = true;

                    if (spriteCarta == null)
                    {
                        Debug.LogWarning("[HeroCardUI] No se encontró Sprite para la carta: " + carta + " (ID: " + cardId + "). Revisa las referencias en el Inspector.");
                    }
                }
            }
        }
    }

    private Sprite ObtenerSpriteCarta(HeroCardId carta)
    {
        if (carta == HeroCardId.PaladinEmbestida)
            return spritePaladinEmbestida;

        if (carta == HeroCardId.PaladinGolpeEscudo)
            return spritePaladinGolpeEscudo;

        if (carta == HeroCardId.PaladinJuramentoSagrado)
            return spritePaladinJuramentoSagrado;

        if (carta == HeroCardId.MagoBolaFuego)
            return spriteMagoBolaFuego;

        if (carta == HeroCardId.MagoAtaqueRayo)
            return spriteMagoAtaqueRayo;

        if (carta == HeroCardId.MagoNevisca)
            return spriteMagoNevisca;

        return null;
    }

    private void OnCardPressed(int index)
    {
        if (juegoTerminado)
        {
            Debug.Log("[HeroCardUI] No puedes usar cartas. El juego terminó.");
            return;
        }

        if (HeroCardManager.Instance == null)
            return;

        Debug.Log("[HeroCardUI] Carta presionada. Index = " + index);

        HeroCardManager.Instance.RequestUseCard(index);

        ActualizarCartas();
    }

    public void BloquearCartasFinJuego()
    {
        juegoTerminado = true;

        if (botonVerCartas != null)
            botonVerCartas.interactable = false;

        if (botonCerrarCartas != null)
            botonCerrarCartas.interactable = false;

        if (botonesCarta != null)
        {
            for (int i = 0; i < botonesCarta.Length; i++)
            {
                if (botonesCarta[i] != null)
                    botonesCarta[i].interactable = false;
            }
        }

        Debug.Log("[HeroCardUI] Cartas bloqueadas por fin de juego.");
    }
}