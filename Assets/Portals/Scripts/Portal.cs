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

    private static Dictionary<PortalType, SimplePortal> existingPortals = new();

    private void Awake()
    {
        if (existingPortals.ContainsKey(portalType) && existingPortals[portalType] != this)
        {
            Destroy(existingPortals[portalType].gameObject);
        }
        existingPortals[portalType] = this;

        playerCam = Camera.main;

        if (destinationCamera != null)
            destinationCamera.enabled = false;

        MeshCollider col = gameObject.GetComponent<MeshCollider>();
        if (!col) col = gameObject.AddComponent<MeshCollider>();
        col.convex = true;
        col.isTrigger = true;
    }

    public void Initialize(Transform destinationTransform, Camera destCam)
    {
        destination = destinationTransform;
        destinationCamera = destCam;

        if (destinationCamera != null)
            destinationCamera.enabled = false;

        MeshCollider col = gameObject.GetComponent<MeshCollider>();
        if (!col) col = gameObject.AddComponent<MeshCollider>();
        col.convex = true;
        col.isTrigger = true;
    }


    private void OnDestroy()
    {
        if (existingPortals.ContainsKey(portalType) && existingPortals[portalType] == this)
        {
            existingPortals.Remove(portalType);
        }

        if (viewTexture != null)
        {
            viewTexture.Release();
            Destroy(viewTexture);
            Destroy(destination.gameObject);
            Destroy(createdMap);
        }
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
            Destroy(gameObject);
        }
    }
}
