        using Components.ProceduralGeneration.SimpleRoomPlacement;
using Unity.Mathematics;
using UnityEngine;
using static BMesh;

public class MyMeshGenerator : MonoBehaviour
{
    public int width = 15;
    public int height = 15;

    BMesh mesh;

    [SerializeField] private NoiseGenerator noiseGenerator;
    [SerializeField] private Material _material;
    [SerializeField] private Gradient gradient;

    public MinMax _elevationMinMax;
    public ColourGenerator _colourGenerator;

    [SerializeField] private VegetationPlacer vegetationPlacer;
    [SerializeField] private PortalConnector portalConnector;

    void Start()
    {
        portalConnector = GameObject.FindGameObjectWithTag("portalConnector").GetComponent<PortalConnector>();
        GenerateGrid();
    }

    public BMesh GenerateGrid()
    {

        _elevationMinMax = new MinMax();
        _colourGenerator = new ColourGenerator(_material);
        Texture2D gradientTex = GradientUtils.CreateGradientTexture(gradient);
        _material.SetTexture("_GradientTex", gradientTex);

        BMesh bm = new BMesh();
        for (int j = 0; j < height; ++j)
        {
            for (int i = 0; i < width; ++i)
            {
                bm.AddVertex(i, 0, j);
                if (i > 0 && j > 0) bm.AddFace(i + j * width, i - 1 + j * width, i - 1 + (j - 1) * width, i + (j - 1) * width);
            }
        }

        mesh = bm;
        noiseGenerator.CreateNoise();
        foreach (Vertex v in mesh.vertices)
        {
            v.point.y = noiseGenerator.GetNoiseDataNotClamped(noiseGenerator._noise, (int)v.point.x, (int)v.point.z);
            _elevationMinMax.AddValue(v.point.y);
        }
        BMeshUnity.SetInMeshFilter(mesh, GetComponent<MeshFilter>());
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            Destroy(meshCollider);
        }
        gameObject.GetComponent<MeshRenderer>().material = _material;
        _colourGenerator.UpdateElevation(_elevationMinMax);
        gameObject.AddComponent<MeshCollider>();

        if (vegetationPlacer != null)
        {
            vegetationPlacer.PlaceVegetation(mesh, _elevationMinMax);
        }

        portalConnector.portalType = PortalType.Desert;

        return bm;
    }
}