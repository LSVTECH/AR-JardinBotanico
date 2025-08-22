using UnityEngine;
using UnityEngine.UI;

public class ScreenOrientationManager : MonoBehaviour
{
    [Header("Orientación Inicial")]
    public bool startInLandscape = false;

    [Header("Configuración de Rotación Automática")]
    public bool allowAutoRotation = true;
    public bool allowPortrait = true;
    public bool allowPortraitUpsideDown = false;
    public bool allowLandscapeLeft = true;
    public bool allowLandscapeRight = true;

    void Start()
    {
        if (startInLandscape)
        {
            SetLandscapeOrientation();
        }
        else
        {
            SetPortraitOrientation();
        }
    }

    // Función para forzar modo horizontal
    public void SetLandscapeOrientation()
    {
        // Desactivar rotación automática
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        // Forzar orientación landscape
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // Asegurar que la UI se ajuste
        UpdateLayoutForLandscape();

        Debug.Log("Modo horizontal activado");
    }

    // Función para revertir a modo portrait o rotación automática
    public void SetPortraitOrientation()
    {
        if (allowAutoRotation)
        {
            // Configurar rotaciones permitidas
            Screen.autorotateToPortrait = allowPortrait;
            Screen.autorotateToPortraitUpsideDown = allowPortraitUpsideDown;
            Screen.autorotateToLandscapeLeft = allowLandscapeLeft;
            Screen.autorotateToLandscapeRight = allowLandscapeRight;

            // Habilitar rotación automática
            Screen.orientation = ScreenOrientation.AutoRotation;
        }
        else
        {
            // Forzar portrait
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;

            Screen.orientation = ScreenOrientation.Portrait;
        }

        // Asegurar que la UI se ajuste
        UpdateLayoutForPortrait();

        Debug.Log("Modo portrait/auto activado");
    }

    // Función para alternar entre orientaciones
    public void ToggleOrientation()
    {
        if (Screen.orientation == ScreenOrientation.Portrait ||
            Screen.orientation == ScreenOrientation.PortraitUpsideDown ||
            Screen.orientation == ScreenOrientation.AutoRotation)
        {
            SetLandscapeOrientation();
        }
        else
        {
            SetPortraitOrientation();
        }
    }

    // Ajustar UI para modo horizontal
    private void UpdateLayoutForLandscape()
    {
        // Aquí puedes agregar código para ajustar tu UI
        // Por ejemplo, reposicionar elementos o cambiar layouts

        // Ejemplo: Buscar y ajustar canvas scalers
        CanvasScaler[] scalers = FindObjectsOfType<CanvasScaler>();
        foreach (CanvasScaler scaler in scalers)
        {
            // Ajustar para landscape
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f; // Balance entre ancho y alto
        }
    }

    // Ajustar UI para modo vertical
    private void UpdateLayoutForPortrait()
    {
        // Ajustar UI para portrait

        CanvasScaler[] scalers = FindObjectsOfType<CanvasScaler>();
        foreach (CanvasScaler scaler in scalers)
        {
            // Ajustar para portrait
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    // Detectar cambios de orientación automáticos
    void Update()
    {
        // Opcional: Detectar cambios de orientación y ajustar UI en tiempo real
        if (Screen.orientation == ScreenOrientation.LandscapeLeft ||
            Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            // El dispositivo está en landscape
            UpdateLayoutForLandscape();
        }
        else if (Screen.orientation == ScreenOrientation.Portrait ||
                 Screen.orientation == ScreenOrientation.PortraitUpsideDown)
        {
            // El dispositivo está en portrait
            UpdateLayoutForPortrait();
        }
    }
}