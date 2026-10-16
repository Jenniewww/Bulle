
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SplineRibbon : MonoBehaviour
{
    public SphereSplineLink splineSource;

    [Min(0.1f)]
    public float height = 2f;

    private Mesh mesh;

    void Awake()
    {
        mesh = new Mesh();
        mesh.name = "SplineRibbonMesh";
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void LateUpdate()
    {
        if (splineSource == null ||
            splineSource.spheres == null ||
            splineSource.spheres.Length < 2)
            return;

        List<Vector3> points = new List<Vector3>();
        Transform[] spheres = splineSource.spheres;
        int steps = Mathf.Max(2, splineSource.pointsPerSegment);

        for (int i = 0; i < spheres.Length - 1; i++)
        {
            if (spheres[i] == null || spheres[i + 1] == null)
                return;

            Vector3 p0 = spheres[Mathf.Max(i - 1, 0)].position;
            Vector3 p1 = spheres[i].position;
            Vector3 p2 = spheres[i + 1].position;
            Vector3 p3 = spheres[Mathf.Min(i + 2, spheres.Length - 1)].position;

            for (int j = 0; j < steps; j++)
            {
                float t = j / (float)steps;
                Vector3 p = 0.5f * (
                    2f * p1
                    + (-p0 + p2) * t
                    + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t
                    + (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
                );

                points.Add(p);
            }
        }

        points.Add(spheres[spheres.Length - 1].position);

        int n = points.Count;
        Vector3[] vertices = new Vector3[n * 2];
        Vector2[] uvs = new Vector2[n * 2];
        int[] triangles = new int[(n - 1) * 6];

        for (int i = 0; i < n; i++)
        {
            vertices[2 * i] =
                transform.InverseTransformPoint(points[i]);

            vertices[2 * i + 1] =
                transform.InverseTransformPoint(
                    points[i] + Vector3.up * height);

            float u = i / (float)(n - 1);
            uvs[2 * i] = new Vector2(u, 0f);
            uvs[2 * i + 1] = new Vector2(u, 1f);
        }

        int k = 0;

        for (int i = 0; i < n - 1; i++)
        {
            int b = 2 * i;

            triangles[k++] = b;
            triangles[k++] = b + 1;
            triangles[k++] = b + 2;

            triangles[k++] = b + 1;
            triangles[k++] = b + 3;
            triangles[k++] = b + 2;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}

