
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
    public float heightBelowHead = 0.55f;
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

    private float fixedHeight;
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
            property.action?.Enable();
    }

    void Start()
    {
        localPositions = new Vector3[spheres.Length];

        // Preserve the original fixed-height behavior
        if (head != null)
        {
            fixedHeight =
                head.position.y - heightBelowHead;
        }

        // Initialize sphere positions
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

        Debug.Log(
            "[FiveSpheres] Controller initialized."
        );

        PrintActionInfo("LEFT", leftGrip.action);
        PrintActionInfo("RIGHT", rightGrip.action);
    }

    void PrintActionInfo(
        string side,
        InputAction action)
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
            " | Enabled: " + action.enabled +
            " | Bindings: " +
            action.bindings.Count
        );

        foreach (var control in action.controls)
        {
            Debug.Log(
                "[" + side + "] Resolved control: " +
                control.path
            );
        }
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

    void DiagnoseGripPress(
        string side,
        InputAction action)
    {
        if (!debugGrip || action == null ||
            !action.enabled)
            return;

        if (action.WasPressedThisFrame())
        {
            Debug.Log(
                "[" + side + "] GRIP DETECTED"
            );
        }

        if (action.WasReleasedThisFrame())
        {
            Debug.Log(
                "[" + side + "] GRIP RELEASED"
            );
        }
    }

    void LateUpdate()
    {
        if (head == null)
            return;

        if (followUser)
        {
            Vector3 targetPosition = head.position;

            // Follow horizontally only
            targetPosition.y = fixedHeight;

            transform.position = targetPosition;
        }

        // Do not rotate with the head

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

        // Select sphere on Grip press
        if (grip.WasPressedThisFrame())
        {
            int index = FindSphereAlongRay(ray);

            if (debugGrip)
            {
                Debug.Log(
                    "[" + side + "] Ray origin: " +
                    ray.origin +
                    " | Direction: " +
                    ray.direction +
                    " | Selected index: " +
                    index
                );
            }

            if (index < 0)
            {
                if (debugGrip)
                {
                    Debug.LogWarning(
                        "[" + side +
                        "] NO SPHERE SELECTED."
                    );
                }

                return;
            }

            if (leftGrab.selected == index ||
                rightGrab.selected == index)
            {
                if (debugGrip)
                {
                    Debug.LogWarning(
                        "[" + side +
                        "] Sphere already grabbed."
                    );
                }

                return;
            }

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

            Debug.Log(
                "[" + side + "] GRABBED: " +
                spheres[index].name
            );
        }

        // Move sphere on horizontal XZ plane
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

            // Keep the original Y coordinate
            targetLocal.y =
                localPositions[index].y;

            localPositions[index] =
                targetLocal;

            spheres[index].localPosition =
                targetLocal;
        }

        // Release sphere
        if (grip.WasReleasedThisFrame())
        {
            if (state.selected >= 0)
            {
                Debug.Log(
                    "[" + side + "] RELEASED: " +
                    spheres[state.selected].name
                );
            }

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

    // Run this from the component's context menu
    [ContextMenu("Diagnose XR Grip")]
    public void DiagnoseXRGrip()
    {
        Debug.Log(
            "========== XR GRIP DIAGNOSIS =========="
        );

        DiagnoseAction("LEFT", leftGrip.action);
        DiagnoseAction("RIGHT", rightGrip.action);
    }

    void DiagnoseAction(
        string side,
        InputAction action)
    {
        if (action == null)
        {
            Debug.LogError(
                "[" + side + "] Action is NULL"
            );
            return;
        }

        Debug.Log(
            "[" + side + "] Enabled: " +
            action.enabled
        );

        Debug.Log(
            "[" + side + "] Controls count: " +
            action.controls.Count
        );

        foreach (var binding in action.bindings)
        {
            Debug.Log(
                "[" + side + "] Binding: " +
                binding.effectivePath
            );
        }

        foreach (var control in action.controls)
        {
            Debug.Log(
                "[" + side + "] Control: " +
                control.path +
                " | Device: " +
                control.device.displayName
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
