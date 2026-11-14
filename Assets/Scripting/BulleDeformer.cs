
using UnityEngine;
using UnityEngine.InputSystem;

public class BulleDeformer : MonoBehaviour
{
    [Header("20 Control Points")]
    public Transform[] controlPoints = new Transform[20];

    [Header("XR Controllers")]
    public Transform leftController;
    public Transform rightController;

    [Header("Grip Actions")]
    public InputActionProperty leftGrip;
    public InputActionProperty rightGrip;

    [Header("Grab Settings")]
    public float maxRayDistance = 5f;
    public float selectionRadius = 0.14f;

    [Header("Debug")]
    public bool debugGrip = true;

    private class GrabState
    {
        public Transform selected;
        public float depth;
        public Vector3 offset;
    }

    private GrabState leftGrab = new GrabState();
    private GrabState rightGrab = new GrabState();

    private void OnValidate()
    {
        if (controlPoints == null)
            controlPoints = new Transform[20];

        if (controlPoints.Length != 20)
            System.Array.Resize(ref controlPoints, 20);
    }

    private void OnEnable()
    {
        EnableEmbeddedAction(leftGrip);
        EnableEmbeddedAction(rightGrip);
    }

    private void EnableEmbeddedAction(InputActionProperty property)
    {
        if (property.reference == null)
            property.action?.Enable();
    }

    private void Update()
    {
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

    private void HandleGrab(
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

        // Grip pressed
        if (grip.WasPressedThisFrame())
        {
            int index = FindControlPoint(ray);

            if (debugGrip)
                Debug.Log("[" + side + "] Selected: " + index);

            if (index < 0)
                return;

            Transform point = controlPoints[index];

            // Prevent both hands from grabbing the same point
            if (leftGrab.selected == point ||
                rightGrab.selected == point)
                return;

            state.selected = point;

            Vector3 pointWorld = point.position;

            state.depth = Vector3.Dot(
                pointWorld - ray.origin,
                ray.direction
            );

            state.offset =
                pointWorld - ray.GetPoint(state.depth);

            if (debugGrip)
                Debug.Log("[" + side + "] GRABBED: " + point.name);
        }

        // Grip held
        if (grip.IsPressed() && state.selected != null)
        {
            Vector3 targetWorld =
                ray.GetPoint(state.depth) + state.offset;

            state.selected.position = targetWorld;
        }

        // Grip released
        if (grip.WasReleasedThisFrame())
        {
            if (debugGrip && state.selected != null)
                Debug.Log("[" + side + "] RELEASED: " +
                          state.selected.name);

            state.selected = null;
        }
    }

    private int FindControlPoint(Ray ray)
    {
        int closest = -1;
        float bestDistance = selectionRadius;

        if (controlPoints == null)
            return -1;

        for (int i = 0; i < controlPoints.Length; i++)
        {
            Transform point = controlPoints[i];

            if (point == null ||
                !point.gameObject.activeInHierarchy)
                continue;

            if (leftGrab.selected == point ||
                rightGrab.selected == point)
                continue;

            Vector3 toPoint = point.position - ray.origin;

            float depth = Vector3.Dot(
                toPoint,
                ray.direction
            );

            if (depth < 0 || depth > maxRayDistance)
                continue;

            Vector3 projected = ray.GetPoint(depth);

            float distance = Vector3.Distance(
                projected,
                point.position
            );

            if (distance < bestDistance)
            {
                bestDistance = distance;
                closest = i;
            }
        }

        return closest;
    }

    // Compatibility method for BulleVisibility.cs
    public void SetBulleVisible(bool visible)
    {
        foreach (Transform point in controlPoints)
        {
            if (point != null)
                point.gameObject.SetActive(visible);
        }
    }

    [ContextMenu("Diagnose XR Grip")]
    public void DiagnoseXRGrip()
    {
        DiagnoseAction("LEFT", leftGrip.action);
        DiagnoseAction("RIGHT", rightGrip.action);
    }

    private void DiagnoseAction(string side, InputAction action)
    {
        if (action == null)
        {
            Debug.LogError("[" + side + "] Grip Action is NULL");
            return;
        }

        Debug.Log(
            "[" + side + "] Action: " +
            action.name +
            " | Enabled: " + action.enabled
        );

        foreach (var control in action.controls)
        {
            Debug.Log(
                "[" + side + "] Binding: " + control.path
            );
        }
    }

    private void OnDisable()
    {
        leftGrab.selected = null;
        rightGrab.selected = null;

        if (leftGrip.reference == null)
            leftGrip.action?.Disable();

        if (rightGrip.reference == null)
            rightGrip.action?.Disable();
    }
}
