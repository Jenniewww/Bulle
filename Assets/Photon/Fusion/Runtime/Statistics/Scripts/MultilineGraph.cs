namespace Fusion.Statistics {
#if FUSION_ENABLE_UGUI
  using System;
  using UnityEngine;
  using UnityEngine.UI;


  /// <summary>
  /// Used for controlling a Multiline graph in unity UI
  /// using the MultilineGraph shader
  /// </summary>
  [RequireComponent(typeof(RawImage))]
  public class MultilineGraph : MonoBehaviour {


    /// <summary>
    /// Can be used to adjust the behaviour of the range of a graph
    /// </summary>
    public enum RangeType {
      /// <summary>
      /// Will use the default range settings provided by
      /// <see cref="_defaultMin"/> and <see cref="_defaultMax"/>
      /// </summary>
      DefaultOnly,
      /// <summary>
      /// Will use the default range settings provided by
      /// <see cref="_defaultMin"/> and <see cref="_defaultMax"/>
      /// as the lower bounds for the range, it will scale the range up and down
      /// outside of these values to match the current min, max value in its domain.
      /// </summary>
      Adaptive,

      /// <summary>
      /// Will use the default range settings provided by
      /// <see cref="_defaultMin"/> and <see cref="_defaultMax"/>
      /// as the lower bounds for the range, it will scale the range up outside
      /// of these values, but will not scale the range back down. hence it gets
      /// stuck at the largest domain value ever received.
      /// </summary>
      Sticky
    }

    [Header("Shader")]
    [SerializeField] Color _backgroundColor = Color.black;
    [SerializeField, Range(0.01f, 0.1f)] float _lineWidth = 0.01f;
    [SerializeField, Range(0.01f, 0.1f)] float _thresholdWidth = 0.01f;

    /// <summary>
    /// We bake line value data into a texture, this controls whether
    /// the texture stores data at full or half precision
    /// <see cref="TextureFormat.RHalf"/> vs <see cref="TextureFormat.RFloat"/>
    ///
    /// Half precision should be prefered where possible to avoid using up
    /// to much GPU memory
    /// </summary>
    [SerializeField] bool _useFullPrecision = false;

    [Header("Range Settings")]
    [SerializeField] float _defaultMin = 0f;
    [SerializeField] float _defaultMax = 1f;
    [SerializeField] RangeType _rangeType = RangeType.Adaptive;
    [SerializeField] float _rangePadding = 0.1f;


    [Header("UI")]
    [SerializeField] Text HeaderText;
    [SerializeField] Text RangeUpper;
    [SerializeField] Text RangeLower;
    [SerializeField] GameObject _legendItemPrefab;
    [SerializeField] VerticalLayoutGroup _linesLegendHolder;
    [SerializeField] VerticalLayoutGroup _thresholdLegendHolder;

    Material _material;
    MultilineGraphData _graphData;
    RawImage _rawImage;

    void Awake() {
      _rawImage = GetComponent<RawImage>();

      _material = new Material(_rawImage.material);
      _material.SetColor("_BackgroundColor", _backgroundColor);
      _material.SetFloat("_LineWidth", _lineWidth);
      _material.SetFloat("_ThresholdWidth", _thresholdWidth);
      _rawImage.material = _material;

      _graphData = new MultilineGraphData(_useFullPrecision);
      _graphData.SetRange(_defaultMin, _defaultMax, _rangeType, _rangePadding);

      if(HeaderText != null && HeaderText.text.Length == 0) {
          HeaderText.gameObject.SetActive(false);
      }
    }

    void OnDestroy() {
      _graphData?.Dispose();

      if (_material != null) {
        Destroy(_material);
      }
    }

    /// <summary>
    /// Add a line to the graph.
    /// Note the graphs support upto <see cref="MultilineGraphData.MaxLines"/>
    /// when trying to add more than this -1 will be returned
    /// </summary>
    /// <param name="color">Line color</param>
    /// <param name="label">Label for legend display</param>
    /// <returns>Line index, or -1 if at max capacity</returns>
    public int AddLine(Color color, string label = null) {
      int index = _graphData.AddLine();

      if(index < 0) {
        return index;
      }

      _graphData.SetLineColor(index, color);

      // Update legend if present
      if (_linesLegendHolder != null && _legendItemPrefab != null) {
        var go = Instantiate(_legendItemPrefab, _linesLegendHolder.transform);
        var legend = go.GetComponent<MultilineGraphLegendItem>();
        legend.Set(label, color, false);
        legend.OnToggled = (visible) => _graphData.SetLineVisible(index, visible);
      }

      return index;
    }

    /// <summary>
    /// Add a threshold to the graph.
    ///
    /// Note the graphs support upto <see cref="MultilineGraphData.MaxThresholds"/>
    /// when trying to add more than this -1 will be returned
    /// </summary>
    /// <param name="value">Initial threshold value</param>
    /// <param name="color">Threshold color</param>
    /// <param name="label">Label for legend display</param>
    /// <returns>Threshold index, or -1 if at max capacity</returns>
    public int AddThreshold(float value, Color color, string label) {
      int index = _graphData.AddThreshold();

      if (index < 0) {
        return index;
      }
      _graphData.SetThreshold(index, value, color);

      // Update legend if present
      if (_thresholdLegendHolder != null && _legendItemPrefab != null) {
        var go = Instantiate(_legendItemPrefab, _thresholdLegendHolder.transform);
        var legend = go.GetComponent<MultilineGraphLegendItem>();
        legend.Set($"{label}{value}", color, true);
        legend.OnToggled = (visible) => _graphData.SetThresholdVisible(index, visible);
      }

      return index;
    }

    /// <summary>
    /// Set the visibility of a line
    /// </summary>
    public void SetLineVisible(int index, bool visible) {
      _graphData.SetLineVisible(index, visible);
    }

    /// <summary>
    /// Set the visibility of a threshold
    /// </summary>
    public void SetThresholdVisible(int index, bool visible) {
      _graphData.SetThresholdVisible(index, visible);
    }

    /// <summary>
    /// Adds a value to a line
    /// </summary>
    /// <param name="lineIndex">The line we want to add the value for</param>
    /// <param name="value">The value we want to set</param>
    /// <returns>True if the value was added, a False indicates that the line with the corresponding index has not been configured</returns>
    public bool AddValue(int lineIndex, float value) {
      return _graphData.AddValue(lineIndex, value);
    }


    /// <summary>
    /// Sets a threshold to a value
    /// </summary>
    /// <param name="index">The threshold we want to set to a value we want to add the value for</param>
    /// <param name="value">The value we want to set</param>
    /// <returns>True if the value was set, a False indicates that the threshold with the corresponding index has not been configured</returns>
    public bool SetThresholdValue(int index, float value) {
      return _graphData.SetThresholdValue(index, value);
    }

    /// <summary>
    /// Submitts data to thet shader and updates the visualisatiton
    /// </summary>
    public void RefreshDisplay() {
      _graphData.Apply();
      _graphData.ApplyToMaterial(_material);

      if (RangeUpper != null) {
        RangeUpper.text = _graphData.MaxValue.ToString("G4");
      }

      if (RangeLower != null) {
        RangeLower.text = _graphData.MinValue.ToString("G4");
      }
    }
  }

  /// <summary>
  /// Controls the MultineGraph Shader
  /// </summary>
  public class MultilineGraphData : System.IDisposable {
    private readonly int _dataTextureShaderPropertyID = Shader.PropertyToID("_DataTex");
    private readonly int _thresholdsShaderPropertyID = Shader.PropertyToID("_Thresholds");
    private readonly int _maxSamplesShaderPropertyID = Shader.PropertyToID("_MaxSamples");
    private readonly int _lineCountShaderPropertyID = Shader.PropertyToID("_LineCount");
    private readonly int _thresholdCountShaderPropertyID = Shader.PropertyToID("_ThresholdCount");
    private readonly int _minValueShaderPropertyID = Shader.PropertyToID("_MinValue");
    private readonly int _maxValueShaderPropertyID = Shader.PropertyToID("_MaxValue");
    private readonly int _lineColorsShaderPropertyID = Shader.PropertyToID("_LineColors");
    private readonly int _thresholdColorsShaderPropertyID = Shader.PropertyToID("_ThresholdColors");
    private readonly int _writeIndicesShaderPropertyID = Shader.PropertyToID("_WriteIndices");
    private readonly int _sampleCountsShaderPropertyID = Shader.PropertyToID("_SampleCounts");
    private readonly int _lineVisibleShaderPropertyID = Shader.PropertyToID("_LineVisible");
    private readonly int _thresholdVisibleShaderPropertyID = Shader.PropertyToID("_ThresholdVisible");

    // SOME OF THESE VALUES ARE MIRRORED IN THE SHADER
    // IF THEY ARE CHANGED HERE, THEY MUST BE UPDATED IN THE SHADER

    /// <summary>
    /// Samples are stored in a circular buffer
    /// This is the size of that buffer
    /// </summary>
    public const int MaxSamples = 512;

    /// <summary>
    /// The Maximum number of lines
    /// </summary>
    public const int MaxLines = 4;

    /// <summary>
    /// The Maximum number of thresholds
    /// </summary>
    public const int MaxThresholds = 4;

    Texture2D _dataTexture;
    Color[] _dataPixels;

    readonly int[] _writeIndices = new int[MaxLines];
    readonly int[] _sampleCounts = new int[MaxLines];

    readonly float[] _writeIndicesFloat = new float[MaxLines];
    readonly float[] _sampleCountsFloat = new float[MaxLines];

    readonly Color[] _lineColors = new Color[MaxLines];
    readonly float[] _thresholds = new float[MaxThresholds];
    readonly Color[] _thresholdColors = new Color[MaxThresholds];
    readonly float[] _lineVisible = new float[MaxLines] { 1f, 1f, 1f, 1f };
    readonly float[] _thresholdVisible = new float[MaxThresholds] { 1f, 1f, 1f, 1f };

    int _lineCount;
    int _thresholdCount;
    float _minValue;
    float _maxValue;

    // Adaptive range settings
    float _defaultMinValue = 0f;
    float _defaultMaxValue = 1f;
    MultilineGraph.RangeType _rangeType = MultilineGraph.RangeType.Adaptive;
    float _rangePadding = 0.1f;

    // Track min/max incrementally to avoid full scan on Apply()
    float _dataMin = float.MaxValue;
    float _dataMax = float.MinValue;
    bool _rangeDirty = true;

    bool _dataDirty;

    /// <summary>
    /// The number of active lines being drawn on this graph
    /// </summary>
    public int LineCount => _lineCount;

    /// <summary>
    /// The number of active thresholds being drawan on this graph
    /// </summary>
    public int ThresholdCount => _thresholdCount;

    /// <summary>
    /// The current MinValue used for the display range of the graph
    /// </summary>
    public float MinValue => _minValue;

    /// <summary>
    /// The current MaxValue used for the display range of the graph
    /// </summary>
    public float MaxValue => _maxValue;

    /// <summary>
    /// </summary>
    public MultilineGraphData(bool useFullPrecision) {
      _dataTexture = new Texture2D(MaxSamples, MaxLines, useFullPrecision ? TextureFormat.RFloat : TextureFormat.RHalf, false);
      _dataTexture.filterMode = FilterMode.Point;
      _dataTexture.wrapMode = TextureWrapMode.Clamp;
      _dataPixels = new Color[MaxSamples * MaxLines];
    }

    /// <inheritdoc/>
    public void Dispose() {
      if (_dataTexture != null) {
        UnityEngine.Object.Destroy(_dataTexture);
        _dataTexture = null;
      }
    }

    /// <summary>
    /// Adds a line to the graph and returns its index
    /// for setting values
    /// </summary>
    /// <returns>
    /// The index, which can be used for setting values
    /// returns - 1 if we have reached the hard limit for the number of lines
    /// </returns>
    public int AddLine() {
      if (_lineCount == MaxLines) {
        return -1;
      }

      _lineCount++;
      return _lineCount - 1;
    }

    /// <summary>
    /// Adds a threshold to the graph and returns its index
    /// for setting values
    /// </summary>
    /// <returns>
    /// The index, which can be used for setting values
    /// returns - 1 if we have reached the hard limit for the number of lines
    /// </returns>
    public int AddThreshold() {
      if (_thresholdCount == MaxThresholds) {
        return -1;
      }

      _thresholdCount++;
      return _thresholdCount - 1;
    }

    /// <summary>
    /// Sets the colour of a line at a given index
    /// </summary>
    /// <param name="index"></param>
    /// <param name="color"></param>
    public void SetLineColor(int index, Color color) {
      if (index >= 0 && index < _lineCount) {
        _lineColors[index] = color;
      }
    }

    /// <summary>
    /// Set the visibility of a line
    /// </summary>
    public void SetLineVisible(int index, bool visible) {
      if (index >= 0 && index < _lineCount) {
        _lineVisible[index] = visible ? 1f : 0f;
        _rangeDirty = true;
        _dataDirty = true;
      }
    }

    /// <summary>
    /// Set the visibility of a threshold
    /// </summary>
    public void SetThresholdVisible(int index, bool visible) {
      if (index >= 0 && index < _thresholdCount) {
        _thresholdVisible[index] = visible ? 1f : 0f;
        _rangeDirty = true;
        _dataDirty = true;
      }
    }

    /// <summary>
    /// Set the value and a colour of a threshold at a given index
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    /// <param name="color"></param>
    public void SetThreshold(int index, float value, Color color) {
      if (index < 0 || index >= _thresholdCount) return;
      _thresholds[index] = value;
      _thresholdColors[index] = color;
      _rangeDirty = true;
    }

    /// <summary>
    /// Sets the value of a threshold
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetThresholdValue(int index, float value) {
      if (index < 0 || index >= _thresholdCount) return false;
      _thresholds[index] = value;
      _rangeDirty = true;
      return true;
    }

    /// <summary>
    /// Configure the default range and adaptive behavior
    /// </summary>
    /// <param name="minValue">Default minimum value</param>
    /// <param name="maxValue">Default maximum value</param>
    /// <param name="rangeType">Controls how the range adapts</param>
    /// <param name="padding">Padding factor when adapting (0.1 = 10%)</param>
    public void SetRange(float minValue, float maxValue, MultilineGraph.RangeType rangeType = MultilineGraph.RangeType.Adaptive, float padding = 0.1f) {
      _defaultMinValue = _minValue = minValue;
      _defaultMaxValue = _maxValue = maxValue;
      _rangeType = rangeType;
      _rangePadding = padding;
      _rangeDirty = true;
    }

    /// <summary>
    /// Add a single value to a line's ring buffer
    /// </summary>
    public bool AddValue(int lineIndex, float value) {
      if (lineIndex < 0 || lineIndex >= _lineCount) return false;

      int writeIndex = _writeIndices[lineIndex];
      int pixelIndex = lineIndex * MaxSamples + writeIndex;

      // Track if we're overwriting a value that was at the boundary
      float oldValue = _dataPixels[pixelIndex].r;
      bool wasAtBoundary = _sampleCounts[lineIndex] == MaxSamples &&
                           (oldValue == _dataMin || oldValue == _dataMax);

      _dataPixels[pixelIndex].r = value;
      _writeIndices[lineIndex] = (writeIndex + 1) % MaxSamples;
      _sampleCounts[lineIndex] = Mathf.Min(_sampleCounts[lineIndex] + 1, MaxSamples);

      // Update range tracking
      if (!float.IsNaN(value) && !float.IsInfinity(value)) {
        if (value < _dataMin) _dataMin = value;
        if (value > _dataMax) _dataMax = value;

        // If we overwrote a boundary value, need full rescan
        if (wasAtBoundary) _rangeDirty = true;
      }

      _dataDirty = true;
      return true;
    }

    /// <summary>
    /// Recalculate data min/max by scanning all values.
    /// Only called when necessary (boundary value overwritten).
    /// </summary>
    void RecalculateDataRange() {
      _dataMin = float.MaxValue;
      _dataMax = float.MinValue;

      for (int lineIdx = 0; lineIdx < _lineCount; lineIdx++) {
        // ignore invisible lines
        if (_lineVisible[lineIdx] < 0.5f) continue;
        int count = _sampleCounts[lineIdx];
        int rowOffset = lineIdx * MaxSamples;

        for (int i = 0; i < count; i++) {
          float value = _dataPixels[rowOffset + i].r;
          if (!float.IsNaN(value) && !float.IsInfinity(value)) {
            if (value < _dataMin) _dataMin = value;
            if (value > _dataMax) _dataMax = value;
          }
        }
      }

      // Include thresholds
      for (int i = 0; i < _thresholdCount; i++) {
        //ignore invisible thresholds
        if (_thresholdVisible[i] < 0.5f) continue;

        float threshValue = _thresholds[i];
        if (threshValue < _dataMin) _dataMin = threshValue;
        if (threshValue > _dataMax) _dataMax = threshValue;
      }

      _rangeDirty = false;
    }

    /// <summary>
    /// Apply changes to texture and calculate range.
    /// </summary>
    public void Apply() {
      if (!_dataDirty) return;

      // Recalculate range if needed
      if (_rangeDirty) {
        RecalculateDataRange();
      }

      // Calculate display range based on range type
      float dataMin = _dataMin;
      float dataMax = _dataMax;

      switch (_rangeType) {
        case MultilineGraph.RangeType.DefaultOnly:
          _minValue = _defaultMinValue;
          _maxValue = _defaultMaxValue;
          break;

        case MultilineGraph.RangeType.Adaptive:
          _minValue = _defaultMinValue;
          _maxValue = _defaultMaxValue;

          if (dataMin < _minValue) {
            float overshoot = _minValue - dataMin;
            _minValue = dataMin - overshoot * _rangePadding;
          }
          if (dataMax > _maxValue) {
            float overshoot = dataMax - _maxValue;
            _maxValue = dataMax + overshoot * _rangePadding;
          }
          break;

        case MultilineGraph.RangeType.Sticky:
          if (dataMin < _minValue) {
            float overshoot = _minValue - dataMin;
            _minValue = dataMin - overshoot * _rangePadding;
          }
          if (dataMax > _maxValue) {
            float overshoot = dataMax - _maxValue;
            _maxValue = dataMax + overshoot * _rangePadding;
          }
          break;
      }

      // Safety check for invalid range
      if (_minValue >= _maxValue) {
        _minValue = _defaultMinValue;
        _maxValue = _defaultMaxValue;

        if (_minValue >= _maxValue) {
          _minValue = 0f;
          _maxValue = 1f;
        }
      }

      _dataTexture.SetPixels(_dataPixels);
      _dataTexture.Apply();

      _dataDirty = false;
    }

    public void ApplyToMaterial(Material mat) {
      // Convert int arrays to float arrays for shader
      for (int i = 0; i < MaxLines; i++) {
        _writeIndicesFloat[i] = _writeIndices[i];
        _sampleCountsFloat[i] = _sampleCounts[i];
      }

      mat.SetTexture(_dataTextureShaderPropertyID, _dataTexture);
      mat.SetInt(_maxSamplesShaderPropertyID, MaxSamples);
      mat.SetInt(_lineCountShaderPropertyID, _lineCount);
      mat.SetInt(_thresholdCountShaderPropertyID, _thresholdCount);
      mat.SetFloat(_minValueShaderPropertyID, _minValue);
      mat.SetFloat(_maxValueShaderPropertyID, _maxValue);
      mat.SetColorArray(_lineColorsShaderPropertyID, _lineColors);
      mat.SetFloatArray(_thresholdsShaderPropertyID, _thresholds);
      mat.SetColorArray(_thresholdColorsShaderPropertyID, _thresholdColors);
      mat.SetFloatArray(_writeIndicesShaderPropertyID, _writeIndicesFloat);
      mat.SetFloatArray(_sampleCountsShaderPropertyID, _sampleCountsFloat);
      mat.SetFloatArray(_lineVisibleShaderPropertyID, _lineVisible);
      mat.SetFloatArray(_thresholdVisibleShaderPropertyID, _thresholdVisible);
    }
  }
#endif
}