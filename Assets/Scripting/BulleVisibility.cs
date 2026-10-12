
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class BulleVisibility : MonoBehaviour
{
    [Header("Bulle Renderer")]
    public Renderer bulleRenderer;

    private InputDevice rightController;
    private bool wasPressed = false;

    void Start()
    {
        if (bulleRenderer == null)
            bulleRenderer = GetComponent<Renderer>();

        FindController();
    }

    void FindController()
    {
        var devices = new List<InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Right |
            InputDeviceCharacteristics.Controller,
            devices
        );

        if (devices.Count > 0)
            rightController = devices[0];
    }

    void Update()
    {
        if (!rightController.isValid)
        {
            wasPressed = false;
            FindController();
            return;
        }

        bool pressed = false;

        if (rightController.TryGetFeatureValue(
            CommonUsages.primaryButton,
            out pressed))
        {
            // Only toggle once per button press
            if (pressed && !wasPressed)
            {
                ToggleBulle();
            }

            wasPressed = pressed;
        }
    }

    void ToggleBulle()
    {
        if (bulleRenderer == null)
            return;

        bulleRenderer.enabled = !bulleRenderer.enabled;

        Debug.Log(
            "Bulle visible: " + bulleRenderer.enabled
        );
    }
}
