namespace Fusion.Statistics {
#if FUSION_ENABLE_UGUI
  using UnityEngine;
  using UnityEngine.UI;

  public class RadialChart : MonoBehaviour {
    [SerializeField] protected Image _sourceImage;
    [SerializeField] protected Text _titleText;
    [SerializeField] protected Text _percentText;

    private int _fillShaderPropertyID = Shader.PropertyToID("_Fill");
    private float _value;
    private float _maxValue;
    private Material _material;

    /// <summary>
    /// Chart title.
    /// </summary>
    public string Title {
      get => _titleText?.text;
      set {
        if (_titleText) _titleText.text = value;
      }
    }

    /// <summary>
    /// Clamps <see cref="value"/> between 0 and 1 based on <see cref="totalValue"/>. Considers 0 as the base value.
    /// </summary>
    protected void SetShaderValues(float value, float totalValue) {
      float clamped;
      if (totalValue == 0) {
        clamped = 0;
      } else {
        clamped = Mathf.Clamp01(value / totalValue);
      }
      _material.SetFloat(_fillShaderPropertyID, clamped);
      _percentText.text = FusionStatsLookup.GetValueText(clamped * 100, FusionStatsLookup.LOOKUP_TABLE_0_PERCENT, "{0}%");
    }


    public void RefreshDisplay() {
      SetShaderValues(_value, _maxValue);
    }

    public void Setup(string title) {
      // create an instance for the material.
      _material             = new Material(_sourceImage.material);
      _sourceImage.material = _material;
      Title = title;
    }

    public void SetValue(float value, float maxValue) {
      _value    = value;
      _maxValue = maxValue;
    }
  }
#endif
}