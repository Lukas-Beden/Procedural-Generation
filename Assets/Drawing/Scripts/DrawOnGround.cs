using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class DrawOnGround : MonoBehaviour
{
    [Header("Drawing Settings")]
    public Camera cam;
    public LayerMask groundMask;
    public LineRenderer line;

    [Header("Portal Settings")]
    public Material portalMaterial;

    public float minPointDistance = 0.05f;

    private bool isDrawing;
    private List<Vector3> points = new List<Vector3>();

    public event Action<Mesh, GameObject> MeshCreated;

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            points.Clear();
            line.positionCount = 0;
            isDrawing = true;
        }

        if (isDrawing)
        {
            if (Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out RaycastHit hit, 100f, groundMask))
            {
                Vector3 point = hit.point;
                if (points.Count == 0 || Vector3.Distance(points[^1], point) > minPointDistance)
                {
                    points.Add(point);
                    line.positionCount = points.Count;
                    line.SetPosition(points.Count - 1, point);
                }
            }
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            SmoothPoints(points);
            isDrawing = false;
            GameObject meshObj = CreateMeshFromPoints(points);
            line.positionCount = 0;

            if (meshObj != null)
            {
                MeshRenderer renderer = meshObj.GetComponent<MeshRenderer>();
                if (renderer != null && portalMaterial != null)
                {
                    renderer.material = new Material(portalMaterial);
                }

                Mesh mesh = meshObj.GetComponent<MeshFilter>().mesh;
                MeshCreated?.Invoke(mesh, meshObj);
            }
        }
    }

    List<Vector3> SmoothPoints(List<Vector3> rawPoints, float factor = 0.5f)
    {
        List<Vector3> smoothed = new List<Vector3>();
        if (rawPoints.Count < 3) return new List<Vector3>(rawPoints);

        smoothed.Add(rawPoints[0]);
        for (int i = 1; i < rawPoints.Count - 1; i++)
        {
            Vector3 prev = rawPoints[i - 1];
            Vector3 curr = rawPoints[i];
            Vector3 next = rawPoints[i + 1];

            Vector3 newPoint = curr + (prev + next - 2 * curr) * factor;
            smoothed.Add(newPoint);
        }
        smoothed.Add(rawPoints[^1]);
        return smoothed;
    }


    GameObject CreateMeshFromPoints(List<Vector3> points3D)
    {
        if (points3D.Count < 3) return null;

        Vector2[] points2D = new Vector2[points3D.Count];
        for (int i = 0; i < points3D.Count; i++)
            points2D[i] = new Vector2(points3D[i].x, points3D[i].z);

        if (GetPolygonArea(points2D) < 0)
        {
            System.Array.Reverse(points2D);
            points3D.Reverse();
        }

        Triangulator tr = new Triangulator(points2D);
        int[] indices = tr.Triangulate();

        Vector3[] vertices = points3D.ToArray();
        Vector3[] normals = new Vector3[vertices.Length];
        for (int i = 0; i < normals.Length; i++) normals[i] = Vector3.up;

        for (int i = 0; i < vertices.Length; i++)
            vertices[i] += Vector3.up * 0.01f;

        Vector2[] uvs = GenerateUVs(points3D);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.normals = normals;
        mesh.uv = uvs;

        GameObject go = new GameObject("DrawnMesh", typeof(MeshFilter), typeof(MeshRenderer));
        go.GetComponent<MeshFilter>().mesh = mesh;

        return go;
    }

    float GetPolygonArea(Vector2[] points)
    {
        float area = 0f;
        for (int i = 0; i < points.Length; i++)
        {
            int j = (i + 1) % points.Length;
            area += (points[i].x * points[j].y - points[j].x * points[i].y);
        }
        return area * 0.5f;
    }

    Vector2[] GenerateUVs(List<Vector3> points)
    {
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        foreach (var p in points)
        {
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.z < minZ) minZ = p.z;
            if (p.z > maxZ) maxZ = p.z;
        }

        float sizeX = maxX - minX;
        float sizeZ = maxZ - minZ;

        Vector2[] uvs = new Vector2[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            float u = (points[i].x - minX) / sizeX;
            float v = (points[i].z - minZ) / sizeZ;
            uvs[i] = new Vector2(u, v);
        }

        return uvs;
    }
}
