// ModelInteractor.cs
using UnityEngine;

public class ModelInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float rotationSpeed = 0.5f;
    public float scaleSpeed = 0.01f;
    public float minScale = 0.5f;
    public float maxScale = 2.0f;

    private Vector2 lastTouchPosition;
    private float initialDistance;
    private bool isInteracting = false;
    private QRModelViewer viewer;
    private Transform modelTransform;
    private Quaternion startRotation;
    private Vector3 startScale;
    private string qrCode;

    public void Initialize(QRModelViewer qrViewer, string qrCode)
    {
        viewer = qrViewer;
        this.qrCode = qrCode;
        modelTransform = transform;
        startRotation = modelTransform.localRotation;
        startScale = modelTransform.localScale;
    }

    void Update()
    {
        if (!isInteracting) return;

        if (Input.touchCount == 1)
            HandleRotation();
        else if (Input.touchCount == 2)
            HandleScaling();
    }

    void HandleRotation()
    {
        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                lastTouchPosition = touch.position;
                viewer.StartManipulation(qrCode);
                break;
            case TouchPhase.Moved:
                Vector2 delta = touch.position - lastTouchPosition;
                modelTransform.Rotate(modelTransform.up, -delta.x * rotationSpeed, Space.World);
                modelTransform.Rotate(modelTransform.right, delta.y * rotationSpeed, Space.World);
                lastTouchPosition = touch.position;
                break;
            case TouchPhase.Ended:
                viewer.StopManipulation(qrCode, modelTransform.localRotation, modelTransform.localScale);
                break;
        }
    }

    void HandleScaling()
    {
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
        {
            initialDistance = Vector2.Distance(touch1.position, touch2.position);
            viewer.StartManipulation(qrCode);
        }
        else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
        {
            float currentDistance = Vector2.Distance(touch1.position, touch2.position);
            float scaleFactor = (currentDistance - initialDistance) * scaleSpeed;

            Vector3 newScale = modelTransform.localScale * (1 + scaleFactor);
            newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
            newScale.y = Mathf.Clamp(newScale.y, minScale, maxScale);
            newScale.z = Mathf.Clamp(newScale.z, minScale, maxScale);

            modelTransform.localScale = newScale;
            initialDistance = currentDistance;
        }
        else if (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended)
        {
            viewer.StopManipulation(qrCode, modelTransform.localRotation, modelTransform.localScale);
        }
    }

    public void StartInteraction() => isInteracting = true;
    public void StopInteraction() => isInteracting = false;
}