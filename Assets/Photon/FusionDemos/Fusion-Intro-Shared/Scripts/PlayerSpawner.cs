using Fusion;
using UnityEngine;

namespace FusionIntroShared {
  public class PlayerSpawner : NetworkBehaviour
  {
    public GameObject PlayerPrefab;

    public override void Spawned() {
      var randomPos = Random.onUnitSphere * 2;
      randomPos.y = 1;
      var obj = Runner.Spawn(PlayerPrefab, randomPos, Quaternion.identity);
      Runner.SetPlayerObject(Runner.LocalPlayer, obj);
    }
  }
}


