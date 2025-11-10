using System.Collections.Generic;
using UnityEngine;

public class PortalConnector : MonoBehaviour
{
    [Header("References")]
    public DrawOnGround drawer;
    public GameObject destinationPrefab;
    public List<GameObject> mapPrefabs;
    public Dictionary<PortalType, GameObject> mapPrefabsByType = new();
    public Material portalMaterial;
    public PortalType portalType = PortalType.Mountain; 
    public Vector3 destinationOffset = new Vector3(0, 1, 0);

    private void Awake()
    {
        SetDictionnaryMapPrefab();
    }

    private void OnEnable()
    {
        if (drawer != null)
            drawer.MeshCreated += OnMeshCreated;
    }

    private void OnDisable()
    {
        if (drawer != null)
            drawer.MeshCreated -= OnMeshCreated;
    }

    private void SetDictionnaryMapPrefab()
    {
        mapPrefabsByType[PortalType.Plains] = mapPrefabs[0];
        mapPrefabsByType[PortalType.Desert] = mapPrefabs[1];
        mapPrefabsByType[PortalType.Mountain] = mapPrefabs[2];
    }

    private void OnMeshCreated(Mesh mesh, GameObject meshObject)
    {
        if (meshObject == null) return;

        if (portalMaterial != null)
        {
            MeshRenderer renderer = meshObject.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.material = new Material(portalMaterial);
        }

        GameObject destinationInstance = null;
        Camera destCam = null;
        GameObject newMap = null;

        if (destinationPrefab != null)
        {
            Vector3 mapOffset = destinationOffset;
            mapOffset.y = 0;
            newMap = Instantiate(mapPrefabsByType[portalType], mapOffset, Quaternion.identity); 
            destinationInstance = Instantiate(destinationPrefab, destinationOffset, Quaternion.identity);
            destCam = destinationInstance.GetComponentInChildren<Camera>();
            if (destCam != null)
                destCam.enabled = false;
        }

        SimplePortal portal = meshObject.AddComponent<SimplePortal>();
        portal.SetPortalMesh(meshObject);

        portal.portalType = portalType;
        portal.Initialize(destinationInstance?.transform, destCam);

        portal.createdMap = newMap;
    }
}
