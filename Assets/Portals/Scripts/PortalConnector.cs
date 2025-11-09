using UnityEngine;

public class PortalConnector : MonoBehaviour
{
    [Header("References")]
    public DrawOnGround drawer;             // Your drawing script
    public GameObject destinationPrefab;    // Prefab containing a disabled camera
    public Material portalMaterial;         // Custom material for the portal mesh
    public PortalType portalType = PortalType.TypeA; // Default type
    public Vector3 destinationOffset = new Vector3(0, 1, 0); // Where to spawn destination prefab

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

    private void OnMeshCreated(Mesh mesh, GameObject meshObject)
    {
        if (meshObject == null) return;

        // Assign custom material
        if (portalMaterial != null)
        {
            MeshRenderer renderer = meshObject.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.material = new Material(portalMaterial); // instance per portal
        }

        // Instantiate destination prefab
        GameObject destinationInstance = null;
        Camera destCam = null;

        if (destinationPrefab != null)
        {
            destinationInstance = Instantiate(destinationPrefab, destinationOffset, Quaternion.identity);
            destCam = destinationInstance.GetComponentInChildren<Camera>();
            if (destCam != null)
                destCam.enabled = false; // only render to RenderTexture
        }

        // Attach SimplePortal and initialize
        SimplePortal portal = meshObject.AddComponent<SimplePortal>();
        portal.SetPortalMesh(meshObject);

        portal.portalType = portalType;
        portal.Initialize(destinationInstance?.transform, destCam);
    }
}
