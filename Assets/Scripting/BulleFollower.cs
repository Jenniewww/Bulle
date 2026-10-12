
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class BulleFollower : MonoBehaviour
{
    [Header("Quest 3 Head")]
    public Transform playerHead;

    [Header("Ellipsoid diameters in meters")]
    public float width = 1.6f;   // X: left / right
    public float height = 1.8f;  // Y: up / down
    public float depth = 2.4f;   // Z: front / back

    [Header("Position")]
    public float verticalOffset = -0.6f;

    [Header("Controller")]
    public float resizeSpeed = 0.8f;
    public float minDiameter = 0.5f;
    public float maxDiameter = 4.0f;

    private InputDevice rightController;
    private InputDevice leftController;

    void Start()
    {
        FindPlayerHead();
        FindControllers();
    }

    void FindPlayerHead()
    {
        if (playerHead == null && Camera.main != null)
            playerHead = Camera.main.transform;

        if (playerHead == null)
            Debug.LogWarning(
                "Bulle: Player Head is missing!"
            );
    }

    void FindControllers()
    {
        var devices = new List<InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Right |
            InputDeviceCharacteristics.Controller,
            devices
        );

        if (devices.Count > 0)
            rightController = devices[0];

        devices.Clear();

        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Left |
            InputDeviceCharacteristics.Controller,
            devices
        );

        if (devices.Count > 0)
            leftController = devices[0];
    }

    void Update()
    {
        if (!rightController.isValid ||
            !leftController.isValid)
        {
            FindControllers();
        }

        // Right joystick: X changes width,
        // Y changes depth.
        if (rightController.TryGetFeatureValue(
            CommonUsages.primary2DAxis,
            out Vector2 rightStick))
        {
            width += rightStick.x * resizeSpeed * Time.deltaTime;
            depth += rightStick.y * resizeSpeed * Time.deltaTime;
        }

        // Left joystick Y: changes height.
        if (leftController.TryGetFeatureValue(
            CommonUsages.primary2DAxis,
            out Vector2 leftStick))
        {
            height += leftStick.y * resizeSpeed * Time.deltaTime;
        }

        // Keyboard controls for Editor testing
        if (Input.GetKey(KeyCode.RightArrow))
            width += resizeSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftArrow))
            width -= resizeSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.UpArrow))
            depth += resizeSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.DownArrow))
            depth -= resizeSpeed * Time.deltaTime;

        width = Mathf.Clamp(width, minDiameter, maxDiameter);
        height = Mathf.Clamp(height, minDiameter, maxDiameter);
        depth = Mathf.Clamp(depth, minDiameter, maxDiameter);
    }

    void LateUpdate()
    {
        if (playerHead == null)
        {
            FindPlayerHead();

            if (playerHead == null)
                return;
        }

        // Follow the real-world headset position
        Vector3 center = playerHead.position;
        center.y += verticalOffset;

        transform.position = center;

        // Unity's default sphere diameter = 1
        transform.localScale = new Vector3(
            width,
            height,
            depth
        );
    }
}
