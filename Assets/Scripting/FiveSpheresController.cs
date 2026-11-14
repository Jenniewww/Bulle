
using UnityEngine;
using UnityEngine.InputSystem;

public class FiveSpheresController : MonoBehaviour
{
    [Header("References")]
    public Transform head;

    // 四层五球，共20个现有控制球
    public Transform[] spheres = new Transform[20];

    [Header("XR Controllers")]
    public Transform leftController;
    public Transform rightController;

    [Header("Ray Visual References")]
    public ControllerRayVisual leftRayVisual;
    public ControllerRayVisual rightRayVisual;

    [Header("Grip Input")]
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
            property.action?.Enable();
    }

    void Start()
    {
        // 保留原版五球布局初始化
        // 20球模式不重新设置位置
        if (spheres != null && spheres.Length == 5)
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

                spheres[i].localPosition = localPositions[i];

                spheres[i].localScale =
                    Vector3.one * sphereDiameter;
            }
        }

        PrintActionInfo("LEFT", leftGrip.action);
        PrintActionInfo("RIGHT", rightGrip.action);
    }

    void Update()
    {
        HandleGrab(
            "LEFT",
            leftController,
            leftRayVisual,
            leftGrip.action,
            leftGrab
        );

        HandleGrab(
            "RIGHT",
            rightController,
            rightRayVisual,
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
            Vector3 headWorldPosition = head.position;

            Vector3 targetWorldPosition = new Vector3(
                headWorldPosition.x,
                headWorldPosition.y - heightBelowHead,
                headWorldPosition.z
            );

            transform.position = targetWorldPosition;
        }

        // 保留原版五球位置恢复逻辑
        if (spheres == null || spheres.Length != 5 ||
            localPositions == null)
            return;

        for (int i = 0; i < spheres.Length; i++)
        {
            if (spheres[i] == null)
                continue;

            if (leftGrab.selected == i ||
                rightGrab.selected == i)
                continue;

            spheres[i].localPosition = localPositions[i];
        }
    }

    void HandleGrab(
        string side,
        Transform controller,
        ControllerRayVisual rayVisual,
        InputAction grip,
        GrabState state)
    {
        if (controller == null ||
            grip == null ||
            !grip.enabled)
            return;

        // 与实际显示射线完全一致
        Ray ray = rayVisual != null
            ? rayVisual.GetRay()
            : new Ray(controller.position, controller.forward);

        if (grip.WasPressedThisFrame())
        {
            int index = FindSphereAlongRay(ray);

            if (debugGrip)
                Debug.Log("[" + side + "] Selected: " + index);

            if (index < 0)
                return;

            if (leftGrab.selected == index ||
                rightGrab.selected == index)
                return;

            state.selected = index;

            Vector3 sphereWorld = spheres[index].position;

            state.depth = Vector3.Dot(
                sphereWorld - ray.origin,
                ray.direction
            );

            state.offset =
                sphereWorld - ray.GetPoint(state.depth);
        }

        if (grip.IsPressed() && state.selected >= 0)
        {
            int index = state.selected;

            Vector3 targetWorld =
                ray.GetPoint(state.depth) + state.offset;

            spheres[index].position = targetWorld;

            // 同步原五球模式的位置缓存
            if (localPositions != null &&
                index < localPositions.Length)
            {
                localPositions[index] =
                    transform.InverseTransformPoint(targetWorld);
            }
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

        if (spheres == null)
            return -1;

        for (int i = 0; i < spheres.Length; i++)
        {
            if (spheres[i] == null)
                continue;

            if (leftGrab.selected == i ||
                rightGrab.selected == i)
                continue;

            Vector3 toSphere = spheres[i].position - ray.origin;

            float depth = Vector3.Dot(
                toSphere,
                ray.direction
            );

            if (depth < 0 || depth > maxRayDistance)
                continue;

            Vector3 projected = ray.GetPoint(depth);

            float distance = Vector3.Distance(
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
        if (!debugGrip || action == null || !action.enabled)
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
            Debug.LogWarning("[" + side + "] ACTION IS NULL");
            return;
        }

        Debug.Log("[" + side + "] Action: " +
            action.name + " | Enabled: " + action.enabled);
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
            Debug.LogError("[" + side + "] Action is NULL");
            return;
        }

        Debug.Log("[" + side + "] Enabled: " + action.enabled);

        foreach (var control in action.controls)
            Debug.Log("[" + side + "] Control: " + control.path);
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
