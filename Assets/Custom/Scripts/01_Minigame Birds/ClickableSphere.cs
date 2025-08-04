using UnityEngine;

public class ClickableSphere : MonoBehaviour
{
    public ObjectData objectData; // Asigna desde el inspector

    void OnMouseDown()
    {
        GameManager.Instance.AddScore(objectData.pointValue);
        Destroy(gameObject);
    }
}
