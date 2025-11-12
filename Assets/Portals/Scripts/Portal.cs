using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class SimplePortal : MonoBehaviour
{
    [Header("Portal Setup")]
    public MeshRenderer screen;
    public Transform destination;
    public Camera destinationCamera;
    public PortalType portalType = PortalType.Desert;
    public GameObject createdMap;

    private Camera playerCam;
    private RenderTexture viewTexture;

    private static List<GameObject> activeMaps = new();
    private static GameObject currentMap;
    private static GameObject pendingOldMap;

    private PortalConnector connector;

    private void Awake()
    {
        playerCam = Camera.main;

        if (destinationCamera != null)
            destinationCamera.enabled = false;

        MeshCollider col = GetComponent<MeshCollider>();
        col.convex = true;
        col.isTrigger = true;

        connector = GameObject.FindGameObjectWithTag("portalConnector").GetComponent<PortalConnector>();
    }

    public void Initialize(Transform destinationTransform, Camera destCam)
    {
        destination = destinationTransform;
        destinationCamera = destCam;

        if (destinationCamera != null)
            destinationCamera.enabled = false;

        MeshCollider col = GetComponent<MeshCollider>();
        col.convex = true;
        col.isTrigger = true;
    }

    private void LateUpdate()
    {
        if (!screen || !destinationCamera || !destination) return;
        RenderDestinationView();
    }

    private void RenderDestinationView()
    {
        CreateViewTexture();

        Matrix4x4 m = destination.localToWorldMatrix * transform.worldToLocalMatrix * playerCam.transform.localToWorldMatrix;
        Vector3 portalCamPos = m.GetColumn(3);

        portalCamPos.y = destination.transform.position.y;

        destinationCamera.transform.SetPositionAndRotation(portalCamPos, m.rotation);
        destinationCamera.projectionMatrix = playerCam.projectionMatrix;
        destinationCamera.Render();

        screen.material.SetTexture("_MainTex", viewTexture);
        screen.material.SetInt("displayMask", 1);
    }

    private void CreateViewTexture()
    {
        if (viewTexture == null || viewTexture.width != Screen.width || viewTexture.height != Screen.height)
        {
            if (viewTexture != null) viewTexture.Release();

            viewTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
            viewTexture.Create();

            if (destinationCamera != null)
                destinationCamera.targetTexture = viewTexture;
        }
    }

    private void OnDestroy()
    {
        Destroy(destination.gameObject);
    }

    public void SetPortalMesh(GameObject meshObject)
    {
        screen = meshObject.GetComponent<MeshRenderer>();

        MeshCollider col = meshObject.GetComponent<MeshCollider>();
        if (!col) col = meshObject.AddComponent<MeshCollider>();

        col.convex = true;
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && destination != null)
        {
            other.transform.position = destination.position;
            other.transform.rotation = destination.rotation;

            if (activeMaps.Count > 1)
            {
                GameObject oldestMap = activeMaps[0];
                if (oldestMap != null)
                    Destroy(oldestMap);

                activeMaps.RemoveAt(0);
            }

            Destroy(gameObject);
        }

        connector.ChangePortalList();
    }

    public static void SetCurrentMap(GameObject newMap)
    {
        if (newMap == null) return;

        if (activeMaps.Count > 1)
        {
            GameObject lastMap = activeMaps[activeMaps.Count - 1];
            if (lastMap != null)
                Object.Destroy(lastMap);

            activeMaps.RemoveAt(activeMaps.Count - 1);
        }
        activeMaps.Add(newMap);
    }

    public static void DeleteLastPortalMap()
    {
        if (pendingOldMap != null)
        {
            Destroy(pendingOldMap);
            pendingOldMap = null;
        }
    }

    public static GameObject GetCurrentMap() => currentMap;
}