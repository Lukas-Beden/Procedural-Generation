using UnityEngine;
using System.Collections.Generic;

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

    [Header("Noise Settings")]
    [SerializeField] private int seed = 12345;

    private FastNoiseLite densityNoise;
    private FastNoiseLite typeNoise;
    private FastNoiseLite clusterNoise;
    private FastNoiseLite detailNoise;

    private Transform vegetationParent;

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
        densityNoise = new FastNoiseLite(seed);
        densityNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        densityNoise.SetFrequency(0.15f);
        densityNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        densityNoise.SetFractalOctaves(3);

        typeNoise = new FastNoiseLite(seed + 1000);
        typeNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        typeNoise.SetFrequency(0.2f);

        clusterNoise = new FastNoiseLite(seed + 2000);
        clusterNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        clusterNoise.SetFrequency(0.25f);
        clusterNoise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Euclidean);
        clusterNoise.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Add);

        detailNoise = new FastNoiseLite(seed + 3000);
        detailNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        detailNoise.SetFrequency(0.8f);
    }

    public void PlaceVegetation(BMesh mesh, MinMax elevationMinMax)
    {
        ClearVegetation();

        float minElevation = elevationMinMax.Min;
        float maxElevation = elevationMinMax.Max;
        float elevationRange = maxElevation - minElevation;

        foreach (BMesh.Vertex v in mesh.vertices)
        {
            Vector3 pos = v.point;

            float normalizedHeight = (pos.y - minElevation) / elevationRange;

            float slope = CalculateSlope(mesh, v);

            if (slope > maxSlope)
                continue;

            VegetationData vegData = GetVegetationData(pos.x, pos.z, normalizedHeight);

            if (vegData.shouldPlace)
            {
                PlaceVegetationAtPoint(pos, vegData);
            }
        }
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

    private void PlaceVegetationAtPoint(Vector3 position, VegetationData data)
    {
        GameObject prefab = GetPrefabForType(data.vegType);

        if (prefab == null)
            return;

        Vector3 adjustedPosition = new Vector3(position.x, position.y + yOffset + portalConnector.mapOffset[portalConnector.portalType].y, position.z);

        GameObject instance = Instantiate(prefab, adjustedPosition, Quaternion.Euler(0, data.rotation, 0), vegetationParent);
        instance.transform.localScale = Vector3.one * data.scale;
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

    private float CalculateSlope(BMesh mesh, BMesh.Vertex vertex)
    {

        int index = mesh.vertices.IndexOf(vertex);
        if (index == -1) return 0f;

        Vector3 pos = vertex.point;
        float maxHeightDiff = 0f;

        foreach (var otherVertex in mesh.vertices)
        {
            float distance = Vector3.Distance(new Vector3(pos.x, 0, pos.z),
                                             new Vector3(otherVertex.point.x, 0, otherVertex.point.z));

            if (distance > 0.1f && distance < 1.5f)
            {
                float heightDiff = Mathf.Abs(pos.y - otherVertex.point.y);
                maxHeightDiff = Mathf.Max(maxHeightDiff, heightDiff);
            }
        }

        return Mathf.Clamp01(maxHeightDiff / 2f);
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

    private enum VegetationType
    {
        Grass,
        Tree,
        Bush,
        Flower
    }
}