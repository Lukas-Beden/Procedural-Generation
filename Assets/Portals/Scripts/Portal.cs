using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class SimplePortal : MonoBehaviour
{
    [Header("Portal Setup")]
    public MeshRenderer screen;
    public Transform destination;
    public Camera destinationCamera;
    public PortalType portalType = PortalType.TypeA; // assign type in inspector

    private Camera playerCam;
    private RenderTexture viewTexture;

    // Dictionary to track existing portals by type
    private static Dictionary<PortalType, SimplePortal> existingPortals = new();

    private void Awake()
    {
        // Destroy previous portal of the same type
        if (existingPortals.ContainsKey(portalType) && existingPortals[portalType] != this)
        {
            Destroy(existingPortals[portalType].gameObject);
        }
        existingPortals[portalType] = this;

        playerCam = Camera.main;

        // Disable destination camera
        if (destinationCamera != null)
            destinationCamera.enabled = false;

        // Setup collider
        MeshCollider col = gameObject.GetComponent<MeshCollider>();
        if (!col) col = gameObject.AddComponent<MeshCollider>();
        col.convex = true;
        col.isTrigger = true;
    }

    public void Initialize(Transform destinationTransform, Camera destCam)
    {
        destination = destinationTransform;
        destinationCamera = destCam;

        // Disable destination camera so it only renders to texture
        if (destinationCamera != null)
            destinationCamera.enabled = false;

        // Ensure the portal mesh has a collider
        MeshCollider col = gameObject.GetComponent<MeshCollider>();
        if (!col) col = gameObject.AddComponent<MeshCollider>();
        col.convex = true;
        col.isTrigger = true;
    }


    private void OnDestroy()
    {
        // Remove from dictionary
        if (existingPortals.ContainsKey(portalType) && existingPortals[portalType] == this)
        {
            existingPortals.Remove(portalType);
        }

        // Release RenderTexture
        if (viewTexture != null)
        {
            viewTexture.Release();
            Destroy(viewTexture);
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
        }
    }
}
