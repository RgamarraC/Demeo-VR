using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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

    private string rolLocal = "";
    private bool listo = false;
    private bool juegoTerminado = false;

    private IEnumerator Start()
    {
        while (GameplayManager.Instance == null || HeroCardManager.Instance == null)
        {
            yield return null;
        }

        while (string.IsNullOrEmpty(GameplayManager.Instance.LocalPlayerRole) ||
               GameplayManager.Instance.LocalPlayerRole == "Sin rol")
        {
            yield return null;
        }

        rolLocal = GameplayManager.Instance.LocalPlayerRole;

        bool soyHeroe =
            rolLocal == "Heroe 1" ||
            rolLocal == "Heroe 2";

        if (botonVerCartas != null)
            botonVerCartas.gameObject.SetActive(soyHeroe);

        if (menuCartas != null)
            menuCartas.SetActive(false);

        if (!soyHeroe)
        {
            listo = false;
            yield break;
        }

        ConfigurarBotones();

        listo = true;

        ActualizarCartas();

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

        if (botonCerrarCartas != null)
        {
            botonCerrarCartas.onClick.RemoveAllListeners();
            botonCerrarCartas.onClick.AddListener(CerrarMenuCartas);
        }

        for (int i = 0; i < botonesCarta.Length; i++)
        {
            int index = i;

            if (botonesCarta[i] != null)
            {
                botonesCarta[i].onClick.RemoveAllListeners();
                botonesCarta[i].onClick.AddListener(() => OnCardPressed(index));
            }
        }
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

        int cantidad =
            HeroCardManager.Instance.GetCardCountForRole(rolLocal);

        if (textoSinCartas != null)
            textoSinCartas.gameObject.SetActive(cantidad == 0);

        for (int i = 0; i < botonesCarta.Length; i++)
        {
            bool tieneCarta = i < cantidad;

            if (botonesCarta[i] != null)
            {
                botonesCarta[i].gameObject.SetActive(tieneCarta);
                botonesCarta[i].interactable = tieneCarta && !juegoTerminado;
            }

            if (imagenesCarta[i] != null)
            {
                imagenesCarta[i].gameObject.SetActive(tieneCarta);

                if (tieneCarta)
                {
                    int cardId =
                        HeroCardManager.Instance.GetCardAtForRole(rolLocal, i);

                    HeroCardId carta = (HeroCardId)cardId;

                    imagenesCarta[i].sprite = ObtenerSpriteCarta(carta);
                    imagenesCarta[i].preserveAspect = true;
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

        for (int i = 0; i < botonesCarta.Length; i++)
        {
            if (botonesCarta[i] != null)
                botonesCarta[i].interactable = false;
        }

        Debug.Log("[HeroCardUI] Cartas bloqueadas por fin de juego.");
    }
}