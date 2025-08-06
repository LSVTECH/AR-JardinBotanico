using UnityEngine;

public class MapBoundary : MonoBehaviour
{
    private Vector2 bounds;
    private Vector3 center;

    public void SetBounds(Vector2 newBounds)
    {
        bounds = newBounds;
        center = transform.position;
    }

    public Vector3 ClampPosition(Vector3 position)
    {
        Vector3 localPos = position - center;
        float clampedX = Mathf.Clamp(localPos.x, -bounds.x / 2f, bounds.x / 2f);
        float clampedZ = Mathf.Clamp(localPos.z, -bounds.y / 2f, bounds.y / 2f);
        return center + new Vector3(clampedX, position.y, clampedZ);
    }

    void OnDrawGizmosSelected()
    {
        if (bounds != Vector2.zero)
        {
            Gizmos.color = Color.green;
            Vector3 size = new Vector3(bounds.x, 0.1f, bounds.y);
            Gizmos.DrawWireCube(center, size);
        }
    }
}