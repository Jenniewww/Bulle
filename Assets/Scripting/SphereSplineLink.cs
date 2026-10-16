
using System.Collections.Generic;
using UnityEngine;

public class SphereSplineLink : MonoBehaviour
{
    public Transform[] spheres;
    public LineRenderer line;
    public int pointsPerSegment = 20;

    void LateUpdate()
    {
        if (line == null || spheres == null || spheres.Length < 2)
            return;

        List<Vector3> points = new List<Vector3>();

        for (int i = 0; i < spheres.Length - 1; i++)
        {
            if (spheres[i] == null || spheres[i + 1] == null)
                return;

            Vector3 p0 = spheres[Mathf.Max(i - 1, 0)].position;
            Vector3 p1 = spheres[i].position;
            Vector3 p2 = spheres[i + 1].position;
            Vector3 p3 = spheres[Mathf.Min(i + 2, spheres.Length - 1)].position;

            for (int j = 0; j < pointsPerSegment; j++)
            {
                float t = j / (float)pointsPerSegment;

                Vector3 point = 0.5f * (
                    2f * p1
                    + (-p0 + p2) * t
                    + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t
                    + (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
                );

                points.Add(point);
            }
        }

        points.Add(spheres[spheres.Length - 1].position);

        line.useWorldSpace = true;
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }
}


