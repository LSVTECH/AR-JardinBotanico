using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CollectableItem : MonoBehaviour
{
    public int itemValue = 1;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
    }

   void OnTriggerEnter(Collider other)
{
    // Verificar primero si estamos en el modo correcto
    if (GameManager.Instance != null && 
        GameManager.Instance.currentGameMode != GameManager.GameMode.ObjectSearch)
    {
        // Si no estamos en modo ObjectSearch, ignorar todas las colisiones
        return;
    }
    
    // Depuración importante
    Debug.Log($"Colisión detectada con: {other.gameObject.name}");
    
    // Verificar si es el jugador por capa, tag o nombre
    bool isPlayer = false;
    
    // Verificar por capa
    if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
    {
        isPlayer = true;
        Debug.Log("Jugador detectado por capa");
    }
    // Verificar por tag
    else if (other.CompareTag("Player"))
    {
        isPlayer = true;
        Debug.Log("Jugador detectado por tag");
    }
    // Verificar por nombre (fallback)
    else if (other.gameObject.name.ToLower().Contains("player") || 
             other.gameObject.name.ToLower().Contains("jugador"))
    {
        isPlayer = true;
        Debug.Log("Jugador detectado por nombre");
    }
    
    if (isPlayer)
    {
        Debug.Log("¡Colisión con jugador detectada!");
        Collect();
    }
    else
    {
        Debug.Log($"Objeto no reconocido como jugador: {other.gameObject.name} (Layer: {other.gameObject.layer}, Tag: {other.tag})");
    }
}

public void Collect()
{
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
            GameManager.Instance.AddScore(itemValue);
            
            // Desactivar el objeto en lugar de destruirlo
            gameObject.SetActive(false);
            
            // Desactivar el collider para evitar colisiones múltiples
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }
            
            // Desactivar el componente FloatingObj si existe
            FloatingObj floater = GetComponent<FloatingObj>();
            if (floater != null)
            {
                floater.enabled = false;
            }
        }
        else
        {
            Debug.LogWarning($"Modo incorrecto: {GameManager.Instance.currentGameMode}. Se esperaba ObjectSearch. Ignorando colisión.");
        }
    }
    else
    {
        Debug.LogError("GameManager no encontrado en la escena");
    }
}
}