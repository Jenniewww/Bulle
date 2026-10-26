
using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class ControllerAcquisition : MonoBehaviour
{
    [SerializeField]
    private CoordinateManager coordinateManager;

    private InputDevice rightController;
    private bool wasPressed;

    private void Update()
    {
        // R閏up鑢e la manette droite si n閏essaire.
        if (!rightController.isValid)
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(
                XRNode.RightHand, devices);

            if (devices.Count > 0)
                rightController = devices[0];
        }

        if (!rightController.isValid)
            return;

        // D閠ecte la pression de la g鈉hette d'index.
        if (rightController.TryGetFeatureValue(
            CommonUsages.triggerButton, out bool isPressed))
        {
            if (isPressed && !wasPressed)
            {
                if (coordinateManager != null)
                    coordinateManager.CaptureAllAndExport();
                else
                    Debug.LogError(
                        "CoordinateManager non assign?");
            }

            wasPressed = isPressed;
        }
    }
}
