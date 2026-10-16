
using UnityEngine;
using UnityEngine.InputSystem;

public class FourZonesVisibility : MonoBehaviour
{
    [Header("Four Zones")]
    public GameObject zoneIntime;
    public GameObject zonePersonnelle;
    public GameObject zoneSociale;
    public GameObject zonePublique;

    [Header("Quest Controller Buttons")]
    public InputActionProperty buttonX;
    public InputActionProperty buttonY;
    public InputActionProperty buttonA;
    public InputActionProperty buttonB;

    void OnEnable()
    {
        EnableAction(buttonX);
        EnableAction(buttonY);
        EnableAction(buttonA);
        EnableAction(buttonB);
    }

    void OnDisable()
    {
        DisableAction(buttonX);
        DisableAction(buttonY);
        DisableAction(buttonA);
        DisableAction(buttonB);
    }

    void Update()
    {
        if (Pressed(buttonX))
            Toggle(zoneIntime, "X - Intime");

        if (Pressed(buttonY))
            Toggle(zonePersonnelle, "Y - Personnelle");

        if (Pressed(buttonA))
            Toggle(zoneSociale, "A - Sociale");

        if (Pressed(buttonB))
            Toggle(zonePublique, "B - Publique");
    }

    bool Pressed(InputActionProperty property)
    {
        return property.action != null &&
               property.action.WasPressedThisFrame();
    }

    void Toggle(GameObject zone, string label)
    {
        if (zone == null)
            return;

        bool visible = !zone.activeSelf;
        zone.SetActive(visible);

        Debug.Log(label + " -> " +
                  (visible ? "VISIBLE" : "HIDDEN"));
    }

    void EnableAction(InputActionProperty property)
    {
        if (property.reference == null)
            property.action?.Enable();
    }

    void DisableAction(InputActionProperty property)
    {
        if (property.reference == null)
            property.action?.Disable();
    }
}
