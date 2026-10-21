namespace Fusion.Statistics {
#if FUSION_ENABLE_UGUI
  using System.Collections.Generic;
  using UnityEngine;

  public class TimeStatisticsPage : FusionStatisticsPage {
    /// <inheritdoc />
    public override string PageName => "Time Statistics";

    [SerializeField] private LineChart _rtt;
    [SerializeField] private LineChart _inputReceiveDelta;
    [SerializeField] private LineChart _timeResets;
    [SerializeField] private LineChart _stateReceiveDelta;
    [SerializeField] private LineChart _simulationTimeOffset;
    [SerializeField] private LineChart _simulationSpeed;
    [SerializeField] private LineChart _interpolationOffset;
    [SerializeField] private LineChart _interpolationSpeed;
    [SerializeField] private LineChart _inputDelay;

    private float _lastRTT; // used instead of 0.

    /// <inheritdoc />
    public override void Init() {
      _rtt.Setup("RTT", FusionStatsLookup.LOOKUP_TABLE_0ms, "{0}ms", forcePerUpdate: true);
      _inputReceiveDelta.Setup("Input Receive Delta", FusionStatsLookup.LOOKUP_TABLE_0ms, "{0:0}ms");
      _timeResets.Setup("Time Resets", FusionStatsLookup.LOOKUP_TABLE_0ms, "{0:0}ms");
      _stateReceiveDelta.Setup("State Receive Delta", FusionStatsLookup.LOOKUP_TABLE_0ms, "{0:0}ms");
      _simulationTimeOffset.Setup("Simulation Time Offset", FusionStatsLookup.LOOKUP_TABLE_0ms, "{0:0}ms");
      _simulationSpeed.Setup("Simulation Speed", null, "{0:f2}x", forcePerUpdate: true);
      _interpolationOffset.Setup("Interpolation Offset", FusionStatsLookup.LOOKUP_TABLE_0ms, "{0:0}ms");
      _interpolationSpeed.Setup("Interpolation Speed", null, "{0:f2}x", forcePerUpdate: true);
      _inputDelay.Setup("Input Delay", FusionStatsLookup.LOOKUP_TABLE_0ms, "{0:0}ms");
    }

    /// <inheritdoc />
    public override void Render() {
      _rtt.RefreshDisplay();
      _inputReceiveDelta.RefreshDisplay();
      _timeResets.RefreshDisplay();
      _stateReceiveDelta.RefreshDisplay();
      _simulationTimeOffset.RefreshDisplay();
      _simulationSpeed.RefreshDisplay();
      _interpolationOffset.RefreshDisplay();
      _interpolationSpeed.RefreshDisplay();
      _inputDelay.RefreshDisplay();
    }

    /// <inheritdoc />
    public override void AfterFusionUpdate() {
      // time stats are in seconds, grab and convert to milliseconds.
      var inputRcvDelta = StatisticsManager.SimulationSnapshot.Stats.GetValueOrDefault(FusionStatType.InputReceiveDelta, 0f) * 1000;
      var timeResets = StatisticsManager.SimulationSnapshot.Stats.GetValueOrDefault(FusionStatType.TimeResets, 0f) * 1000;
      var stateRcvDelta = StatisticsManager.SimulationSnapshot.Stats.GetValueOrDefault(FusionStatType.StateReceiveDelta, 0f) * 1000;
      var simulationTimeOffset = StatisticsManager.SimulationSnapshot.Stats.GetValueOrDefault(FusionStatType.SimulationTimeOffset, 0f) * 1000;
      var simulationSpeed = StatisticsManager.SimulationSnapshot.Stats.GetValueOrDefault(FusionStatType.SimulationSpeed, 0f);
      var interpolationOffset = StatisticsManager.SimulationSnapshot.Stats.GetValueOrDefault(FusionStatType.InterpolationOffset, 0f) * 1000;
      var interpolationSpeed = StatisticsManager.SimulationSnapshot.Stats.GetValueOrDefault(FusionStatType.InterpolationSpeed, 0f);
      var inputDelay = StatisticsManager.SimulationSnapshot.Stats.GetValueOrDefault(FusionStatType.SimulationInputDelay, 0f) * 1000;

      var rtt = StatisticsManager.SimulationSnapshot.Stats.GetValueOrDefault(FusionStatType.RoundTripTime, 0) * 1000; // rtt is in seconds, convert to ms.
      if (rtt == 0) {
        rtt = _lastRTT;
      }
      _lastRTT =  rtt;

      _rtt.AddValue(rtt);
      _inputReceiveDelta.AddValue(inputRcvDelta);
      _timeResets.AddValue(timeResets);
      _stateReceiveDelta.AddValue(stateRcvDelta);
      _simulationTimeOffset.AddValue(simulationTimeOffset);
      _simulationSpeed.AddValue(simulationSpeed);
      _interpolationOffset.AddValue(interpolationOffset);
      _interpolationSpeed.AddValue(interpolationSpeed);
      _inputDelay.AddValue(inputDelay);
    }
  }
#endif
}