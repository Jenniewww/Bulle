
using UnityEngine;
using UnityEngine.InputSystem;

public class FiveSpheresController : MonoBehaviour
{
    [Header("References")]
    public Transform head;
    public Transform[] spheres = new Transform[5];

    [Header("XR Controllers")]
    public Transform leftController;
    public Transform rightController;

    public InputActionProperty leftGrip;
    public InputActionProperty rightGrip;

    [Header("Initial Layout")]
    public float radius = 0.75f;

    [Tooltip("Vertical distance below the headset in meters.")]
    public float heightBelowHead = 0.65f;

    public float sphereDiameter = 0.16f;

    public float[] angles =
    {
        0f, -55f, 55f, -140f, 140f
    };

    [Header("Ray Grab")]
    public float maxRayDistance = 5f;
    public float selectionRadius = 0.14f;

    [Header("Follow User")]
    public bool followUser = true;

    [Header("Debug")]
    public bool debugGrip = true;

    private Vector3[] localPositions;

    private class GrabState
    {
        public int selected = -1;
        public float depth;
        public Vector3 offset;
    }

    private GrabState leftGrab = new GrabState();
    private GrabState rightGrab = new GrabState();

    void OnEnable()
    {
        EnableEmbeddedAction(leftGrip);
        EnableEmbeddedAction(rightGrip);
    }

    void EnableEmbeddedAction(InputActionProperty property)
    {
        if (property.reference == null)
        {
            property.action?.Enable();
        }
    }

    void Start()
    {
        localPositions = new Vector3[spheres.Length];

        for (int i = 0; i < spheres.Length; i++)
        {
            if (spheres[i] == null)
                continue;

            float angle = i < angles.Length
                ? angles[i] * Mathf.Deg2Rad
                : 0f;

            localPositions[i] = new Vector3(
                Mathf.Sin(angle) * radius,
                0f,
                Mathf.Cos(angle) * radius
            );

            spheres[i].localPosition =
                localPositions[i];

            spheres[i].localScale =
                Vector3.one * sphereDiameter;
        }

        PrintActionInfo("LEFT", leftGrip.action);
        PrintActionInfo("RIGHT", rightGrip.action);
    }

    void Update()
    {
        DiagnoseGripPress("LEFT", leftGrip.action);
        DiagnoseGripPress("RIGHT", rightGrip.action);

        HandleGrab(
            "LEFT",
            leftController,
            leftGrip.action,
            leftGrab
        );

        HandleGrab(
            "RIGHT",
            rightController,
            rightGrip.action,
            rightGrab
        );
    }

    void LateUpdate()
    {
        if (head == null)
            return;

        if (followUser)
        {
            // Use the headset's WORLD position.
            Vector3 headWorldPosition = head.position;

            // Follow horizontally and vertically.
            // Place the sphere controller below the head.
            Vector3 targetWorldPosition = new Vector3(
                headWorldPosition.x,
                headWorldPosition.y - heightBelowHead,
                headWorldPosition.z
            );

            transform.position = targetWorldPosition;
        }

        // Preserve manually dragged positions.
        for (int i = 0; i < spheres.Length; i++)
        {
            if (spheres[i] == null)
                continue;

            if (leftGrab.selected == i ||
                rightGrab.selected == i)
                continue;

            spheres[i].localPosition =
                localPositions[i];
        }
    }

    void HandleGrab(
        string side,
        Transform controller,
        InputAction grip,
        GrabState state)
    {
        if (controller == null ||
            grip == null ||
            !grip.enabled)
            return;

        Ray ray = new Ray(
            controller.position,
            controller.forward
        );

        if (grip.WasPressedThisFrame())
        {
            int index = FindSphereAlongRay(ray);

            if (debugGrip)
            {
                Debug.Log(
                    "[" + side + "] Selected: " + index
                );
            }

            if (index < 0)
                return;

            if (leftGrab.selected == index ||
                rightGrab.selected == index)
                return;

            state.selected = index;

            Vector3 sphereWorld =
                spheres[index].position;

            state.depth = Vector3.Dot(
                sphereWorld - ray.origin,
                ray.direction
            );

            state.offset =
                sphereWorld -
                ray.GetPoint(state.depth);
        }

        if (grip.IsPressed() &&
            state.selected >= 0)
        {
            int index = state.selected;

            Vector3 targetWorld =
                ray.GetPoint(state.depth) +
                state.offset;

            Vector3 targetLocal =
                transform.InverseTransformPoint(
                    targetWorld
                );

            // Keep points on the horizontal plane.
            targetLocal.y = localPositions[index].y;

            localPositions[index] = targetLocal;

            spheres[index].localPosition =
                targetLocal;
        }

        if (grip.WasReleasedThisFrame())
        {
            state.selected = -1;
        }
    }

    int FindSphereAlongRay(Ray ray)
    {
        int closest = -1;
        float bestDistance = selectionRadius;

        for (int i = 0; i < spheres.Length; i++)
        {
            if (spheres[i] == null)
                continue;

            if (leftGrab.selected == i ||
                rightGrab.selected == i)
                continue;

            Vector3 toSphere =
                spheres[i].position - ray.origin;

            float depth = Vector3.Dot(
                toSphere,
                ray.direction
            );

            if (depth < 0 ||
                depth > maxRayDistance)
                continue;

            Vector3 projected =
                ray.GetPoint(depth);

            float distance =
                Vector3.Distance(
                    projected,
                    spheres[i].position
                );

            if (distance < bestDistance)
            {
                bestDistance = distance;
                closest = i;
            }
        }

        return closest;
    }

    void DiagnoseGripPress(string side, InputAction action)
    {
        if (!debugGrip ||
            action == null ||
            !action.enabled)
            return;

        if (action.WasPressedThisFrame())
            Debug.Log("[" + side + "] GRIP DETECTED");

        if (action.WasReleasedThisFrame())
            Debug.Log("[" + side + "] GRIP RELEASED");
    }

    void PrintActionInfo(string side, InputAction action)
    {
        if (!debugGrip)
            return;

        if (action == null)
        {
            Debug.LogWarning(
                "[" + side + "] ACTION IS NULL"
            );
            return;
        }

        Debug.Log(
            "[" + side + "] Action: " +
            action.name +
            " | Enabled: " + action.enabled
        );
    }

    [ContextMenu("Diagnose XR Grip")]
    public void DiagnoseXRGrip()
    {
        DiagnoseAction("LEFT", leftGrip.action);
        DiagnoseAction("RIGHT", rightGrip.action);
    }

    void DiagnoseAction(string side, InputAction action)
    {
        if (action == null)
        {
            Debug.LogError(
                "[" + side + "] Action is NULL"
            );
            return;
        }

        Debug.Log(
            "[" + side + "] Enabled: " + action.enabled
        );

        foreach (var control in action.controls)
        {
            Debug.Log(
                "[" + side + "] Control: " +
                control.path
            );
        }
    }

    void OnDisable()
    {
        leftGrab.selected = -1;
        rightGrab.selected = -1;

        if (leftGrip.reference == null)
            leftGrip.action?.Disable();

        if (rightGrip.reference == null)
            rightGrip.action?.Disable();
    }
}
