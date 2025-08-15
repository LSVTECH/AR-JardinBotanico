using UnityEngine;
using System.Collections;

public class PartSelector : MonoBehaviour
{
    [Header("Part Settings")]
    public string partName;
    public int partIndex;
    public Color highlightColor = Color.yellow;
    public float highlightDuration = 0.3f;
    public string colorPropertyName = "_BaseColor"; // Nombre alternativo común en URP

    private QRModelViewer viewer;
    private Renderer partRenderer;
    private Material originalMaterial;
    private bool hasValidColorProperty = false;

    void Start()
    {
        viewer = FindObjectOfType<QRModelViewer>();
        partRenderer = GetComponent<Renderer>();

        if (partRenderer != null)
        {
            originalMaterial = partRenderer.material;

            // Verificar propiedades de color disponibles
            hasValidColorProperty = originalMaterial.HasProperty("_Color") ||
                                   originalMaterial.HasProperty(colorPropertyName);
        }
    }

    void OnMouseDown()
    {
        if (viewer != null)
        {
            viewer.ShowPartInfo(partIndex);
            StartCoroutine(HighlightPart());
        }
    }

    IEnumerator HighlightPart()
    {
        if (partRenderer == null || !hasValidColorProperty) yield break;

        // Crear material temporal para el resaltado
        Material tempMaterial = new Material(originalMaterial);
        partRenderer.material = tempMaterial;

        // Aplicar resaltado usando propiedad correcta
        if (originalMaterial.HasProperty("_Color"))
        {
            tempMaterial.color = highlightColor;
        }
        else if (originalMaterial.HasProperty(colorPropertyName))
        {
            tempMaterial.SetColor(colorPropertyName, highlightColor);
        }

        yield return new WaitForSeconds(highlightDuration);

        // Restaurar material original
        if (partRenderer != null)
        {
            partRenderer.material = originalMaterial;
        }

        // Destruir material temporal
        Destroy(tempMaterial);
    }
}