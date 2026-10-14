
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MeshFilter))]
public class BulleDeformer : MonoBehaviour
{
    [Header("Control Points")]
    [Range(10, 250)]
    public int targetPointCount = 120;

    public float pointSize = 1f;
    public Color pointColor = Color.cyan;
    public Color selectedColor = Color.yellow;

    [Header("Smooth Deformation (meters)")]
    [Min(0.01f)]
    public float deformationRadius = 0.45f;

    [Range(0.1f, 1f)]
    public float deformationStrength = 0.8f;

    [Header("XR Controllers")]
    public Transform leftController;
    public Transform rightController;
    public InputActionProperty leftGrip;
    public InputActionProperty rightGrip;

    [Header("Ray Selection (meters)")]
    public float maxRayDistance = 5f;
    public float selectionRadius = 0.12f;

    private Mesh mesh;
    private Vector3[] vertices;

    private readonly List<Transform> points = new();
    private readonly List<int> pointIndices = new();

    private Material normalMaterial;
    private Material selectedMaterial;

    private class GrabState
    {
        public int selected = -1;
        public float depth;
        public Vector3 offset;
        public Vector3 startPointWorld;
        public Vector3[] originalVertices;
        public Vector3[] originalWorldPositions;
    }

    private readonly GrabState leftState = new();
    private readonly GrabState rightState = new();

    private bool visible = true;

    void OnEnable()
    {
        leftGrip.action?.Enable();
        rightGrip.action?.Enable();
    }

    void Start()
    {
        MeshFilter filter = GetComponent<MeshFilter>();

        mesh = Instantiate(filter.sharedMesh);
        mesh.name = "Bulle Runtime Mesh";
        filter.mesh = mesh;

        vertices = mesh.vertices;

        CreateMaterials();
        CreatePoints();

        Debug.Log(
            $"Bulle ready: {vertices.Length} vertices, " +
            $"{points.Count} control points"
        );
    }

    void CreateMaterials()
    {
        Shader shader = Shader.Find(
            "Universal Render Pipeline/Unlit"
        );

        normalMaterial = new Material(shader);
        normalMaterial.color = pointColor;

        selectedMaterial = new Material(shader);
        selectedMaterial.color = selectedColor;
    }

    void CreatePoints()
    {
        // 空间均匀采样：优先选择距离已有点较远的顶点 répartition des points dans l'environnement au hasard
        List<int> candidates = new();

        // 限制候选数量，避免过多距离计算
        int stride = Mathf.Max(1, vertices.Length / 3000);

        for (int i = 0; i < vertices.Length; i += stride)
            candidates.Add(i);

        if (candidates.Count == 0)
            return;

        List<int> chosen = new();
        float[] minDistances = new float[candidates.Count];

        for (int i = 0; i < minDistances.Length; i++)
            minDistances[i] = float.PositiveInfinity;

        int current = 0;
        int count = Mathf.Min(targetPointCount, candidates.Count);

        for (int p = 0; p < count; p++)
        {
            int vertexIndex = candidates[current];
            chosen.Add(vertexIndex);

            Vector3 selectedPosition =
                transform.TransformPoint(vertices[vertexIndex]);

            float bestDistance = -1f;
            int next = current;

            for (int c = 0; c < candidates.Count; c++)
            {
                Vector3 candidatePosition =
                    transform.TransformPoint(vertices[candidates[c]]);

                float d = (
                    candidatePosition - selectedPosition
                ).sqrMagnitude;

                minDistances[c] = Mathf.Min(
                    minDistances[c], d
                );

                if (minDistances[c] > bestDistance)
                {
                    bestDistance = minDistances[c];
                    next = c;
                }
            }

            current = next;
        }

        GameObject template =
            GameObject.CreatePrimitive(PrimitiveType.Sphere);

        template.SetActive(false);

        foreach (int index in chosen)
        {
            GameObject obj = Instantiate(template, transform);

            obj.name = "ControlPoint_" + index;
            obj.transform.localPosition = vertices[index];
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one * pointSize;

            obj.GetComponent<Renderer>().sharedMaterial =
                normalMaterial;

            Collider collider = obj.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            obj.SetActive(true);

            points.Add(obj.transform);
            pointIndices.Add(index);
        }

        Destroy(template);
    }

