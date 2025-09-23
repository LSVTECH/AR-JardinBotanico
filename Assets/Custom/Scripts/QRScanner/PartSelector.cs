// PartSelector.cs
using UnityEngine;
using System.Collections;

public class PartSelector : MonoBehaviour
{
    [Header("Part Settings")]
    public string partName;
    public int partIndex;
    public Color highlightColor = Color.yellow;
    public float highlightDuration = 0.3f;
    public string colorPropertyName = "_BaseColor";

    private QRModelViewer viewer;
    private Renderer partRenderer;
    private Material originalMaterial;
    private bool hasValidColorProperty = false;
    private Material tempMaterial;

    void Start()
    {
        viewer = FindObjectOfType<QRModelViewer>();
        partRenderer = GetComponent<Renderer>();

        if (partRenderer != null)
        {
            originalMaterial = partRenderer.material;
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

        tempMaterial = new Material(originalMaterial);
        partRenderer.material = tempMaterial;

        if (originalMaterial.HasProperty("_Color"))
            tempMaterial.color = highlightColor;
        else if (originalMaterial.HasProperty(colorPropertyName))
            tempMaterial.SetColor(colorPropertyName, highlightColor);

        yield return new WaitForSeconds(highlightDuration);

        if (partRenderer != null)
            partRenderer.material = originalMaterial;

        Destroy(tempMaterial);
    }

    void OnDestroy()
    {
        if (tempMaterial != null)
            Destroy(tempMaterial);
    }
}