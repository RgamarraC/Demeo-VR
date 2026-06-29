using UnityEngine;
using TMPro;
using DemeoVR.Gameplay;

public class PieceMiniUI : MonoBehaviour
{
    [Header("Referencias")]
    public BoardPiece boardPiece;
    public TMP_Text textoStats;

    [Header("Configuración")]
    public bool esHeroe = false;
    public bool mirarACamara = true;

    private Camera camaraPrincipal;

    private void Start()
    {
        if (boardPiece == null)
            boardPiece = GetComponentInParent<BoardPiece>();

        camaraPrincipal = Camera.main;
    }

    private void Update()
    {
        if (boardPiece == null || textoStats == null)
            return;

        if (boardPiece.CurrentHealth <= 0)
        {
            textoStats.gameObject.SetActive(false);
            return;
        }

        textoStats.gameObject.SetActive(true);

        if (esHeroe)
        {
            textoStats.text =
                "HP: " + boardPiece.CurrentHealth + "\n" +
                "AP: " + boardPiece.CurrentAP;
        }
        else
        {
            textoStats.text =
                "HP: " + boardPiece.CurrentHealth;
        }

        if (mirarACamara && camaraPrincipal != null)
        {
            transform.LookAt(transform.position + camaraPrincipal.transform.rotation * Vector3.forward,
                             camaraPrincipal.transform.rotation * Vector3.up);
        }
    }
}