using Fusion;
using UnityEngine;

namespace FusionIntroShared {
  
  /// <summary>
  /// Interactable for the player to pick up a color.
  /// </summary>
  public class ColorPicker : MonoBehaviour, IInteractable {

    public Color Color;

    private void Awake() {
      // Set the object material color as the color it will provide.
      GetComponent<Renderer>().material.color = Color;
    }

    public void Interact(NetworkObject interactingPlayer) {
      
      // Give the PlayerColor of the interacting player the color this ColorPicker provide.
      if (interactingPlayer.TryGetComponent<PlayerColor>(out var playerColor)) {
        playerColor.SetColor(Color);
      }
    }
  }

  public struct UncompressedFloat : INetworkStruct {
    public float Value;
  }
}