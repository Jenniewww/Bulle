namespace Fusion.Statistics {
#if FUSION_ENABLE_UGUI
  using System.Collections.Generic;
  using System.Linq;
  using UnityEngine;

  public class FusionStatisticsForecastObjectPage : FusionStatisticsPage {
    /// <inheritdoc />
    public override string PageName => "Forecast Object";

    [Header("References")]
    [SerializeField] private FusionStatisticsForecastObjectStats _prefabNOStats;
    [SerializeField] private MultipleOptionsPanel _multipleOptionsPrefab;
    [SerializeField] private Transform _content;

    private MultipleOptionsPanel _NoOptionsInstance;
    private List<FusionStatisticsForecastObjectStats> _forecastedObjectStats = new();

    /// <summary>
    /// Add a new network object to be monitored.
    /// </summary>
    public void MonitorObject(NetworkTransform nt) {
      nt.SetTraceEnabled(true);
      var NOStat = Instantiate(_prefabNOStats, _content);
      NOStat.Setup(this, nt.name, nt.Object.Id);
      _forecastedObjectStats.Add(NOStat);
    }

    /// <summary>
    /// Stop monitoring an object and destroy <see cref="FusionStatisticsForecastObjectStats"/> instance.
    /// </summary>
    /// <param name="stats"></param>
    public void RemoveMonitoredNetworkObject(FusionStatisticsForecastObjectStats stats) {
      _forecastedObjectStats.Remove(stats);
      var nt = Runner.TryGetNetworkedBehaviourFromNetworkedObjectRef<NetworkTransform>(stats.ID);
      nt.SetTraceEnabled(false);
      Destroy(stats.gameObject);
    }

    /// <summary>
    /// Open a pop-up to search for all Forecasting network transforms and select one to be monitored
    /// </summary>
    public void SearchAllNetworkObjects() {
      if (_NoOptionsInstance) return;

      var allObjects = Runner.GetAllBehaviours<NetworkTransform>().Where(obj => obj.HasForecastEnabled).ToArray();


      _NoOptionsInstance = Instantiate(_multipleOptionsPrefab, FusionStatistics.GlobalStatisticsCanvas.transform);
      _NoOptionsInstance.Setup("Select Object", allObjects, nt => nt.gameObject.name, nt => MonitorObject(nt));
    }

    /// <inheritdoc />
    public override void Init() {
    }

    /// <inheritdoc />
    public override void Render() {
      foreach (var stats in _forecastedObjectStats) {
        stats.RefreshView();
      }
    }

    /// <inheritdoc />
    public override void AfterFusionUpdate() {
      var toRemove = new List<FusionStatisticsForecastObjectStats>();
      foreach (var stats in _forecastedObjectStats) {
        if (Runner.Exists(stats.ID) == false) {
          toRemove.Add(stats);
          continue;
        }

        stats.SetData(StatisticsManager);
      }

      foreach (var remove in toRemove) {
        _forecastedObjectStats.Remove(remove);
        Destroy(remove.gameObject);
      }
    }
  }
#endif
}