
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ControllerRayVisual : MonoBehaviour
{
    [Header("Ray Settings")]
    [SerializeField] private float rayLength = 5f;
    [SerializeField] private float startWidth = 0.008f;
    [SerializeField] private float endWidth = 0.008f;

    [Header("Direction")]
    [SerializeField] private Axis directionAxis = Axis.Y;

    [Header("Controller")]
    [SerializeField] private Transform controller;

    public enum Axis
    {
        X,
        Y,
        Z
    }

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        if (controller == null)
            controller = transform;

        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
    }

    private void Update()
    {
        if (controller == null || lineRenderer == null)
            return;

        Vector3 origin = controller.position;
        Vector3 direction = GetRayDirection();

        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(
            1,
            origin + direction * rayLength
        );
    }

    public Vector3 GetRayDirection()
    {
        if (controller == null)
            return -transform.up;

        Vector3 direction;

        switch (directionAxis)
        {
            case Axis.X:
                direction = controller.right;
                break;

            case Axis.Y:
                direction = controller.up;
                break;

            default:
                direction = controller.forward;
                break;
        }

        // 强制反转方向
        return -direction.normalized;
    }

    public Ray GetRay()
    {
        return new Ray(
            controller.position,
            GetRayDirection()
        );
    }
}
