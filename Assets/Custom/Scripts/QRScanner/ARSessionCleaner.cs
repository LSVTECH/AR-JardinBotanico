using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARSessionCleaner : MonoBehaviour
{
    public ARSession arSession;
    public ARTrackedImageManager trackedImageManager;

    public void ResetARSession()
    {
        if (trackedImageManager != null)
        {
            // Desactivar antes de resetear
            trackedImageManager.enabled = false;

            // Destruir todos los trackables existentes
            foreach (var trackedImage in trackedImageManager.trackables)
            {
                Destroy(trackedImage.gameObject);
            }
        }

        if (arSession != null)
        {
            arSession.Reset();
        }

        if (trackedImageManager != null)
        {
            // Reactivar después de resetear
            trackedImageManager.enabled = true;
        }
    }
}