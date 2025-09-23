// CameraDisabler.cs
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class CameraDisabler : MonoBehaviour
{
    public ARCameraManager cameraManager;

    public void ToggleCamera(bool enable)
    {
        if (cameraManager != null)
            cameraManager.enabled = enable;
    }
}