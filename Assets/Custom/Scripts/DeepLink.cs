using UnityEngine;

public class DeepLink : MonoBehaviour
{
    [Header("Configuración de URL")]
    [Tooltip("URL que se abrirá cuando se active la función")]
    public string urlToOpen = "https://www.google.com";
    public void OpenURL()
    {
        if (!string.IsNullOrEmpty(urlToOpen))
        {
            Debug.Log($"Abriendo URL: {urlToOpen}");
            Application.OpenURL(urlToOpen);
        }
        else
        {
            Debug.LogWarning("La URL está vacía. Por favor, especifica una URL válida.");
        }
    }
        public void OpenSpecificURL(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            Debug.Log($"Abriendo URL específica: {url}");
            Application.OpenURL(url);
        }
        else
        {
            Debug.LogWarning("La URL proporcionada está vacía.");
        }
    }
}
