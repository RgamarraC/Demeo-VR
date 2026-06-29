using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

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
    public bool estaEnNiebla;

    [Header("Feedback Visual")]
    [SerializeField] private MeshRenderer quadRenderer;

    public void SetearEstadoVisual(string estado)
    {
        if (quadRenderer == null) return;

        // Obtenemos el material instanciado de la baldosa
        Material mat = quadRenderer.material;

        switch (estado)
        {
            case "Visible":
            case "Apagado":
                // Escondemos el quad para revelar el mapa artístico limpio
                quadRenderer.gameObject.SetActive(false);
                break;

            case "Niebla":
                quadRenderer.gameObject.SetActive(true);
                // 1. Teñimos de negro semi-transparente (el 0.7f es la opacidad)
                mat.SetColor("_BaseColor", new Color(0.05f, 0.05f, 0.05f, 0.75f));
                
                // 2. Apagamos la emisión por completo para que sea oscuridad real
                mat.DisableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.black);
                break;

            case "EnRango":
                quadRenderer.gameObject.SetActive(true);
                // Amarillo base
                mat.SetColor("_BaseColor", Color.yellow);
                
                // Forzamos el mapa de emisión a brillar en amarillo
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.yellow * 1.5f); // El multiplicador le da intensidad HDR
                break;

            case "Hover":
                quadRenderer.gameObject.SetActive(true);
                // Azul base para cuando pasas la ficha encima
                mat.SetColor("_BaseColor", Color.blue);
                
                // Emisión en azul
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.blue * 1.5f);
                break;

            case "Spawn":
                quadRenderer.gameObject.SetActive(true);
                // Verde base para los puntos de spawn de los héroes
                mat.SetColor("_BaseColor", Color.green);
                
                // Emisión en verde
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.green * 1.5f);
                break;
                
            case "DM_Target":
                quadRenderer.gameObject.SetActive(true);
                // Rojo o púrpura para indicar objetivo de invocación
                mat.SetColor("_BaseColor", new Color(0.8f, 0f, 0f, 0.8f));
                
                // Emisión en rojo brillante
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.red * 2f);
                break;
        }
    }

    public Vector3 ObtenerCentro()
    {        
        return transform.position;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        GridManager grid = Object.FindFirstObjectByType<GridManager>();
        bool debugActivo = grid != null ? grid.modoDebugColorCasillas : false;
        ActualizarColorDebug(debugActivo);
    }

    public void ActualizarColorDebug(bool debugActivo)
    {
        // Esto cambia el color del SUELO BASE (no el Quad) en el Editor cuando modificas el bitmask
        MeshRenderer rendererBase = GetComponent<MeshRenderer>();
        if (rendererBase != null)
        {
            MaterialPropertyBlock prop = new MaterialPropertyBlock();
            rendererBase.GetPropertyBlock(prop);

            Color colorBitmask = Color.white; // 0 Muros (Libre) o color base

            if (debugActivo)
            {
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
            }

            prop.SetColor("_Color", colorBitmask);
            prop.SetColor("_BaseColor", colorBitmask);
            rendererBase.SetPropertyBlock(prop);
        }
    }
#endif
}
