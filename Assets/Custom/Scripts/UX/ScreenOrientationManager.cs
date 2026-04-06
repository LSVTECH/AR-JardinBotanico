using UnityEngine;
using UnityEngine.UI;

public class ScreenOrientationManager : MonoBehaviour
{
    [Header("Orientación Inicial")]
    public bool startInLandscape = false;

    void Start()
    {
        if (startInLandscape)
            SetLandscapeOrientation();
        else
            SetPortraitOrientation();
    }

    /// <summary>
    /// Fuerza el dispositivo a modo Horizontal (Para juegos como el Mono)
    /// </summary>
    public void SetLandscapeOrientation()
    {
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    /// <summary>
    /// Fuerza el dispositivo a modo Vertical (Para el menú principal y cámara AR)
    /// </summary>
    public void SetPortraitOrientation()
    {
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        
        Screen.orientation = ScreenOrientation.Portrait;
    }
}
