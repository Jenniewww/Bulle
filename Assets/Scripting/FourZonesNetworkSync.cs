
using Fusion;
using UnityEngine;

public class FourZonesNetworkSync : MonoBehaviour
{
    [SerializeField] private Transform localHead;
    [SerializeField] private GameObject fourZonesPrefab;

    private NetworkRunner runner;
    private NetworkObject myAvatar;
    private bool spawnRequested;

    private void Update()
    {
        if (spawnRequested)
            return;

        if (runner == null)
            runner = FindFirstObjectByType<NetworkRunner>();

        if (runner == null ||
            !runner.IsRunning ||
            !runner.IsConnectedToServer)
            return;

        if (localHead == null || fourZonesPrefab == null)
            return;

        NetworkObject prefabObject =
            fourZonesPrefab.GetComponent<NetworkObject>();

        if (prefabObject == null)
        {
            Debug.LogError("[BULLE] Missing Fusion NetworkObject");
            return;
        }

        spawnRequested = true;

        myAvatar = runner.Spawn(
            prefabObject,
            localHead.position,
            localHead.rotation
        );

        if (myAvatar == null)
        {
            Debug.LogError("[BULLE] Spawn failed");
            spawnRequested = false;
            return;
        }

        Debug.Log("[BULLE] Player avatar spawned");
    }

    private void OnDestroy()
    {
        if (runner != null &&
            runner.IsRunning &&
            myAvatar != null &&
            myAvatar.IsValid)
        {
            runner.Despawn(myAvatar);
        }
    }
}
