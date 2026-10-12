
using UnityEngine;
using UnityEngine.InputSystem;

public class BulleControls : MonoBehaviour
{
    [Header("Bulle References")]
    public BulleFollower bulle;
    public Renderer bulleRenderer;

    [Header("Resize Settings")]
    public float resizeSpeed = 0.8f;

    private InputAction toggleAction;
    private InputAction gripAction;
    private InputAction rightStickAction;
    private InputAction leftStickAction;

    void Awake()
    {
        if (bulle == null)
            bulle = GetComponent<BulleFollower>();

        if (bulleRenderer == null)
            bulleRenderer = GetComponent<Renderer>();

        // Quest right controller A button
        toggleAction = new InputAction(
            "Toggle Bulle",
            InputActionType.Button,
            "<XRController>{RightHand}/primaryButton"
        );

        // Right controller Grip
        gripAction = new InputAction(
            "Edit Bulle",
            InputActionType.Button,
            "<XRController>{RightHand}/gripPressed"
        );

        // Right joystick
        rightStickAction = new InputAction(
            "Right Stick",
            InputActionType.Value,
            "<XRController>{RightHand}/thumbstick"
        );

        // Left joystick
        leftStickAction = new InputAction(
            "Left Stick",
            InputActionType.Value,
            "<XRController>{LeftHand}/thumbstick"
        );
    }

    void OnEnable()
    {
        toggleAction?.Enable();
        gripAction?.Enable();
        rightStickAction?.Enable();
        leftStickAction?.Enable();
    }

    void OnDisable()
    {
        toggleAction?.Disable();
        gripAction?.Disable();
        rightStickAction?.Disable();
        leftStickAction?.Disable();
    }

    void Update()
    {
        if (bulle == null || bulleRenderer == null)
            return;

        // A: show/hide Bulle
        if (toggleAction.WasPressedThisFrame())
        {
            bulleRenderer.enabled =
                !bulleRenderer.enabled;

            Debug.Log(
                "Bulle visible = " +
                bulleRenderer.enabled
            );
        }

        // Only resize while holding Grip
        if (!gripAction.IsPressed())
            return;

        Vector2 rightStick =
            rightStickAction.ReadValue<Vector2>();

        Vector2 leftStick =
            leftStickAction.ReadValue<Vector2>();

        float change = resizeSpeed * Time.deltaTime;

        // Right joystick
        bulle.width += rightStick.x * change;
        bulle.depth += rightStick.y * change;

        // Left joystick
        bulle.height += leftStick.y * change;

        // Keep valid sizes
        bulle.width = Mathf.Clamp(
            bulle.width,
            bulle.minDiameter,
            bulle.maxDiameter
        );

        bulle.height = Mathf.Clamp(
            bulle.height,
            bulle.minDiameter,
            bulle.maxDiameter
        );

        bulle.depth = Mathf.Clamp(
            bulle.depth,
            bulle.minDiameter,
            bulle.maxDiameter
        );
    }

    void OnDestroy()
    {
        toggleAction?.Dispose();
        gripAction?.Dispose();
        rightStickAction?.Dispose();
        leftStickAction?.Dispose();
    }
}
