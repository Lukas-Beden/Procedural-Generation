using System.Collections.Generic;
using UnityEngine;

public class PortalConnector : MonoBehaviour
{
    [Header("References")]
    public DrawOnGround drawer;
    public GameObject destinationPrefab;
    public List<GameObject> mapPrefabs;
    public List<PortalType> portalTypeObtained;
    public List<PortalType> utilisablePortalType;
    public Material portalMaterial;
    public PortalType portalType = PortalType.Plains;
    public PortalType portalTypeCreated = PortalType.Plains;
    public Vector2 destinationXY = new();

    private Dictionary<PortalType, GameObject> mapPrefabsByType = new();
    public Dictionary<PortalType, Vector3> mapOffset = new();

    public bool hasStarted = false;

    private void Awake()
    {
        SetDictionaryMapPrefab();
    }

    private void Start()
    {
        if (mapPrefabsByType.ContainsKey(portalType))
        {
            Vector3 startPosition = mapOffset[portalType];
            GameObject startingMap = Instantiate(mapPrefabsByType[portalType], startPosition, Quaternion.identity);
            SimplePortal.SetCurrentMap(startingMap);

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = startPosition + new Vector3(0, 10f, 0);
            }
        }
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

    private void SetDictionaryMapPrefab()
    {
        mapPrefabsByType[PortalType.Plains] = mapPrefabs[0];
        mapPrefabsByType[PortalType.Desert] = mapPrefabs[1];
        mapPrefabsByType[PortalType.Mountain] = mapPrefabs[2];
        mapOffset[PortalType.Plains] = new Vector3(0, 10000, 0);
        mapOffset[PortalType.Desert] = new Vector3(0, 0, 0);
        mapOffset[PortalType.Mountain] = new Vector3(0, -10000, 0);
    }

    private void OnMeshCreated(Mesh mesh, GameObject meshObject)
    {
        if (!portalTypeObtained.Contains(portalType))
        {
            return;
        }

        SimplePortal.DeleteLastPortalMap();

        foreach (var oldPortal in FindObjectsOfType<SimplePortal>())
        {
            Destroy(oldPortal.gameObject);
            Destroy(oldPortal.destination.gameObject);
        }

        portalTypeCreated = portalType;

        var portal = meshObject.AddComponent<SimplePortal>();
        portal.SetPortalMesh(meshObject);

        if (portalMaterial != null)
        {
            var renderer = meshObject.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.material = new Material(portalMaterial);
        }

        GameObject newMap = null;
        GameObject destinationInstance = null;
        Camera destCam = null;

        if (destinationPrefab != null && mapPrefabsByType.ContainsKey(portalType))
        {
            Vector3 destinationOffset = mapOffset[portalType];
            destinationOffset.x += destinationXY.x;
            destinationOffset.y += 100;
            destinationOffset.z += destinationXY.y;
            newMap = Object.Instantiate(mapPrefabsByType[portalType], mapOffset[portalType], Quaternion.identity);

            destinationInstance = Object.Instantiate(destinationPrefab, destinationOffset, Quaternion.identity);
            destCam = destinationInstance.GetComponentInChildren<Camera>();
            if (destCam != null)
                destCam.enabled = false;
        }

        if (destinationInstance == null || destCam == null)
        {
            Debug.LogWarning("Destination or camera missing for portal.");
            return;
        }

        portal.Initialize(destinationInstance.transform, destCam);
        portal.createdMap = newMap;

        SimplePortal.SetCurrentMap(newMap);
    }

    public void ChangePortalList()
    {
        utilisablePortalType.Clear();

        foreach (PortalType obtPortalType in portalTypeObtained)
        {
            if (obtPortalType != portalTypeCreated)
            {
                utilisablePortalType.Add(obtPortalType);
            }
        }
    }
}
