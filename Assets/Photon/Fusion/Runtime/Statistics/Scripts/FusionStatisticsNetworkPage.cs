namespace Fusion.Statistics {
#if FUSION_ENABLE_UGUI
  using System.Collections.Generic;
  using UnityEngine;

  public class FusionStatisticsNetworkPage : FusionStatisticsPage {
    /// <inheritdoc />
    public override string PageName => "Network";

    [Header("References")]
    [SerializeField] private LineChart _rtt;
    [SerializeField] private LineChart _inBandwidth;
    [SerializeField] private LineChart _outBandwidth;
    [SerializeField] private LineChart _inPackets;
    [SerializeField] private LineChart _outPackets;
    [SerializeField] private LineChart _inputInBandwidth;
    [SerializeField] private LineChart _inputOutBandwidth;
    [SerializeField] private LineChart _outPacketsGame;
    [SerializeField] private LineChart _outPacketsReliable;
    [SerializeField] private LineChart _outPacketsStreaming;
    [SerializeField] private LineChart _gameQueueDepth;
    [SerializeField] private LineChart _reliableQueueDepth;
    [SerializeField] private LineChart _fragmentGroupsLost;
    [SerializeField] private LineChart _fragmentGroupSize;

    private float _lastRTT; // used instead of 0.

    /// <inheritdoc />
    public override void Init() {
      var byteLabel = "{0} B";
      var unitTable = FusionStatsLookup.LOOKUP_TABLE_0;
      var byteTable = FusionStatsLookup.LOOKUP_TABLE_0_BYTES;

      _rtt.Setup("RTT", FusionStatsLookup.LOOKUP_TABLE_0ms, "{0} ms", forcePerUpdate: true);
      _inBandwidth.Setup("In Bandwidth", byteTable, byteLabel);
      _outBandwidth.Setup("Out Bandwidth", byteTable, byteLabel);
      _inPackets.Setup("In Packets", unitTable);
      _outPackets.Setup("Out Packets", unitTable);
      _inputInBandwidth.Setup("Input In Bandwidth", byteTable, byteLabel);
      _inputOutBandwidth.Setup("Input Out Bandwidth", byteTable, byteLabel);
      _outPacketsGame?.Setup("Out Packets (Game)", unitTable);
      _outPacketsReliable?.Setup("Out Packets (Reliable)", unitTable);
      _outPacketsStreaming?.Setup("Out Packets (Streaming)", unitTable);
      _gameQueueDepth?.Setup("Game Queue Depth", unitTable);
      _reliableQueueDepth?.Setup("Reliable Queue Depth", unitTable);
      _fragmentGroupsLost?.Setup("Fragment Groups Lost", unitTable);
      _fragmentGroupSize?.Setup("Fragment Group Size", unitTable);
    }

    /// <inheritdoc />
    public override void Render() {
      _rtt.RefreshDisplay();
      _inBandwidth.RefreshDisplay();
      _outBandwidth.RefreshDisplay();
      _inPackets.RefreshDisplay();
      _outPackets.RefreshDisplay();
      _inputInBandwidth.RefreshDisplay();
      _inputOutBandwidth.RefreshDisplay();

      _outPacketsGame?.RefreshDisplay();
      _outPacketsReliable?.RefreshDisplay();
      _outPacketsStreaming?.RefreshDisplay();
      _gameQueueDepth?.RefreshDisplay();
      _reliableQueueDepth?.RefreshDisplay();
      _fragmentGroupsLost?.RefreshDisplay();
      _fragmentGroupSize?.RefreshDisplay();
    }

    /// <inheritdoc />
    public override void AfterFusionUpdate() {
      var rtt = StatisticsManager.SimulationSnapshot.Stats.GetValueOrDefault(FusionStatType.RoundTripTime, 0);
      var inB = StatisticsManager.SimulationSnapshot.Stats.GetValueOrDefault(FusionStatType.InBandwidth, 0);
      var outB = StatisticsManager.SimulationSnapshot.Stats.GetValueOrDefault(FusionStatType.OutBandwidth, 0);
      var inP = StatisticsManager.SimulationSnapshot.Stats.GetValueOrDefault(FusionStatType.InPackets, 0);
      var outP = StatisticsManager.SimulationSnapshot.Stats.GetValueOrDefault(FusionStatType.OutPackets, 0);
      var inInput = StatisticsManager.SimulationSnapshot.Stats.GetValueOrDefault(FusionStatType.InputInBandwidth, 0);
      var outInput = StatisticsManager.SimulationSnapshot.Stats.GetValueOrDefault(FusionStatType.InputOutBandwidth, 0);

      if (rtt == 0) {
        rtt = _lastRTT;
      }

      _lastRTT =  rtt;
      rtt      *= 1000; // rtt is in seconds, convert to ms.

      _rtt.AddValue(rtt);
      _inBandwidth.AddValue(inB);
      _outBandwidth.AddValue(outB);
      _inPackets.AddValue(inP);
      _outPackets.AddValue(outP);
      _inputInBandwidth.AddValue(inInput);
      _inputOutBandwidth.AddValue(outInput);

      // State-fragment / per-channel diagnostics (null-guarded - only render once wired into the prefab).
      var stats = StatisticsManager.SimulationSnapshot.Stats;
      _outPacketsGame?.AddValue(stats.GetValueOrDefault(FusionStatType.OutPacketsGame, 0));
      _outPacketsReliable?.AddValue(stats.GetValueOrDefault(FusionStatType.OutPacketsReliable, 0));
      _outPacketsStreaming?.AddValue(stats.GetValueOrDefault(FusionStatType.OutPacketsStreaming, 0));
      _gameQueueDepth?.AddValue(stats.GetValueOrDefault(FusionStatType.GameQueueDepth, 0));
      _reliableQueueDepth?.AddValue(stats.GetValueOrDefault(FusionStatType.ReliableQueueDepth, 0));
      _fragmentGroupsLost?.AddValue(stats.GetValueOrDefault(FusionStatType.StateFragmentGroupsLost, 0));
      _fragmentGroupSize?.AddValue(stats.GetValueOrDefault(FusionStatType.StateFragmentGroupSize, 0));
    }
  }
#endif
}