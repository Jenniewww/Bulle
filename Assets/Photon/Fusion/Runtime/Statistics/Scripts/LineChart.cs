namespace Fusion.Statistics {
#if FUSION_ENABLE_UGUI
  using UnityEngine;
  using UnityEngine.EventSystems;
  using UnityEngine.UI;

  public class LineChart : MonoBehaviour, IPointerClickHandler {
    [SerializeField] protected float _threshold;
    [SerializeField] protected RawImage _sourceImage;
    [SerializeField] protected Text _titleText;
    [SerializeField] protected Text _peakValueText;
    [SerializeField] protected Text _avgValueText;
    [SerializeField] protected Text _lastValueText;

    protected StatAccumulator _accumulator;

    private string _originalTitle;
    private string _labelFormat;
    private bool _forcePerUpdate;
    private float _lookUpTableMultiplier;

    /// <summary>
    /// Chart title.
    /// </summary>
    public string Title {
      set {
        if (_titleText) _titleText.text = value;
      }
    }


    private readonly int _valuesShaderPropertyID = Shader.PropertyToID("_Values");
    private readonly int _valueMinShaderPropertyID = Shader.PropertyToID("_ValueMin");
    private readonly int _valueMaxShaderPropertyID = Shader.PropertyToID("_ValueMax");
    private readonly int _thresholdShaderPropertyID = Shader.PropertyToID("_Threshold");
    private readonly int _baseBottomColorShaderPropertyID = Shader.PropertyToID("_BottomColor");
    private readonly int _baseTopColorShaderPropertyID = Shader.PropertyToID("_TopColor");
    private readonly int _thresholdBottomColorShaderPropertyID = Shader.PropertyToID("_ThresholdBottomColor");
    private readonly int _thresholdTopColorShaderPropertyID = Shader.PropertyToID("_ThresholdTopColor");
    private readonly int _zeroIsTransparentShaderPropertyId = Shader.PropertyToID("_ZeroIsTransparent");

    // The shader has the same property, both should match.
    const int BUFFER_SAMPLES = 180;
    private float[] _values;
    private float[] _valuesToDispatch;
    private int _headIndex;
    private Material _material;
    private string[][] _lookupTable;
    private float _lastNonZeroValue;

    /// <summary>
    /// Set threshold value.
    /// </summary>
    public void SetThreshold(float threshold) {
      threshold = _accumulator.DisplayingPerSecond ? threshold * FusionStatistics.EstimateFusionAfterUpdatesPerSecond : threshold;
      _material.SetFloat(_thresholdShaderPropertyID, threshold);
    }

    /// <summary>
    /// Configure shader colors.
    /// </summary>
    public void SetColors(Gradient defaultGradient, Gradient thresholdGradient, bool zeroIsTransparent) {
      _material.SetColor(_baseBottomColorShaderPropertyID, defaultGradient.Evaluate(0));
      _material.SetColor(_baseTopColorShaderPropertyID, defaultGradient.Evaluate(1));

      _material.SetColor(_thresholdBottomColorShaderPropertyID, thresholdGradient.Evaluate(0));
      _material.SetColor(_thresholdTopColorShaderPropertyID, thresholdGradient.Evaluate(1));

      _material.SetInteger(_zeroIsTransparentShaderPropertyId, zeroIsTransparent ? 1 : 0);
    }

    /// <summary>
    /// Clear all values.
    /// </summary>
    public void Clear() {
      for (int i = 0; i < _values.Length; i++) {
        _values[i] = 0;
      }
    }

    /// <summary>
    /// Fills the data dispatch array with the values in the order they were added, calculate min and max and sets on the shader.
    /// </summary>
    public void DispatchValuesToShader() {
      var minValue = _values[_headIndex];
      var maxValue = _values[_headIndex];
      var avg = 0f;
      bool notAllZeros = false;
      for (int i = (_headIndex + 1) % BUFFER_SAMPLES, k = 0; k < BUFFER_SAMPLES; k++) {
        _valuesToDispatch[k] =  _values[i];
        avg                  += _valuesToDispatch[k];
        notAllZeros |= _valuesToDispatch[k] != 0;
        if (_values[i] < minValue) minValue = _values[i];
        if (_values[i] > maxValue) maxValue = _values[i];
        i = (i + 1) % BUFFER_SAMPLES;
      }

      avg /= BUFFER_SAMPLES;
      if (_lookupTable != FusionStatsLookup.LOOKUP_TABLE_0_00ms && avg > 0 && avg < 1) avg = 1; // Avoid 0 on data without decimal places

      var config = FusionStatistics.Config;
      var lastValue = _valuesToDispatch[^1];
      if (lastValue != 0) {
        _lastNonZeroValue = lastValue;
      }
      _lastNonZeroValue   = notAllZeros ? _lastNonZeroValue : 0;
      _peakValueText.text = FusionStatsLookup.GetValueText(maxValue, _lookupTable, _labelFormat, multiplierToMatchTable: _lookUpTableMultiplier);
      _lastValueText.text = FusionStatsLookup.GetValueText(lastValue == 0 && config.DontDisplayZeroOnLastValue ? _lastNonZeroValue : lastValue, _lookupTable, _labelFormat, multiplierToMatchTable: _lookUpTableMultiplier);
      _avgValueText.text  = FusionStatsLookup.GetValueText(avg, _lookupTable, _labelFormat, multiplierToMatchTable: _lookUpTableMultiplier);

      // set values array
      _material.SetFloatArray(_valuesShaderPropertyID, _valuesToDispatch);

      // If a previous min/max value exists, pass a value in between the previous and current to the shader to smoothly change the chart range.
      minValue = _material.HasFloat(_valueMinShaderPropertyID) ? (_material.GetFloat(_valueMinShaderPropertyID) + minValue) * .5f : minValue;
      maxValue = _material.HasFloat(_valueMaxShaderPropertyID) ? (_material.GetFloat(_valueMaxShaderPropertyID) + maxValue) * .5f : maxValue;
      _material.SetFloat(_valueMinShaderPropertyID, minValue);
      _material.SetFloat(_valueMaxShaderPropertyID, maxValue);
    }

    /// <summary>
    /// Update thresholds and send values to shared to be rendered.
    /// </summary>
    public void RefreshDisplay() {
      SetThreshold(_threshold);
      DispatchValuesToShader();
    }

    /// <summary>
    /// Setups LineChart data.
    /// </summary>
    /// <param name="title">Title to be displayed on the chart</param>
    /// <param name="lookupTable">Define the lookup table to prevent the creation of new strings for cached values. Lookup tables can be found at <see cref="FusionStatsLookup"/>.</param>
    /// <param name="labelFormat">This is the fallback format for displaying the string value as text on the chart. If the value to be displayed is not present in the lookup table, this string formatting will be used to create it.</param>
    /// <param name="forcePerUpdate">Force the chart to refresh with each update, not allowing it to change to refresh per second.</param>
    /// <param name="lookUpTableMultiplier">Multiplier to match the numeric value into the correct column and line of the lookup table.</param>
    public void Setup(string title, string[][] lookupTable, string labelFormat = "{0}", bool forcePerUpdate = false, float lookUpTableMultiplier = 1) {
      _material             = new Material(_sourceImage.material);
      _sourceImage.material = _material;
      _values               = new float[BUFFER_SAMPLES];
      _valuesToDispatch     = new float[BUFFER_SAMPLES];
      _headIndex            = 0;

      _forcePerUpdate = forcePerUpdate;
      _originalTitle  = title;
      Title           = $"{_originalTitle} {(_accumulator.DisplayingPerSecond ? "(S)" : "(U)")}";
      _labelFormat    = labelFormat;
      _lookupTable    = lookupTable;
      _lookUpTableMultiplier = lookUpTableMultiplier;
      SetThreshold(_threshold);
      var config = FusionStatistics.Config;
      SetColors(config.DefaultGradient, config.ThresholdGradient, config.RenderZeroAsTransparent);
    }

    /// <summary>
    /// Add a new value to the buffer.
    /// </summary>
    public void AddValue(float value) {
      var lastTime = _accumulator.LastTimeStamp;
      _accumulator.Accumulate(value);

      // not yet accumulated 1 second of new data
      if (_accumulator.DisplayingPerSecond && Mathf.Approximately(_accumulator.LastTimeStamp, lastTime)) return;

      var accumulatedValue = _accumulator.DisplayingPerSecond ? _accumulator.ValuePerSecond : _accumulator.Value;
      _headIndex          = (_headIndex + 1) % BUFFER_SAMPLES;
      _values[_headIndex] = accumulatedValue;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
      if (_forcePerUpdate) {
        _accumulator.DisplayingPerSecond = false;
      } else {
        _accumulator.DisplayingPerSecond = !_accumulator.DisplayingPerSecond;
      }

      Title = $"{_originalTitle} {(_accumulator.DisplayingPerSecond ? "(S)" : "(U)")}";
    }
  }
#endif
}