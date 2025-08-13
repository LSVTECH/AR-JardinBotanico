using UnityEngine;

public class FloatingObj : MonoBehaviour
{
    public float floatSpeed = 0.5f;
    public float floatHeight = 0.1f;
    private bool enableRotation = true;
    private float rotationSpeed = 20f;
    private Vector3 rotationAxis = Vector3.up;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private float randomOffset;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        randomOffset = Random.Range(0f, 2f * Mathf.PI);
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
