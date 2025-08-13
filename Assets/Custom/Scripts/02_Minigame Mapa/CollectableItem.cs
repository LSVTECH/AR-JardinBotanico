using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CollectableItem : MonoBehaviour
{
    public int itemValue = 1;
    public string birdID; // Identificador único para cada ave

    private bool isCollected = false; // Bandera para controlar recolección única

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // Si ya fue recolectado, ignorar
        if (isCollected) return;

        // Verificar primero si estamos en el modo correcto
        if (GameManager.Instance != null &&
            GameManager.Instance.currentGameMode != GameManager.GameMode.ObjectSearch)
        {
            return;
        }

        // Depuración importante
        Debug.Log($"Colisión detectada con: {other.gameObject.name}");

        // Verificar si es el jugador
        bool isPlayer = other.gameObject.layer == LayerMask.NameToLayer("Player") ||
                        other.CompareTag("Player") ||
                        other.gameObject.name.ToLower().Contains("player") ||
                        other.gameObject.name.ToLower().Contains("jugador");

        if (isPlayer)
        {
            Debug.Log("¡Colisión con jugador detectada!");
            Collect();
        }
        else
        {
            Debug.Log($"Objeto no reconocido como jugador: {other.gameObject.name}");
        }
    }

    public void Collect()
    {
        // Si ya fue recolectado, ignorar
        if (isCollected) return;
        isCollected = true;

        Debug.Log("Intentando recolectar objeto...");

        // Buscar GameManager si Instance es null
        if (GameManager.Instance == null)
        {
            GameManager.Instance = FindObjectOfType<GameManager>();
            Debug.Log($"GameManager encontrado: {GameManager.Instance != null}");
        }

        if (GameManager.Instance != null)
        {
            Debug.Log($"Modo actual: {GameManager.Instance.currentGameMode}");

            // Verificar que estamos en el modo correcto
            if (GameManager.Instance.currentGameMode == GameManager.GameMode.ObjectSearch)
            {
                Debug.Log("Recolectando en modo ObjectSearch");

                // Notificar al GameManager sobre esta ave específica
                if (!string.IsNullOrEmpty(birdID))
                {
                    GameManager.Instance.CollectBird(birdID);
                }

                GameManager.Instance.AddScore(itemValue);

                // Desactivar el objeto
                gameObject.SetActive(false);

                // Desactivar el collider
                Collider col = GetComponent<Collider>();
                if (col != null) col.enabled = false;

                // Desactivar el componente FloatingObj si existe
                FloatingObj floater = GetComponent<FloatingObj>();
                if (floater != null) floater.enabled = false;
            }
            else
            {
                Debug.LogWarning("Modo incorrecto. Ignorando colisión.");
            }
        }
        else
        {
            Debug.LogError("GameManager no encontrado en la escena");
        }
    }
}