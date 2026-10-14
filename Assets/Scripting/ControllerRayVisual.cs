
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ControllerRayVisual : MonoBehaviour
{
    [Header("Ray Settings")]
    public float rayLength = 5f;
    public float rayWidth = 0.008f;
    public Color rayColor = Color.cyan;

    [Header("Direction")]
    public bool useNegativeZ = false;

    private LineRenderer line;
    private Material rayMaterial;

    void Awake()
    {
        line = GetComponent<LineRenderer>();

        line.positionCount = 2;
        line.useWorldSpace = true;

        line.startWidth = rayWidth;
        line.endWidth = rayWidth;

        line.numCapVertices = 6;

        Shader shader = Shader.Find(
            "Universal Render Pipeline/Unlit"
        );

        rayMaterial = new Material(shader);
        rayMaterial.color = rayColor;

        line.material = rayMaterial;
    }

    void LateUpdate()
    {
        Vector3 direction = useNegativeZ
            ? -transform.forward
            : transform.forward;

        Vector3 start = transform.position;
        Vector3 end = start + direction * rayLength;

        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    void OnDestroy()
    {
        if (rayMaterial != null)
            Destroy(rayMaterial);
    }
}
