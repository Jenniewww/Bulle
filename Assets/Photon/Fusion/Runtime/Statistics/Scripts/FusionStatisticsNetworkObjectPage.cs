namespace Fusion.Statistics {
#if FUSION_ENABLE_UGUI
  using System.Collections.Generic;
  using UnityEngine;

  public class FusionStatisticsNetworkObjectPage : FusionStatisticsPage {
    /// <inheritdoc />
    public override string PageName => "Network Object";

    [Header("References")]
    [SerializeField] private FusionStatisticsNetworkObjectStats _prefabNOStats;
    [SerializeField] private MultipleOptionsPanel _multipleOptionsPrefab;
    [SerializeField] private Transform _content;

    private MultipleOptionsPanel _NoOptionsInstance;
    private List<FusionStatisticsNetworkObjectStats> _networkObjectStats = new();

    /// <summary>
    /// Add a new network object to be monitored.
    /// </summary>
    public void MonitorObject(NetworkId networkId) {
      if (StatisticsManager.IsObjectMonitored(networkId)) return;

      StatisticsManager.MonitorNetworkObject(networkId);

      var NOStat = Instantiate(_prefabNOStats, _content);
      NOStat.Setup(this, Runner.FindObject(networkId).Name, networkId);
      _networkObjectStats.Add(NOStat);
    }

    /// <summary>
    /// Stop monitoring an object and destroy <see cref="FusionStatisticsNetworkObjectStats"/> instance.
    /// </summary>
    /// <param name="stats"></param>
    public void RemoveMonitoredNetworkObject(FusionStatisticsNetworkObjectStats stats) {
      _networkObjectStats.Remove(stats);
      StatisticsManager.StopMonitorNetworkObject(stats.ID);
      Destroy(stats.gameObject);
    }

    /// <summary>
    /// Open a pop-up to search for all NetworkObject alive on the NetworkRunner and select one to be monitored.
    /// </summary>
    public void SearchAllNetworkObjects() {
      if (_NoOptionsInstance) return;

      var allObjects = Runner.GetAllNetworkObjects().ToArray();

      _NoOptionsInstance = Instantiate(_multipleOptionsPrefab, FusionStatistics.GlobalStatisticsCanvas.transform);
      _NoOptionsInstance.Setup("Select Object", allObjects, no => no.Name, no => MonitorObject(no.Id));
    }

    /// <inheritdoc />
    public override void Init() {
    }

    /// <inheritdoc />
    public override void Render() {
      foreach (FusionStatisticsNetworkObjectStats stats in _networkObjectStats) {
        stats.RefreshView();
      }
    }

    /// <inheritdoc />
    public override void AfterFusionUpdate() {
      var toRemove = new List<FusionStatisticsNetworkObjectStats>();
      foreach (FusionStatisticsNetworkObjectStats stats in _networkObjectStats) {
        if (Runner.Exists(stats.ID) == false) {
          toRemove.Add(stats);
          continue;
        }

        stats.SetData(StatisticsManager);
      }

      foreach (var remove in toRemove) {
        _networkObjectStats.Remove(remove);
        Destroy(remove.gameObject);
      }
    }
  }
#endif
}