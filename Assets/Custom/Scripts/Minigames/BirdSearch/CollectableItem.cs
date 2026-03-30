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
        if (isCollected) return;

        Debug.Log($"Colisión detectada con: {other.gameObject.name}");

        bool isPlayer = other.gameObject.layer == LayerMask.NameToLayer("Player") ||
                        other.CompareTag("Player") ||
                        other.gameObject.name.ToLower().Contains("player") ||
                        other.gameObject.name.ToLower().Contains("jugador");

        if (isPlayer)
        {
            Debug.Log("¡Colisión con jugador detectada!");
            Collect();
        }
    }

    public void Collect()
    {
        if (isCollected) return;
        isCollected = true;

        if (!string.IsNullOrEmpty(birdID))
        {
            EventManager.OnBirdCollected?.Invoke(birdID);
        }

        gameObject.SetActive(false);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        FloatingObj floater = GetComponent<FloatingObj>();
        if (floater != null) floater.enabled = false;
    }
}