
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class AnchorSupportCheck : MonoBehaviour
{
    IEnumerator Start()
    {
        // 等待 AR Session 初始化
        yield return new WaitForSeconds(3f);

        Debug.Log("[AnchorTest] AR Session state: " + ARSession.state);

        var manager = GetComponent<ARAnchorManager>();

        if (manager == null)
        {
            Debug.LogError("[AnchorTest] ARAnchorManager not found.");
            yield break;
        }

        var subsystem = manager.subsystem;

        if (subsystem == null)
        {
            Debug.LogWarning(
                "[AnchorTest] No XRAnchorSubsystem available. " +
                "Current runtime may not support AR anchors."
            );
            yield break;
        }

        Debug.Log("[AnchorTest] XRAnchorSubsystem is running: "
                  + subsystem.running);

        Debug.Log("[AnchorTest] Provider: "
                  + subsystem.subsystemDescriptor.id);
    }
}
