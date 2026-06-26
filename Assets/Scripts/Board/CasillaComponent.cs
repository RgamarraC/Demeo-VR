using UnityEngine;

public class CasillaComponent : MonoBehaviour
{
    [Header("Coordenadas")]
    [SerializeField] private Vector2Int coordenadaGrid;
    
    [Header("Estado")]
    [SerializeField] private bool estaOcupada;
    
    [Header("Configuración de Spawn")]
    [SerializeField] private bool esSpawnHeroe;

    [Header("Feedback Visual")]
    [SerializeField] private GameObject efectoLuzSeleccion;

    private MeshRenderer meshRenderer;

    // Propiedades públicas
    public Vector2Int CoordenadaGrid { get => coordenadaGrid; set => coordenadaGrid = value; }
    public bool EstaOcupada { get => estaOcupada; set => estaOcupada = value; }
    public bool EsSpawnHeroe => esSpawnHeroe;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void CambiarColor(Color nuevoColor)
    {
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(propBlock);
            
            propBlock.SetColor("_Color", nuevoColor); 
            propBlock.SetColor("_BaseColor", nuevoColor);
            
            meshRenderer.SetPropertyBlock(propBlock);
        }

        if (efectoLuzSeleccion != null)
        {
            efectoLuzSeleccion.SetActive(true);
        }
    }

    /// <summary>
    /// Restablece el color al estado base del material (gris original).
    /// Al borrar el bloque de propiedades (null), el MeshRenderer vuelve automáticamente
    /// al color del material guardado en su memoria original.
    /// </summary>
    public void RestablecerColorOriginal()
    {
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.SetPropertyBlock(null);
        }

        if (efectoLuzSeleccion != null)
        {
            efectoLuzSeleccion.SetActive(false);
        }
    }

    public Vector3 ObtenerCentro()
    {
        return transform.position;
    }
}
