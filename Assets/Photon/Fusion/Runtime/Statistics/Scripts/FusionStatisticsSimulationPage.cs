namespace Fusion.Statistics {
#if FUSION_ENABLE_UGUI
  using System.Collections.Generic;
  using UnityEngine;

  public class FusionStatisticsSimulationPage : FusionStatisticsPage {
    /// <inheritdoc />
    public override string PageName => "Simulation";

    [Header("References")]
    [SerializeField] private LineChart _forwardTick;
    [SerializeField] private LineChart _resimTick;
    [SerializeField] private LineChart _objUpdateIn;
    [SerializeField] private LineChart _objUpdateOut;

    /// <inheritdoc />
    public override void Init() {
      _forwardTick.Setup("Forward Ticks", FusionStatsLookup.LOOKUP_TABLE_0);
      _resimTick.Setup("Re-simulation Ticks", FusionStatsLookup.LOOKUP_TABLE_0);

      _objUpdateIn.Setup("Object Update In", FusionStatsLookup.LOOKUP_TABLE_0);
      _objUpdateOut.Setup("Object Update Out", FusionStatsLookup.LOOKUP_TABLE_0);
    }


    /// <inheritdoc />
    public override void Render() {
      _forwardTick.RefreshDisplay();
      _resimTick.RefreshDisplay();
      _objUpdateIn.RefreshDisplay();
      _objUpdateOut.RefreshDisplay();
    }

    /// <inheritdoc />
    public override void AfterFusionUpdate() {
      var forwardTicks = StatisticsManager.SimulationSnapshot.Stats.GetValueOrDefault(FusionStatType.ForwardTicks, 0f);
      var resimTicks = StatisticsManager.SimulationSnapshot.Stats.GetValueOrDefault(FusionStatType.Resimulations, 0f);
      var objectUpdateIn = StatisticsManager.SimulationSnapshot.Stats.GetValueOrDefault(FusionStatType.InObjectUpdates, 0f);
      var objectUpdateOut = StatisticsManager.SimulationSnapshot.Stats.GetValueOrDefault(FusionStatType.OutObjectUpdates, 0f);

      _forwardTick.AddValue(forwardTicks);
      _resimTick.AddValue(resimTicks);
      _objUpdateIn.AddValue(objectUpdateIn);
      _objUpdateOut.AddValue(objectUpdateOut);
    }
  }
#endif
}