using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARMapPlacer : MonoBehaviour
{
    [SerializeField] GameObject mapPrefab;
    [SerializeField] ARRaycastManager raycastManager;
    [SerializeField] GameObject startButton;

    private GameObject spawnedMap;
    private bool placementMode = true;

    void Update()
    {
        if (!placementMode) return;

        // Detectar toque en pantalla
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            Vector2 touchPosition = Input.GetTouch(0).position;

            if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                PlaceMap(hitPose.position);
            }
        }
    }

    void PlaceMap(Vector3 position)
    {
        if (spawnedMap == null)
        {
            spawnedMap = Instantiate(mapPrefab, position, Quaternion.identity);
        }
        else
        {
            spawnedMap.transform.position = position;
        }

        // Activar controles al colocar el mapa
        placementMode = false;
        startButton.SetActive(false);
        GetComponent<PlayerController>().enabled = true;
    }

    // Llamado por el botón UI
    public void StartPlacement()
    {
        placementMode = true;
        startButton.SetActive(false);
    }
}