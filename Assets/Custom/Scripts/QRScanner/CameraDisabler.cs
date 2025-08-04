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

            // Apagar completamente la c�mara en dispositivos m�viles
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