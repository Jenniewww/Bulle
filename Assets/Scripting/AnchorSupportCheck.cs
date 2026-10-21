
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR.Features.Meta;

public class AnchorSupportCheck : MonoBehaviour
{
    [SerializeField] private ARAnchorManager anchorManager;

    private IEnumerator Start()
    {
        Debug.Log("[SSA] Starting Shared Spatial Anchors check...");

        if (anchorManager == null)
            anchorManager = GetComponent<ARAnchorManager>();

        if (anchorManager == null)
        {
            Debug.LogError("[SSA] ARAnchorManager not found!");
            yield break;
        }

        // Wait for XR and AR Session initialization.
        yield return new WaitForSeconds(5f);

        Debug.Log("[SSA] AR Session state: " + ARSession.state);

        // Retry because the subsystem may initialize later.
        for (int attempt = 1; attempt <= 10; attempt++)
        {
            XRAnchorSubsystem subsystem = anchorManager.subsystem;

            if (subsystem != null && subsystem.running)
            {
                Debug.Log("[SSA] XRAnchorSubsystem is RUNNING!");

                MetaOpenXRAnchorSubsystem metaSubsystem =
                    subsystem as MetaOpenXRAnchorSubsystem;

                if (metaSubsystem != null)
                {
                    Debug.Log(
                        "[SSA] Shared Anchors support: " +
                        metaSubsystem.isSharedAnchorsSupported
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "[SSA] Anchor subsystem is not MetaOpenXRAnchorSubsystem."
                    );
                }

                yield break;
            }

            Debug.LogWarning(
                "[SSA] Attempt " + attempt +
                "/10: XRAnchorSubsystem not running."
            );

            yield return new WaitForSeconds(2f);
        }

        Debug.LogError(
            "[SSA] FAILED: No running XRAnchorSubsystem after retries."
        );
    }
}
