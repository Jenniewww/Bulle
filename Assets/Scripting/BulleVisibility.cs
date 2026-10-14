
using UnityEngine;
using UnityEngine.InputSystem;

public class BulleVisibility : MonoBehaviour
{
    [Header("Target")]
    public BulleDeformer bulleDeformer;

    [Header("Quest Right Controller A Button")]
    public InputActionProperty aButton;

    private bool visible = true;

    void OnEnable()
    {
        aButton.action?.Enable(); //hello
    }

    void Update()
    {
        if (bulleDeformer == null ||
            aButton.action == null)
            return;

        if (aButton.action.WasPressedThisFrame())
        {
            visible = !visible;
            bulleDeformer.SetBulleVisible(visible);

            Debug.Log("Bulle visible: " + visible);
        }
    }
}
