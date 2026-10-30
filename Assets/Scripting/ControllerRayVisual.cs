
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

    [Tooltip("Additional local rotation of the ray, in degrees")]
    public Vector3 directionOffset = Vector3.zero;

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

        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        rayMaterial = new Material(shader);
        rayMaterial.color = rayColor;

        line.material = rayMaterial;
    }

    void LateUpdate()
    {
        Vector3 localDirection =
            useNegativeZ ? Vector3.back : Vector3.forward;

        Quaternion offsetRotation =
            Quaternion.Euler(directionOffset);

        Vector3 direction =
            transform.rotation *
            (offsetRotation * localDirection);

        Vector3 start = transform.position;
        Vector3 end = start + direction * rayLength;

        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    void OnDestroy()
    {
        if (rayMaterial != null)
        {
            Destroy(rayMaterial);
        }
    }
}
