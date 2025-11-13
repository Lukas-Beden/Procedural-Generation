using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class VegetationPlacer : MonoBehaviour
{
    [Header("Vegetation Prefabs")]
    [SerializeField] private GameObject grassPrefab;
    [SerializeField] private GameObject[] treePrefabs;
    [SerializeField] private GameObject[] bushPrefabs;
    [SerializeField] private GameObject[] flowerPrefabs;

    [Header("Density Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float grassDensity = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float treeDensity = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float bushDensity = 0.3f;

    [Header("Placement Rules")]
    [SerializeField] private float maxSlope = 0.5f;
    [SerializeField] private float minTreeHeight = 0.3f;
    [SerializeField] private float maxTreeHeight = 0.8f;
    [SerializeField] private float yOffset = -10f;
    [SerializeField] private float sampleDistance = 2f;

    [Header("Noise Settings")]
    [SerializeField] private int seed = 12345;

    private FastNoiseLite densityNoise;
    private FastNoiseLite typeNoise;
    private FastNoiseLite clusterNoise;
    private FastNoiseLite detailNoise;

    private Transform vegetationParent;

    private Dictionary<BMesh.Vertex, float> slopeCache = new Dictionary<BMesh.Vertex, float>();

    public PortalConnector portalConnector;

    private void Awake()
    {
        InitializeNoiseGenerators();

        portalConnector = GameObject.FindGameObjectWithTag("portalConnector").GetComponent<PortalConnector>();

        vegetationParent = new GameObject("Vegetation").transform;
        vegetationParent.SetParent(transform);
    }

    private void InitializeNoiseGenerators()
    {
        densityNoise = new FastNoiseLite(RandomNumberGenerator.GetInt32(9999));
        densityNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        densityNoise.SetFrequency(0.15f);
        densityNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        densityNoise.SetFractalOctaves(3);

        typeNoise = new FastNoiseLite(RandomNumberGenerator.GetInt32(9999));
        typeNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        typeNoise.SetFrequency(0.2f);

        clusterNoise = new FastNoiseLite(RandomNumberGenerator.GetInt32(9999));
        clusterNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        clusterNoise.SetFrequency(0.25f);
        clusterNoise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Euclidean);
        clusterNoise.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Add);

        detailNoise = new FastNoiseLite(RandomNumberGenerator.GetInt32(9999));
        detailNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        detailNoise.SetFrequency(0.8f);
    }

    public void PlaceVegetation(BMesh mesh, MinMax elevationMinMax)
    {
        ClearVegetation();
        slopeCache.Clear();

        float minElevation = elevationMinMax.Min;
        float maxElevation = elevationMinMax.Max;
        float elevationRange = maxElevation - minElevation;

        // Build spatial lookup for efficient neighbor finding
        Dictionary<Vector2Int, BMesh.Vertex> spatialGrid = BuildSpatialGrid(mesh);

        // Pre-batch vegetation instances to instantiate
        List<VegetationInstance> instancesToCreate = new List<VegetationInstance>();

        // Sample vertices at intervals instead of every vertex
        int sampleStep = Mathf.Max(1, Mathf.RoundToInt(sampleDistance));

        for (int i = 0; i < mesh.vertices.Count; i += sampleStep)
        {
            BMesh.Vertex v = mesh.vertices[i];
            Vector3 pos = v.point;

            float normalizedHeight = (pos.y - minElevation) / elevationRange;

            float slope = CalculateSlopeFast(mesh, v, spatialGrid);

            if (slope > maxSlope)
                continue;

            VegetationData vegData = GetVegetationData(pos.x, pos.z, normalizedHeight);

            if (vegData.shouldPlace)
            {
                instancesToCreate.Add(new VegetationInstance
                {
                    position = pos,
                    data = vegData
                });
            }
        }

        // Batch instantiate all vegetation at once
        BatchInstantiateVegetation(instancesToCreate);
    }

    private Dictionary<Vector2Int, BMesh.Vertex> BuildSpatialGrid(BMesh mesh)
    {
        Dictionary<Vector2Int, BMesh.Vertex> grid = new Dictionary<Vector2Int, BMesh.Vertex>();

        foreach (BMesh.Vertex v in mesh.vertices)
        {
            Vector2Int key = new Vector2Int(
                Mathf.RoundToInt(v.point.x),
                Mathf.RoundToInt(v.point.z)
            );

            if (!grid.ContainsKey(key))
            {
                grid[key] = v;
            }
        }

        return grid;
    }

    private float CalculateSlopeFast(BMesh mesh, BMesh.Vertex vertex, Dictionary<Vector2Int, BMesh.Vertex> spatialGrid)
    {
        // Check cache first
        if (slopeCache.TryGetValue(vertex, out float cachedSlope))
        {
            return cachedSlope;
        }

        Vector3 pos = vertex.point;
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));

        float maxHeightDiff = 0f;

        // Only check immediate neighbors (8 directions)
        Vector2Int[] neighborOffsets = new Vector2Int[]
        {
            new Vector2Int(-1, 0), new Vector2Int(1, 0),
            new Vector2Int(0, -1), new Vector2Int(0, 1),
            new Vector2Int(-1, -1), new Vector2Int(1, 1),
            new Vector2Int(-1, 1), new Vector2Int(1, -1)
        };

        foreach (Vector2Int offset in neighborOffsets)
        {
            Vector2Int neighborKey = gridPos + offset;

            if (spatialGrid.TryGetValue(neighborKey, out BMesh.Vertex neighbor))
            {
                float heightDiff = Mathf.Abs(pos.y - neighbor.point.y);
                maxHeightDiff = Mathf.Max(maxHeightDiff, heightDiff);
            }
        }

        float slope = Mathf.Clamp01(maxHeightDiff / 2f);
        slopeCache[vertex] = slope;

        return slope;
    }

    private VegetationData GetVegetationData(float x, float z, float normalizedHeight)
    {
        VegetationData data = new VegetationData();

        float density = densityNoise.GetNoise(x, z);
        float cluster = clusterNoise.GetNoise(x, z);
        float type = typeNoise.GetNoise(x, z);
        float detail = detailNoise.GetNoise(x, z);

        float finalDensity = (density * 0.6f + cluster * 0.4f);

        data.shouldPlace = finalDensity > -0.2f;

        if (!data.shouldPlace)
            return data;

        data.vegType = DetermineVegetationType(type, normalizedHeight);
        data.scale = 0.7f + detail * 0.6f;
        data.rotation = detail * 360f;

        switch (data.vegType)
        {
            case VegetationType.Tree:
                data.shouldPlace = finalDensity > (0.5f - treeDensity);
                break;
            case VegetationType.Bush:
                data.shouldPlace = finalDensity > (0.3f - bushDensity);
                break;
            case VegetationType.Grass:
                data.shouldPlace = finalDensity > (0.1f - grassDensity);
                break;
        }

        return data;
    }

    private VegetationType DetermineVegetationType(float typeValue, float height)
    {
        if (height > 0.7f)
        {
            return typeValue > 0.5f ? VegetationType.Tree : VegetationType.Grass;
        }
        else if (height > 0.4f)
        {
            if (typeValue > 0.6f) return VegetationType.Tree;
            if (typeValue > 0.2f) return VegetationType.Bush;
            return VegetationType.Grass;
        }
        else
        {
            if (typeValue > 0.7f) return VegetationType.Tree;
            if (typeValue > 0.3f) return VegetationType.Bush;
            if (typeValue > -0.2f) return VegetationType.Grass;
            return VegetationType.Flower;
        }
    }

    private void BatchInstantiateVegetation(List<VegetationInstance> instances)
    {
        foreach (VegetationInstance instance in instances)
        {
            GameObject prefab = GetPrefabForType(instance.data.vegType);

            if (prefab == null)
                continue;

            Vector3 adjustedPosition = new Vector3(
                instance.position.x,
                instance.position.y + yOffset + portalConnector.mapOffset[portalConnector.portalType].y,
                instance.position.z
            );

            GameObject obj = Instantiate(prefab, adjustedPosition, Quaternion.Euler(0, instance.data.rotation, 0), vegetationParent);
            obj.transform.localScale = Vector3.one * instance.data.scale;
        }
    }

    private GameObject GetPrefabForType(VegetationType type)
    {
        switch (type)
        {
            case VegetationType.Grass:
                return grassPrefab;
            case VegetationType.Tree:
                return treePrefabs != null && treePrefabs.Length > 0
                    ? treePrefabs[Random.Range(0, treePrefabs.Length)]
                    : null;
            case VegetationType.Bush:
                return bushPrefabs != null && bushPrefabs.Length > 0
                    ? bushPrefabs[Random.Range(0, bushPrefabs.Length)]
                    : null;
            case VegetationType.Flower:
                return flowerPrefabs != null && flowerPrefabs.Length > 0
                    ? flowerPrefabs[Random.Range(0, flowerPrefabs.Length)]
                    : null;
            default:
                return null;
        }
    }

    public void ClearVegetation()
    {
        if (vegetationParent != null)
        {
            foreach (Transform child in vegetationParent)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private struct VegetationData
    {
        public bool shouldPlace;
        public VegetationType vegType;
        public float scale;
        public float rotation;
    }

    private struct VegetationInstance
    {
        public Vector3 position;
        public VegetationData data;
    }

    private enum VegetationType
    {
        Grass,
        Tree,
        Bush,
        Flower
    }
}