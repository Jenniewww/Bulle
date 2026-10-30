
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class AnchorSupportCheck : MonoBehaviour
{
    [SerializeField]
    private ARAnchorManager anchorManager;

    private IEnumerator Start()
    {
        Debug.Log("[SSA] Starting Anchor Support Check...");

        if (anchorManager == null)
        {
            anchorManager = GetComponent<ARAnchorManager>();
        }

        if (anchorManager == null)
        {
            Debug.LogWarning(
                "[SSA] ARAnchorManager not found."
            );
            yield break;
        }

        yield return new WaitForSeconds(5f);

        Debug.Log(
            "[SSA] AR Session state: " + ARSession.state
        );

        for (int attempt = 1; attempt <= 10; attempt++)
        {
            XRAnchorSubsystem subsystem =
                anchorManager.subsystem;

            if (subsystem != null && subsystem.running)
            {
                Debug.Log(
                    "[SSA] XRAnchorSubsystem is RUNNING!"
                );

                yield break;
            }

            Debug.LogWarning(
                "[SSA] Attempt " + attempt +
                "/10: XRAnchorSubsystem not running."
            );

            yield return new WaitForSeconds(2f);
        }

        Debug.LogWarning(
            "[SSA] No running XRAnchorSubsystem. " +
            "Spatial anchors are unavailable in this configuration."
        );
    }
}
