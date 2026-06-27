using UnityEngine;
using System.Collections.Generic;

public class CasillaComponent : MonoBehaviour
{
    [Header("Configuración Manual")]
    public int coordenadaX;
    public int coordenadaZ;
    public int valorBitmask;

    [Header("Estado")]
    public bool estaOcupada;
    public bool esObstaculo;
    public bool esSpawnHeroe;

    [Header("Feedback Visual")]
    [SerializeField] private MeshRenderer quadRenderer;

    public void SetearEstadoVisual(string estado)
    {
        if (quadRenderer == null) return;

        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        quadRenderer.GetPropertyBlock(propBlock);

        switch (estado)
        {
            case "Apagado":
                if (esSpawnHeroe)
                {
                    quadRenderer.gameObject.SetActive(true);
                    propBlock.SetColor("_Color", Color.green);
                    propBlock.SetColor("_BaseColor", Color.green);
                }
                else
                {
                    quadRenderer.gameObject.SetActive(false);
                }
                break;
            case "EnRango":
                quadRenderer.gameObject.SetActive(true);
                propBlock.SetColor("_Color", Color.yellow);
                propBlock.SetColor("_BaseColor", Color.yellow);
                break;
            case "Hover":
                quadRenderer.gameObject.SetActive(true);
                propBlock.SetColor("_Color", Color.blue);
                propBlock.SetColor("_BaseColor", Color.blue);
                break;
        }

        quadRenderer.SetPropertyBlock(propBlock);
    }

    public Vector3 ObtenerCentro()
    {
        return transform.position;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Esto cambia el color del SUELO BASE (no el Quad) en el Editor cuando modificas el bitmask
        MeshRenderer rendererBase = GetComponent<MeshRenderer>();
        if (rendererBase != null)
        {
            MaterialPropertyBlock prop = new MaterialPropertyBlock();
            rendererBase.GetPropertyBlock(prop);

            Color colorBitmask = Color.white; // 0 Muros (Libre)

            if ((valorBitmask & 16) != 0) // Es una Puerta (+16)
            {
                colorBitmask = new Color(0.6f, 0.3f, 0.1f); // Marrón
            }
            else
            {
                int numParedes = 0;
                if ((valorBitmask & 1) != 0) numParedes++; // Norte
                if ((valorBitmask & 2) != 0) numParedes++; // Este
                if ((valorBitmask & 4) != 0) numParedes++; // Sur
                if ((valorBitmask & 8) != 0) numParedes++; // Oeste

                switch (numParedes)
                {
                    case 0: colorBitmask = Color.white; break;
                    case 1: colorBitmask = Color.cyan; break;       // 1 Muro
                    case 2: colorBitmask = new Color(1f, 0.5f, 0f); break; // Naranja (Pasillos/Esquinas)
                    case 3: colorBitmask = Color.red; break;        // Callejón sin salida
                    case 4: colorBitmask = Color.black; break;      // Encerrado total (15)
                }
            }

            prop.SetColor("_Color", colorBitmask);
            prop.SetColor("_BaseColor", colorBitmask);
            rendererBase.SetPropertyBlock(prop);
        }
    }
#endif
}
