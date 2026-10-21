using Fusion;
using UnityEngine;

namespace FusionIntroShared {
  /// <summary>
  /// Interactable class for the object that will be colored by the player in shared mode.
  /// </summary>
  public class ColorDeposit : NetworkBehaviour, IInteractable {
    
    [SerializeField] private ColorResult _colorResult;
    
    // Networked color of this object.
    [OnChangedRender(nameof(OnColorChanged))]
    [Networked] private Color _color { get; set; }

    public override void Spawned() {
      if (Object.HasStateAuthority == false) {
        // Ensure late joiners will update the color on spawned.
        OnColorChanged();
      }
    }

    public void Interact(NetworkObject interactingPlayer) {
      // Instead of interacting with the deposit directly an RPC is used.
      // This is because the deposit is a MasterClientObject and only the Master client has Authority over changing its properties.
      RPC_Interact(interactingPlayer);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_Interact(NetworkObject interactingPlayer) {

      // Sometimes a NetworkObject referenced in an RPC is not present on the target client. This is a safety check for that
      if (interactingPlayer == null) {
        return;
      }

      // If able to get the player color behaviour, get the player color and set as this object color.
      // Also trigger the colors match check on the color manager.
      if (interactingPlayer.TryGetBehaviour<PlayerColor>(out var playerColor)) {
        var color = playerColor.GetColor();
        _color = color;
        _colorResult.CheckColorsMatch();
      }
    }

    /// <summary>
    /// Reset this object color to white.
    /// </summary>
    public void ResetColor() {
      _color = Color.white;
    }

    /// <summary>
    /// Get the current object color.
    /// </summary>
    public Color GetColor() {
      return _color;
    }
    // This function is automatically called on any client whenever the _color Networked Property changes because of the OnChangedRender attribute.
    private void OnColorChanged() {
      if (TryGetComponent<Renderer>(out var renderer)) {
        renderer.material.color = _color;
      }
    }
  }
}