    void Update()
    {
        if (!visible) return;

        HandleGrip(leftController, leftGrip.action, leftState);
        HandleGrip(rightController, rightGrip.action, rightState);
    }

    void HandleGrip(
        Transform controller,
        InputAction grip,
        GrabState state)
    {
        if (controller == null || grip == null)
            return;

        Ray ray = new Ray(
            controller.position,
            controller.forward
        );

        if (grip.WasPressedThisFrame())
        {
            int index = FindPointAlongRay(ray);

            if (index >= 0 &&
                leftState.selected != index &&
                rightState.selected != index)
            {
                state.selected = index;

                Vector3 pointWorld = points[index].position;

                state.depth = Vector3.Dot(
                    pointWorld - ray.origin,
                    ray.direction
                );

                state.offset =
                    pointWorld - ray.GetPoint(state.depth);

                state.startPointWorld = pointWorld;

                // 保存抓取开始时的完整形状
                state.originalVertices =
                    (Vector3[])vertices.Clone();

                state.originalWorldPositions =
                    new Vector3[vertices.Length];

                for (int i = 0; i < vertices.Length; i++)
                {
                    state.originalWorldPositions[i] =
                        transform.TransformPoint(vertices[i]);
                }

                SetPointColor(index, true);

                Debug.Log("Grip selected: " + index);
            }
        }

        if (grip.IsPressed() && state.selected >= 0)
        {
            Vector3 target =
                ray.GetPoint(state.depth) + state.offset;

            Deform(state, target);
        }

        if (grip.WasReleasedThisFrame())
        {
            if (state.selected >= 0)
                SetPointColor(state.selected, false);

            state.selected = -1;
        }
    }

    int FindPointAlongRay(Ray ray)
    {
        int closest = -1;
        float bestDistance = selectionRadius;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 toPoint = points[i].position - ray.origin;

            float depth = Vector3.Dot(
                toPoint, ray.direction
            );

            if (depth < 0 || depth > maxRayDistance)
                continue;

            Vector3 projected = ray.GetPoint(depth);

            float distance = Vector3.Distance(
                projected, points[i].position
            );

            if (distance < bestDistance)
            {
                bestDistance = distance;
                closest = i;
            }
        }

        return closest;
    }

    void Deform(GrabState state, Vector3 targetWorld)
    {
        Vector3 displacement =
            targetWorld - state.startPointWorld;

        Vector3 localDisplacement =
            transform.InverseTransformVector(displacement);

        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = Vector3.Distance(
                state.originalWorldPositions[i],
                state.startPointWorld
            );

            if (distance >= deformationRadius)
            {
                vertices[i] = state.originalVertices[i];
                continue;
            }

            float t = Mathf.Clamp01(
                distance / deformationRadius
            );

            // Smoothstep falloff
            float weight =
                1f - t * t * (3f - 2f * t);

            vertices[i] =
                state.originalVertices[i] +
                localDisplacement *
                weight *
                deformationStrength;
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        for (int i = 0; i < points.Count; i++)
        {
            points[i].localPosition =
                vertices[pointIndices[i]];
        }
    }

    void SetPointColor(int index, bool selected)
    {
        if (index < 0 || index >= points.Count)
            return;

        points[index].GetComponent<Renderer>().sharedMaterial =
            selected ? selectedMaterial : normalMaterial;
    }

    public void SetBulleVisible(bool show)
    {
        visible = show;

        MeshRenderer renderer = GetComponent<MeshRenderer>();

        if (renderer != null)
            renderer.enabled = show;

        foreach (Transform point in points)
        {
            if (point != null)
                point.gameObject.SetActive(show);
        }

        if (!show)
        {
            leftState.selected = -1;
            rightState.selected = -1;
        }
    }

    public bool IsBulleVisible()
    {
        return visible;
    }

    void OnDestroy()
    {
        if (normalMaterial != null)
            Destroy(normalMaterial);

        if (selectedMaterial != null)
            Destroy(selectedMaterial);

        if (mesh != null)
            Destroy(mesh);
    }
}
