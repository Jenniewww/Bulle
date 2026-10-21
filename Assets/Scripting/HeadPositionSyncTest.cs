
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class HeadPositionSyncTest : MonoBehaviour
{
    public Transform head;

    private bool registered = false;
    private float nextSendTime = 0f;

    void Update()
    {
        var nm = NetworkManager.Singleton;

        if (nm == null || !nm.IsListening)
        {
            registered = false;
            return;
        }

        // 等待手动启动 Host / Client 后注册
        if (!registered)
        {
            nm.CustomMessagingManager.RegisterNamedMessageHandler(
                "HeadPositionTest", OnMessage);

            registered = true;
            Debug.Log("[NETWORK] HeadPositionTest ready");
        }

        if (head == null || Time.time < nextSendTime)
            return;

        nextSendTime = Time.time + 1f;

        // PC2 向 PC1 发送头显位置
        if (nm.IsClient && !nm.IsServer && nm.IsConnectedClient)
        {
            using (var writer = new FastBufferWriter(
                128, Allocator.Temp))
            {
                writer.WriteValueSafe(head.position);

                nm.CustomMessagingManager.SendNamedMessage(
                    "HeadPositionTest",
                    NetworkManager.ServerClientId,
                    writer);
            }
        }
    }

    void OnMessage(ulong senderId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out Vector3 position);

        Debug.Log(
            $"[NETWORK] Received head from client {senderId}: {position}");
    }

    void OnDisable()
    {
        var nm = NetworkManager.Singleton;

        if (registered && nm != null &&
            nm.CustomMessagingManager != null)
        {
            nm.CustomMessagingManager.UnregisterNamedMessageHandler(
                "HeadPositionTest");
        }

        registered = false;
    }
}
