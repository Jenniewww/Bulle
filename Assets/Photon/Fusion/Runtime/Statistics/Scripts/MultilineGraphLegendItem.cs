namespace Fusion.Statistics {
#if FUSION_ENABLE_UGUI
  using System;
  using UnityEngine;
  using UnityEngine.UI;

  public class MultilineGraphLegendItem : MonoBehaviour {
    [SerializeField] Image _swatch;
    [SerializeField] Image[] _dashedSwatch;
    [SerializeField] Text _label;

    Color _color;

    bool _visible = true;
    public Action<bool> OnToggled;

    public void UI_Clicked() {
      _visible = !_visible;

      OnToggled?.Invoke(_visible);

      UpdateColors();
    }

    public void Set(string label, Color color, bool usesDashedSwatch) {
      _color = color;
      var activeColor = GetColor();

      _label.text = label;

      _swatch.gameObject.SetActive(!usesDashedSwatch);
      foreach (var swatch in _dashedSwatch) {
        swatch.gameObject.SetActive(usesDashedSwatch);
      }

      UpdateColors();
    }

    void UpdateColors() {
      var color = GetColor();
      _label.color = color;
      _swatch.color = color;
      foreach (var swatch in _dashedSwatch) {
        swatch.color = color;
      }
    }

    Color GetColor() => _visible ? _color : Color.grey;
  }
#endif
}