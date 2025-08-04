using UnityEngine;

public class ObjFloatSettings : MonoBehaviour
{
    [Header("Float Settings")]
    public bool overrideFloat;
    [ConditionalHide("overrideFloat", true)]
    public float floatSpeed = 0.5f;

    public bool overrideHeight;
    [ConditionalHide("overrideHeight", true)]
    public float floatHeight = 0.1f;

    [Header("Rotation Settings")]
    public bool enableRotation = true;

    public bool overrideRotation;
    [ConditionalHide("overrideRotation", true)]
    public float rotationSpeed = 20f;

    public bool overrideAxis;
    [ConditionalHide("overrideAxis", true)]
    public Vector3 rotationAxis = Vector3.up;
}

// Clase auxiliar para mostrar/ocultar propiedades en el inspector
public class ConditionalHideAttribute : PropertyAttribute
{
    public string conditionalSourceField;
    public bool hideInInspector;

    public ConditionalHideAttribute(string conditionalSourceField, bool hideInInspector = false)
    {
        this.conditionalSourceField = conditionalSourceField;
        this.hideInInspector = hideInInspector;
    }
}
