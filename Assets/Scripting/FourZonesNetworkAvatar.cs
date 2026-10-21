
using Fusion;
using UnityEngine;

public class FourZonesNetworkAvatar : NetworkBehaviour
{
    [Networked] public Vector3 HeadPosition { get; set; }
    [Networked] public Quaternion HeadRotation { get; set; }

    private Transform localHead;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            if (Camera.main != null)
                localHead = Camera.main.transform;

            // Our original scene FourZones is already visible.
            // Hide this additional network copy locally.
            foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
                r.enabled = false;
        }

        Debug.Log("[BULLE] Avatar ready. Owner: " + HasStateAuthority);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (localHead == null && Camera.main != null)
            localHead = Camera.main.transform;

        if (localHead == null)
            return;

        HeadPosition = localHead.position;
        HeadRotation = localHead.rotation;
    }

    public override void Render()
    {
        if (HasStateAuthority)
            return;

        transform.SetPositionAndRotation(HeadPosition, HeadRotation);
    }
}
