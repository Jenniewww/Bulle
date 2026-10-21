using Fusion;
using UnityEngine;

namespace FusionIntroShared {
  /// <summary>
  /// Class for indicate the player active color.
  /// </summary>
  public class PlayerColor : NetworkBehaviour {
    
    // The MeshRender showing the player's current color
    public MeshRenderer ColorIndicatorMeshRenderer;
    
    // The networked color value for this player.
    [OnChangedRender(nameof(OnColorChanged))]
    [Networked] private Color _color { get; set; }
    

    public override void Spawned() {
      if (Object.HasStateAuthority) {
        _color = Color.white;
      } else {
        // Ensure late joiners will update the color on spawned.
        OnColorChanged();
      }
    }

    /// <summary>
    /// Set the player active color.
    /// </summary>
    public void SetColor(Color color) {
      _color = color;
    }

    /// <summary>
    /// Get the player active color.
    /// </summary>
    public Color GetColor() {
      return _color;
    }

    // This function is automatically called on any client whenever the _color Networked Property changes because of the OnChangedRender attribute.
    private void OnColorChanged() {
      ColorIndicatorMeshRenderer.material.color = _color;
    }
  }
}