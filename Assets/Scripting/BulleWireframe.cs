
using UnityEngine;
using System.Collections.Generic;

public class BulleWireframe : MonoBehaviour
{
    [Header("Wireframe Settings")]
    public int latitudeLines = 9;
    public int longitudeLines = 16;
    public int segments = 100;
    public float lineWidth = 0.012f;
    public Color lineColor =
        new Color(0.65f, 0.9f, 1f, 0.75f);

    private List<LineRenderer> lines =
        new List<LineRenderer>();

    void Start()
    {
        GenerateWireframe();
    }

    void GenerateWireframe()
    {
        Material lineMaterial = new Material(
            Shader.Find("Sprites/Default")
        );

        lineMaterial.color = lineColor;

        // Latitude lines
        for (int i = 1; i <= latitudeLines; i++)
        {
            float phi =
                Mathf.PI * i / (latitudeLines + 1);

            float y = 0.5f * Mathf.Cos(phi);
            float radius = 0.5f * Mathf.Sin(phi);

            LineRenderer lr = CreateLine(lineMaterial);

            for (int j = 0; j <= segments; j++)
            {
                float theta =
                    2f * Mathf.PI * j / segments;

                lr.SetPosition(j, new Vector3(
                    radius * Mathf.Cos(theta),
                    y,
                    radius * Mathf.Sin(theta)
                ));
            }
        }

        // Longitude lines
        for (int i = 0; i < longitudeLines; i++)
        {
            float theta =
                2f * Mathf.PI * i / longitudeLines;

            LineRenderer lr = CreateLine(lineMaterial);

            for (int j = 0; j <= segments; j++)
            {
                float phi =
                    2f * Mathf.PI * j / segments;

                lr.SetPosition(j, new Vector3(
                    0.5f * Mathf.Sin(phi) *
                        Mathf.Cos(theta),
                    0.5f * Mathf.Cos(phi),
                    0.5f * Mathf.Sin(phi) *
                        Mathf.Sin(theta)
                ));
            }
        }
    }

    LineRenderer CreateLine(Material mat)
    {
        GameObject obj =
            new GameObject("Bulle Grid Line");

        obj.transform.SetParent(
            transform, false
        );

        LineRenderer lr =
            obj.AddComponent<LineRenderer>();

        lr.useWorldSpace = false;
        lr.positionCount = segments + 1;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.material = mat;
        lr.startColor = lineColor;
        lr.endColor = lineColor;
        lr.numCornerVertices = 4;
        lr.numCapVertices = 4;

        lines.Add(lr);

        return lr;
    }
}
