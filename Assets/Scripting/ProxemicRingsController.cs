
using UnityEngine;
using UnityEngine.InputSystem;

public class ProxemicRingsController : MonoBehaviour
{
    [Header("Follow the existing FiveSpheres")]
    public Transform fiveSpheres;

    [Header("Ring Radii (meters)")]
    public float intimateRadius = 0.45f;
    public float personalRadius = 1.20f;
    public float socialRadius = 3.60f;
    public float publicRadius = 7.50f;

    [Header("Appearance")]
    public float lineWidth = 0.035f;
    [Range(32, 256)]
    public int segments = 128;

    [Header("Height Offset")]
    public float verticalOffset = 0.02f;

    [Header("Initial Visibility")]
    public bool showIntimate = true;
    public bool showPersonal = true;
    public bool showSocial = true;
    public bool showPublic = true;

    private LineRenderer[] rings =
        new LineRenderer[4];

    private float[] radii = new float[4];

    private bool[] visible = new bool[4];

    private Color[] colors =
    {
        new Color(1f, 0.15f, 0.15f),   // Red
        new Color(1f, 0.60f, 0.05f),   // Orange
        new Color(0.15f, 1f, 0.25f),   // Green
        new Color(0.60f, 0.25f, 1f)    // Purple
    };

    private string[] ringNames =
    {
        "01_Distance_Intime",
        "02_Distance_Personnelle",
        "03_Distance_Sociale",
        "04_Distance_Publique"
    };

    void Start()
    {
        radii[0] = intimateRadius;
        radii[1] = personalRadius;
        radii[2] = socialRadius;
        radii[3] = publicRadius;

        visible[0] = showIntimate;
        visible[1] = showPersonal;
        visible[2] = showSocial;
        visible[3] = showPublic;

        Shader shader =
            Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        if (shader == null)
        {
            Debug.LogError(
                "No compatible shader found for rings."
            );
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            GameObject ringObject =
                new GameObject(ringNames[i]);

            ringObject.transform.SetParent(
                transform, false
            );

            LineRenderer lr =
                ringObject.AddComponent<LineRenderer>();

            lr.useWorldSpace = false;
            lr.loop = true;
            lr.positionCount = segments;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;

            lr.numCornerVertices = 4;
            lr.numCapVertices = 4;

            Material material =
                new Material(shader);

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", colors[i]);

            if (material.HasProperty("_Color"))
                material.SetColor("_Color", colors[i]);

            lr.material = material;
            lr.startColor = colors[i];
            lr.endColor = colors[i];

            rings[i] = lr;

            DrawRing(i);
            lr.enabled = visible[i];
        }
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.digit1Key.wasPressedThisFrame)
            ToggleRing(0);

        if (keyboard.digit2Key.wasPressedThisFrame)
            ToggleRing(1);

        if (keyboard.digit3Key.wasPressedThisFrame)
            ToggleRing(2);

        if (keyboard.digit4Key.wasPressedThisFrame)
            ToggleRing(3);
    }

    void LateUpdate()
    {
        if (fiveSpheres == null)
            return;

        // Follow the already calibrated sphere center.
        transform.position =
            fiveSpheres.position +
            Vector3.up * verticalOffset;

        // Horizontal rings stay horizontal.
        transform.rotation = Quaternion.identity;
    }

    void DrawRing(int index)
    {
        if (rings[index] == null)
            return;

        float radius = radii[index];

        for (int i = 0; i < segments; i++)
        {
            float angle =
                2f * Mathf.PI * i / segments;

            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;

            rings[index].SetPosition(
                i,
                new Vector3(x, 0f, z)
            );
        }
    }

    public void ToggleRing(int index)
    {
        if (index < 0 || index >= rings.Length)
            return;

        if (rings[index] == null)
            return;

        visible[index] = !visible[index];
        rings[index].enabled = visible[index];

        Debug.Log(
            ringNames[index] +
            " visible: " + visible[index]
        );
    }

    public void SetRingVisibility(
        int index, bool isVisible)
    {
        if (index < 0 || index >= rings.Length)
            return;

        if (rings[index] == null)
            return;

        visible[index] = isVisible;
        rings[index].enabled = isVisible;
    }
}
