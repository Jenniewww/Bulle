namespace Fusion.Statistics {
#if FUSION_ENABLE_UGUI
  using System;
  using UnityEngine;
  using UnityEngine.UI;

  public class FusionBehaviourStats : MonoBehaviour {
    [SerializeField] private Text _name;
    [SerializeField] private Text _runCount;
    [SerializeField] private Text _time;

    public Type BehaviourType => _behaviour;

    private StatAccumulator _runCountAccum;
    private StatAccumulator _execTimeAccum;
    private Type _behaviour;
    private FusionBehaviourStatisticsPage _behaviourPage;


    /// <summary>
    /// Setups the behaviour stats, define the behaviour type and pass a reference to the behaviour page.
    /// </summary>
    public void Setup(Type BehaviourType, FusionBehaviourStatisticsPage behaviourPage) {
      _name.text = BehaviourType.Name;
      _behaviour = BehaviourType;
      _behaviourPage = behaviourPage;

      // Behaviour stats are always per second. Per update is too fast to follow.
      _runCountAccum.DisplayingPerSecond = true;
      _execTimeAccum.DisplayingPerSecond = true;
    }

    /// <summary>
    /// Accumulate execution count and execution time for Render and FixedUpdateNetwork.
    /// </summary>
    public void AccumulateRunAndTime(FusionBehaviourStatisticsPage statisticsPage) {
      if (!statisticsPage.Runner.TryGetBehaviourStatistics(_behaviour, out var snapshot)) return;

      _runCountAccum.Accumulate(statisticsPage.DisplayingFun ? snapshot.FixedUpdateNetworkExecutionCount : snapshot.RenderExecutionCount);
      _execTimeAccum.Accumulate((float)(statisticsPage.DisplayingFun ? snapshot.FixedUpdateNetworkExecutionTime : snapshot.RenderExecutionTime));
    }

    /// <summary>
    /// Refresh the values displayed.
    /// </summary>
    public void RefreshView() {
      var runCount = _runCountAccum.DisplayingPerSecond ? _runCountAccum.ValuePerSecond : _runCountAccum.Value;
      var execTime = _execTimeAccum.DisplayingPerSecond ? _execTimeAccum.ValuePerSecond : _execTimeAccum.Value;
      _runCount.text = FusionStatsLookup.GetValueText(runCount, FusionStatsLookup.LOOKUP_TABLE_0, "{0}");
      _time.text     = FusionStatsLookup.GetValueText(execTime, FusionStatsLookup.LOOKUP_TABLE_0_00ms, "{0} ms", 100);
    }

    /// <summary>
    /// Delete this behaviour stat.
    /// </summary>
    public void DeleteBehaviourStat() {
      _behaviourPage.DeleteStat(this);
    }
  }
#endif
}