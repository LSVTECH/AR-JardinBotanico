using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;

public class ARSystemManager : MonoBehaviour
{
    private ARCameraManager cameraManager;

    private void OnEnable()
    {
        EventManager.OnARSystemInitialized += EnsureARSetup;
    }

    private void OnDisable()
    {
        EventManager.OnARSystemInitialized -= EnsureARSetup;
    }

    private void EnsureARSetup()
    {
        XROrigin sessionOrigin = Object.FindFirstObjectByType<XROrigin>();
        if (sessionOrigin == null)
        {
            Debug.LogError("No XROrigin found. Creating one...");
            InitializeAR();
            return;
        }

        if (sessionOrigin.Camera == null)
        {
            Debug.LogError("XR Origin camera is null. Reconfiguring...");
            Camera arCamera = sessionOrigin.GetComponentInChildren<Camera>();
            if (arCamera != null)
            {
                sessionOrigin.Camera = arCamera;
                Debug.Log("AR Camera reconfigured successfully");
            }
            else
            {
                Debug.LogError("No camera found in XR Origin. Recreating AR setup...");
                InitializeAR();
            }
        }

        if (Object.FindFirstObjectByType<ARSession>() == null)
        {
            Debug.LogWarning("No ARSession found. Creating one...");
            GameObject arSession = new GameObject("AR Session");
            arSession.AddComponent<ARSession>();
        }

        if (cameraManager == null)
        {
            cameraManager = sessionOrigin.GetComponentInChildren<ARCameraManager>();
            if (cameraManager == null)
            {
                Debug.LogError("No ARCameraManager found. Recreating AR setup...");
                InitializeAR();
            }
        }
    }

    private void InitializeAR()
    {
        if (cameraManager == null)
        {
            cameraManager = Object.FindFirstObjectByType<ARCameraManager>();
        }

        if (cameraManager == null)
        {
            Debug.LogWarning("ARCameraManager not found. Creating one...");

            // Crear XR Origin
            GameObject arSessionOrigin = new GameObject("XR Origin");
            XROrigin sessionOrigin = arSessionOrigin.AddComponent<XROrigin>();

            // Crear AR Camera
            GameObject arCamera = new GameObject("AR Camera");
            arCamera.transform.SetParent(arSessionOrigin.transform);

            // Configurar la cámara
            Camera camera = arCamera.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 20f;

            // Agregar ARCameraManager
            cameraManager = arCamera.AddComponent<ARCameraManager>();

            // Configurar la cámara en el XROrigin
            sessionOrigin.Camera = camera;

            // Agregar ARSession si no existe
            if (Object.FindFirstObjectByType<ARSession>() == null)
            {
                GameObject arSession = new GameObject("AR Session");
                arSession.AddComponent<ARSession>();
            }

            Debug.Log("AR Camera setup completed successfully");
        }
    }
}
