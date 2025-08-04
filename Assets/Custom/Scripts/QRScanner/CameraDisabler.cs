using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class CameraDisabler : MonoBehaviour
{
    public ARCameraManager cameraManager;

    public void ToggleCamera(bool enable)
    {
        if (cameraManager != null)
        {
            cameraManager.enabled = enable;

            // Apagar completamente la cámara en dispositivos móviles
            if (Application.isMobilePlatform)
            {
                WebCamTexture webcamTexture = new WebCamTexture();
                if (enable)
                {
                    webcamTexture.Play();
                }
                else
                {
                    webcamTexture.Stop();
                }
            }
        }
    }
}