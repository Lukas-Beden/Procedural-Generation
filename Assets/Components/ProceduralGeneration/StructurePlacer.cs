using NUnit.Framework.Internal;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;
using VTools.RandomService;

public class StructurePlacer : MonoBehaviour
{
    [SerializeField] List<GameObject> structure;
    Dictionary<PortalType, GameObject> structByType = new();
    GameObject structCreated;
    PortalConnector connector;
    MyMeshGenerator generator;
    RandomService randomService;

    private void Awake()
    {
        connector = GameObject.FindGameObjectWithTag("portalConnector").GetComponent<PortalConnector>();
        generator = gameObject.GetComponent<MyMeshGenerator>();
        randomService = new(RandomNumberGenerator.GetInt32(9999));
        structByType[PortalType.Plains] = structure[0];
        structByType[PortalType.Desert] = structure[1];
    }

    public void Place()
    {
        BoxCollider prefabCol = structByType[connector.portalTypeCreated].GetComponent<BoxCollider>();

        structCreated = Instantiate(structByType[connector.portalTypeCreated],
            new Vector3(randomService.Range(0 + prefabCol.size.x, generator.width - prefabCol.size.x),
                connector.mapOffset[connector.portalTypeCreated].y,
                randomService.Range(0 + prefabCol.size.z, generator.height - prefabCol.size.z)),
            Quaternion.identity, gameObject.transform);

        AdjustChildrenHeightToTerrain(structCreated.transform);

        BoxCollider boxCol = structCreated.GetComponent<BoxCollider>();
        Vector3 halfExtents = boxCol != null ? boxCol.size / 2f : Vector3.one;

        Vector3 boxCenter = boxCol != null
            ? structCreated.transform.position + boxCol.center
            : structCreated.transform.position;

        Collider[] hitColliders = Physics.OverlapBox(boxCenter, halfExtents, structCreated.transform.rotation);

        foreach (Collider col in hitColliders)
        {
            ProcessTransformRecursive(col.transform, boxCenter, halfExtents);
        }
    }

    void AdjustChildrenHeightToTerrain(Transform parent)
    {
        BMesh mesh = generator.mesh;
        if (mesh == null) return;

        foreach (Transform child in parent)
        {
            if (child.gameObject.name == "crypt-large-roof" || child.gameObject.name == "Exit") { continue; }
            Vector3 pos = child.position;
            float terrainHeight = GetTerrainHeightAt(mesh, pos.x, pos.z);
            pos.y = terrainHeight + connector.mapOffset[connector.portalTypeCreated].y;
            child.position = pos;

            if (child.childCount > 0)
            {
                AdjustChildrenHeightToTerrain(child);
            }
        }
    }

    float GetTerrainHeightAt(BMesh mesh, float x, float z)
    {
        int ix = Mathf.Clamp(Mathf.RoundToInt(x), 0, generator.width - 1);
        int iz = Mathf.Clamp(Mathf.RoundToInt(z), 0, generator.height - 1);

        int vertexIndex = ix + iz * generator.width;

        if (vertexIndex >= 0 && vertexIndex < mesh.vertices.Count)
        {
            return mesh.vertices[vertexIndex].point.y;
        }

        return 0f;
    }

    void ProcessTransformRecursive(Transform t, Vector3 boxCenter, Vector3 halfExtents)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            Transform child = t.GetChild(i);

            if (IsInsideBox(child.position, boxCenter, halfExtents))
            {
                if (!child.CompareTag("veg") && !child.CompareTag("mapPrefabs"))
                {
                    Destroy(child.gameObject);
                    continue;
                }
            }

            ProcessTransformRecursive(child, boxCenter, halfExtents);
        }
    }

    bool IsInsideBox(Vector3 point, Vector3 boxCenter, Vector3 halfExtents)
    {
        return Mathf.Abs(point.x - boxCenter.x) <= halfExtents.x &&
               Mathf.Abs(point.y - boxCenter.y) <= halfExtents.y &&
               Mathf.Abs(point.z - boxCenter.z) <= halfExtents.z;
    }

    void Update()
    {

    }
}