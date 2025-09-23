// ARSessionCleaner.cs
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
            trackedImageManager.enabled = false;
            foreach (var trackedImage in trackedImageManager.trackables)
                Destroy(trackedImage.gameObject);
        }

        if (arSession != null)
            arSession.Reset();

        if (trackedImageManager != null)
            trackedImageManager.enabled = true;
    }
}