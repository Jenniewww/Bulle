using System;
using Fusion;
using UnityEngine;

namespace FusionIntroShared {
  /// <summary>
  /// Class responsible for displaying the requested color and checking if the provided colors are correct.
  /// </summary>
  public class ColorResult : NetworkBehaviour {
    // Reference to the objects that will be used to add the colors and the object that will show the resulting color
    [SerializeField] private ColorDeposit _colorProvider1;
    [SerializeField] private ColorDeposit _colorProvider2;
    [SerializeField] private Renderer _colorResultRenderer;
    [SerializeField ]private Color[] _availableColorResults;
    
    // Networked index of the active color merge.
    [OnChangedRender(nameof(OnColorRequestChanged))]
    [Networked] private int _currentColorRequestIndex { get; set; }

    public override void Spawned() {
      // Setup first color request as state authority.
      if (Object.HasStateAuthority) {
        SetupColorRequest();
      }else {
        // Ensure late joiners will update the color on spawned.
        OnColorRequestChanged();
      }
    }

    // Reset colors providers and increment the request index.
    private void SetupColorRequest() {
      _colorProvider1.ResetColor();
      _colorProvider2.ResetColor();
      _currentColorRequestIndex = ++_currentColorRequestIndex % _availableColorResults.Length;
    }

    public void CheckColorsMatch() {
      // Run only on state authority
      if (Object.HasStateAuthority == false) return;

      Color32 color1     = _colorProvider1.GetColor();
      Color32 color2     = _colorProvider2.GetColor();
      var     mixedColor = new Color32((byte)Math.Min(color1.r + color2.r, 255), (byte)Math.Min(color1.g + color2.g, 255), (byte)Math.Min(color1.b + color2.b, 255), 255);

      if (mixedColor == _availableColorResults[_currentColorRequestIndex]) {
        SetupColorRequest();
      }
    }
    
    // This function is automatically called on any client whenever the _currentColorRequestIndex Networked Property changes because of the OnChangedRender attribute.
    private void OnColorRequestChanged() {
      _colorResultRenderer.material.color = _availableColorResults[_currentColorRequestIndex];
    }
  }
}