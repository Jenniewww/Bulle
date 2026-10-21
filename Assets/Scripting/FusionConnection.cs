
using UnityEngine;
using Fusion;

public class FusionConnection : MonoBehaviour
{
    private NetworkRunner runner;

    private async void Start()
    {
        runner = gameObject.AddComponent<NetworkRunner>();

        Debug.Log("[FUSION] Connecting to BulleRoom...");

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = "BulleRoom",
            PlayerCount = 2
        });

        if (result.Ok)
        {
            Debug.Log("[FUSION] Connected to BulleRoom!");
        }
        else
        {
            Debug.LogError(
                "[FUSION] Connection failed: " +
                result.ShutdownReason
            );
        }
    }
}
