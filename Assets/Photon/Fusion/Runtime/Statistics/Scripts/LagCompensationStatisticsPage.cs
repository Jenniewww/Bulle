namespace Fusion.Statistics {
#if FUSION_ENABLE_UGUI
  using System.Text;
  using UnityEngine;
  using UnityEngine.UI;

  public class LagCompensationStatisticsPage : FusionStatisticsPage {
    /// <inheritdoc />
    public override string PageName => "Lag Compensation";

    [SerializeField] private RadialChart _hitboxesUsage;
    [Space]
    [SerializeField] private LineChart _totalElapsedTime;
    [SerializeField] private LineChart _advanceBufferTime;
    [SerializeField] private LineChart _updateBufferTime;
    [SerializeField] private LineChart _addOnBufferTime;
    [SerializeField] private LineChart _refitBVHTime;
    [SerializeField] private LineChart _updateBVHTime;
    [SerializeField] private LineChart _addOnBVHTime;

    /// <inheritdoc />
    public override void Init() {
      _hitboxesUsage.Setup("Hitboxes Usage");

      _totalElapsedTime.Setup("Total Elapsed Time", FusionStatsLookup.LOOKUP_TABLE_0_00ms, "{0} ms", lookUpTableMultiplier: 100);
      _advanceBufferTime.Setup("Advance Buffer Time", FusionStatsLookup.LOOKUP_TABLE_0_00ms, "{0} ms", lookUpTableMultiplier: 100);
      _updateBufferTime.Setup("Update Buffer Time", FusionStatsLookup.LOOKUP_TABLE_0_00ms, "{0} ms", lookUpTableMultiplier: 100);
      _addOnBufferTime.Setup("Add on Buffer Time", FusionStatsLookup.LOOKUP_TABLE_0_00ms, "{0} ms", lookUpTableMultiplier: 100);
      _refitBVHTime.Setup("Refit BVH Time", FusionStatsLookup.LOOKUP_TABLE_0_00ms, "{0} ms", lookUpTableMultiplier: 100);
      _updateBVHTime.Setup("Update BVH Time", FusionStatsLookup.LOOKUP_TABLE_0_00ms, "{0} ms", lookUpTableMultiplier: 100);
      _addOnBVHTime.Setup("Add on BVH Time", FusionStatsLookup.LOOKUP_TABLE_0_00ms, "{0} ms", lookUpTableMultiplier: 100);
    }

    /// <inheritdoc />
    public override void Render() {
      _totalElapsedTime.RefreshDisplay();
      _advanceBufferTime.RefreshDisplay();
      _updateBufferTime.RefreshDisplay();
      _addOnBufferTime.RefreshDisplay();
      _refitBVHTime.RefreshDisplay();
      _updateBVHTime.RefreshDisplay();
      _addOnBVHTime.RefreshDisplay();

      _hitboxesUsage.RefreshDisplay();
    }

    /// <inheritdoc />
    public override void AfterFusionUpdate() {
      var lagCompSnapshot = StatisticsManager.LagCompensationSnapshot;
      if (lagCompSnapshot == null) return;

      _totalElapsedTime.AddValue((float)lagCompSnapshot.TotalElapsedTime);
      _advanceBufferTime.AddValue((float)lagCompSnapshot.AdvanceBufferTime);
      _updateBufferTime.AddValue((float)lagCompSnapshot.UpdateBufferTime);
      _addOnBufferTime.AddValue((float)lagCompSnapshot.AddOnBufferTime);
      _refitBVHTime.AddValue((float)lagCompSnapshot.RefitBVHTime);
      _updateBVHTime.AddValue((float)lagCompSnapshot.UpdateBVHTime);
      _addOnBVHTime.AddValue((float)lagCompSnapshot.AddOnBVHTime);

      _hitboxesUsage.SetValue(lagCompSnapshot.HitboxesCount, Runner.Config.LagCompensation.HitboxDefaultCapacity);
    }
  }
#endif
}