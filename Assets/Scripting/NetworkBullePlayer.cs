
using Fusion;
using UnityEngine;

public class NetworkBullePlayer : NetworkBehaviour
{
    [Header("Local XR Head")]
    public Transform localHead;

    [Header("Remote FourZones Prefab")]
    public GameObject remoteFourZonesPrefab;

    [Networked]
    public Vector3 HeadPosition { get; set; }

    [Networked]
    public Quaternion HeadRotation { get; set; }

    private GameObject remoteZones;

    public override void Spawned()
    {
        if (!Object.HasStateAuthority)
        {
            if (remoteFourZonesPrefab != null)
            {
                remoteZones = Instantiate(
                    remoteFourZonesPrefab
                );
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        if (localHead == null)
            return;

        HeadPosition = localHead.position;
        HeadRotation = localHead.rotation;
    }

    public override void Render()
    {
        if (Object.HasStateAuthority)
            return;

        if (remoteZones == null)
            return;

        remoteZones.transform.position =
            HeadPosition;

        remoteZones.transform.rotation =
            Quaternion.identity;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (remoteZones != null)
            Destroy(remoteZones);
    }
}
