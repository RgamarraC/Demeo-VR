using UnityEngine;

/// <summary>
/// Script para añadir visión de "Rayos X" a un objeto.
/// Esto dibuja un borde del color elegido SOLO cuando el objeto cae detrás de otro (ej. una pared).
/// </summary>
public class XRayVision : MonoBehaviour
{
    [Header("Configuración de Rayos X")]
    
    [Tooltip("¡ARRASTRA EL SHADER AQUÍ! Ve a tu carpeta Assets/Shaders y arrastra XRaySilhouette.shader a esta casilla. Es obligatorio para que funcione en el APK.")]
    public Shader xrayShaderReference;

    [Tooltip("El color que tendrá el objeto cuando esté oculto detrás de una pared.")]
    public Color xrayColor = new Color(0f, 1f, 1f, 0.8f); // Cian por defecto
    
    [Tooltip("El grosor del borde (0.005 es un buen valor inicial)")]
    [Range(0f, 0.05f)]
    public float outlineThickness = 0.005f;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // Verificamos si pusiste el shader en el inspector
            if (xrayShaderReference != null)
            {
                // Creamos un nuevo material usando nuestra referencia directa
                Material xrayMat = new Material(xrayShaderReference);
                xrayMat.SetColor("_XRayColor", xrayColor);
                xrayMat.SetFloat("_Thickness", outlineThickness);

                // Añadimos este material como un SEGUNDO material al Renderer.
                Material[] currentMaterials = rend.materials;
                Material[] newMaterials = new Material[currentMaterials.Length + 1];
                
                for (int i = 0; i < currentMaterials.Length; i++)
                {
                    newMaterials[i] = currentMaterials[i];
                }
                
                newMaterials[newMaterials.Length - 1] = xrayMat;
                rend.materials = newMaterials;
            }
            else
            {
                Debug.LogError("XRayVision: Te falta asignar el Shader en el Inspector. ¡Arrastra el archivo XRaySilhouette.shader a la casilla correspondiente!");
            }
        }
    }
}
