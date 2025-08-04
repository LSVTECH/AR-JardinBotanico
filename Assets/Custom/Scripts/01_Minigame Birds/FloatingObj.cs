using UnityEngine;

public class FloatingObj : MonoBehaviour
{
    private float floatSpeed;
    private float floatHeight;
    private bool enableRotation;
    private float rotationSpeed;
    private Vector3 rotationAxis;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private float randomOffset;

    void Start()
    {
        // Obtener valores del GameManager como respaldo
        GameManager gm = FindObjectOfType<GameManager>();
        floatSpeed = gm.defaultFloatSpeed;
        floatHeight = gm.defaultFloatHeight;
        enableRotation = true;
        rotationSpeed = 20f;
        rotationAxis = Vector3.up;

        // Intentar obtener configuración personalizada
        ObjFloatSettings settings = GetComponent<ObjFloatSettings>();
        if (settings != null)
        {
            if (settings.overrideFloat) floatSpeed = settings.floatSpeed;
            if (settings.overrideHeight) floatHeight = settings.floatHeight;
            if (settings.overrideRotation) rotationSpeed = settings.rotationSpeed;
            if (settings.overrideAxis) rotationAxis = settings.rotationAxis;
            enableRotation = settings.enableRotation;
        }

        startPosition = transform.position;
        startRotation = transform.rotation;
        randomOffset = Random.Range(0f, 2f * Mathf.PI);

       // Debug.Log($"Config: Speed={floatSpeed} Height={floatHeight} " +
            // $"Rotation={rotationSpeed} Axis={rotationAxis}");
    }

    void Update()
    {
        // Movimiento vertical
        if (floatHeight > 0.01f)
        {
            float newY = startPosition.y + Mathf.Sin((Time.time + randomOffset) * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        // Rotación
        if (enableRotation && rotationSpeed > 0.01f)
        {
            transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
        }
    }
}
