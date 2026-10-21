using Fusion;
using UnityEngine;

namespace FusionIntroShared {
  
  /// <summary>
  /// Interface to indicate an object that the player can interact in the world.
  /// </summary>
  public interface IInteractable {
    public void Interact(NetworkObject interactingPlayer);
  }
}