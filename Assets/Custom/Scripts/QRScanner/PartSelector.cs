using UnityEngine;
using System.Collections;

public class PartSelector : MonoBehaviour
{
    [Header("Part Settings")]
    public string partName;
    public int partIndex;
    public Color highlightColor = Color.yellow; // Color configurable
    public float highlightDuration = 0.3f;

    private QRModelViewer viewer;
    private Renderer partRenderer;
    private Material originalMaterial;
    private Color originalColor;

    void Start()
    {
        viewer = FindObjectOfType<QRModelViewer>();
        partRenderer = GetComponent<Renderer>();

        if (partRenderer != null)
        {
            originalMaterial = partRenderer.material;
            originalColor = originalMaterial.color;
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
        if (partRenderer == null) yield break;

        // Guardar material original
        Material tempMaterial = new Material(originalMaterial);
        Color original = tempMaterial.color;

        // Aplicar color de resaltado
        tempMaterial.color = highlightColor;
        partRenderer.material = tempMaterial;

        // Esperar y restaurar
        yield return new WaitForSeconds(highlightDuration);

        if (partRenderer != null)
        {
            tempMaterial.color = original;
            partRenderer.material = tempMaterial;
        }
    }
}