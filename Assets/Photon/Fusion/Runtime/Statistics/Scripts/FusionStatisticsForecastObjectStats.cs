namespace Fusion.Statistics {
#if FUSION_ENABLE_UGUI
  using UnityEngine;
  using UnityEngine.UI;
  using static Fusion.NetworkTransformTrace;

  public class FusionStatisticsForecastObjectStats : MonoBehaviour {
    [SerializeField] private MultilineGraph _velocityCorrection;
    [SerializeField] private MultilineGraph _stallHeuristic;
    [SerializeField] private MultilineGraph _collisionEnterHeuristic;
    [SerializeField] private Text _title;
    [SerializeField] private Button _closeButton;

    /// <summary>
    /// ID of the monitored NetworkObject.
    /// </summary>
    public NetworkId ID;

    BufferedDataReader<NetworkTransformTrace.CollisionEnterData> _collisionEnterReader;
    BufferedDataReader<NetworkTransformTrace.ForecastData> _forecastReader;
    BufferedDataReader<NetworkTransformTrace.StallHeuristicData> _stallHeuristicReader;
    BufferedDataReader<NetworkTransformTrace.StallData> _stallReader;

    NetworkRunner _runner = null;

    // Collision graph lines
    int _impactSpeedLine;
    int _totalSpeedLine;
    int _minImpactThreshold;

    // Velocity graph lines
    int _currentVelocityLine;
    int _desiredVelocityLine;
    int _lerpedVelocityLine;
    int _lerpAlphaLine;

    // Stall heuristic graph lines
    int _errorSimilarityLine;
    int _correctionProgressLine;
    int _accruedScoreLine;
    int _errorSimilarityThreshold;
    int _correctionProgressThreshold;
    int _accruedScoreThreshold;

    /// <summary>
    /// Setups the NetworkObject statistics object.
    /// </summary>
    public void Setup(FusionStatisticsForecastObjectPage objectPage, string title, NetworkId id) {
      _title.text      = title;
      _runner = objectPage.Runner;
      ID = id;
      _closeButton.onClick.RemoveAllListeners();
      _closeButton.onClick.AddListener(delegate { objectPage.RemoveMonitoredNetworkObject(this); });

      var networkTransform = _runner.TryGetNetworkedBehaviourFromNetworkedObjectRef<NetworkTransform>(ID);
      var settings = networkTransform.PhysicsSettings;

      // Configure collision enter heuristic graph
      _impactSpeedLine = _collisionEnterHeuristic.AddLine(Color.cyan, "Impact Speed");
      _totalSpeedLine = _collisionEnterHeuristic.AddLine(Color.magenta, "Total Speed");
      _minImpactThreshold = _collisionEnterHeuristic.AddThreshold(settings.MinImpactfulCollisionAlignment, Color.cyan, "Min Impact Speed");

      _collisionEnterReader = new BufferedDataReader<CollisionEnterData>(networkTransform.CurrentTrace?.CollisionEnterBuffer ?? null);

      // Configure velocity correction graph
      _currentVelocityLine = _velocityCorrection.AddLine(Color.cyan, "Current Velocity");
      _desiredVelocityLine = _velocityCorrection.AddLine(Color.magenta, "Desired Velocity");
      _lerpedVelocityLine = _velocityCorrection.AddLine(Color.yellow, "Lerped Velocity");
      _lerpAlphaLine = _velocityCorrection.AddLine(Color.green, "Lerp Alpha");

      _forecastReader = new BufferedDataReader<ForecastData>(networkTransform.CurrentTrace?.ForecastBuffer ?? null);

      // Configure stall heuristic graph
      _errorSimilarityLine = _stallHeuristic.AddLine(Color.cyan, "Error Similarity");
      _correctionProgressLine = _stallHeuristic.AddLine(Color.magenta, "Correction Progress");
      _accruedScoreLine = _stallHeuristic.AddLine(Color.yellow, "Accrued Score");
      _errorSimilarityThreshold = _stallHeuristic.AddThreshold(settings.HighErrorSimilarityThreshold, Color.cyan, "Error Similarity<");
      _correctionProgressThreshold = _stallHeuristic.AddThreshold(settings.LowCorrectionProgressThreshold, Color.magenta, "Corr Progress<");
      _accruedScoreThreshold = _stallHeuristic.AddThreshold(settings.MaxErrorTotalTime, Color.yellow, "Accrued Score>");

      _stallHeuristicReader = new BufferedDataReader<StallHeuristicData>(networkTransform.CurrentTrace?.StallHeuristicBuffer ?? null);
      _stallReader = new BufferedDataReader<StallData>(networkTransform.CurrentTrace?.StallBuffer ?? null);
    }

    /// <summary>
    /// Collect statistics values for the monitored NetworkObject.
    /// </summary>
    public void SetData(FusionStatisticsManager statisticsManager) {
      var networkTransform = _runner.TryGetNetworkedBehaviourFromNetworkedObjectRef<NetworkTransform>(ID);
      var settings = networkTransform.PhysicsSettings;

      // Update stall heuristic thresholds
      _stallHeuristic.SetThresholdValue(_errorSimilarityThreshold, settings.HighErrorSimilarityThreshold);
      _stallHeuristic.SetThresholdValue(_correctionProgressThreshold, settings.LowCorrectionProgressThreshold);
      _stallHeuristic.SetThresholdValue(_accruedScoreThreshold, settings.MaxErrorTotalTime);

      // Read stall heuristic data
      bool readNewData = false;
      while (_stallHeuristicReader.Read(out var value)) {
        readNewData = true;
        _stallHeuristic.AddValue(_errorSimilarityLine, value.ErrorSimilarity);
        _stallHeuristic.AddValue(_correctionProgressLine, value.CorrectionProgress);
      }

      if (!readNewData) {
        _stallHeuristic.AddValue(_errorSimilarityLine, _stallHeuristicReader.Recent.ErrorSimilarity);
        _stallHeuristic.AddValue(_correctionProgressLine, _stallHeuristicReader.Recent.CorrectionProgress);
      }

      // Read stall data
      readNewData = false;
      while (_stallReader.Read(out var value)) {
        _stallHeuristic.AddValue(_accruedScoreLine, value.StallProgress);
        readNewData = true;
      }

      if (!readNewData) {
        _stallHeuristic.AddValue(_accruedScoreLine, _stallReader.Recent.StallProgress);
      }

      // Read velocity correction data
      while (_forecastReader.Read(out var value)) {
        _velocityCorrection.AddValue(_currentVelocityLine, value.PreviousVelocity.magnitude);
        _velocityCorrection.AddValue(_desiredVelocityLine, value.DesiredVelocity.magnitude);
        _velocityCorrection.AddValue(_lerpedVelocityLine, value.NewVelocity.magnitude);
        _velocityCorrection.AddValue(_lerpAlphaLine, value.LerpAlpha);
      }

      // Read collision enter data
      while (_collisionEnterReader.Read(out var value)) {
        _collisionEnterHeuristic.AddValue(_impactSpeedLine, value.ImpactAlignment);
        _collisionEnterHeuristic.AddValue(_totalSpeedLine, value.RelativeVelocity);
      }

      // Update collision threshold
      _collisionEnterHeuristic.SetThresholdValue(_minImpactThreshold, settings.MinImpactfulCollisionAlignment);
    }

    /// <summary>
    /// Refresh charts display.
    /// </summary>
    public void RefreshView() {
      _velocityCorrection.RefreshDisplay();
      _stallHeuristic.RefreshDisplay();
      _collisionEnterHeuristic.RefreshDisplay();
    }
  }
#endif
}