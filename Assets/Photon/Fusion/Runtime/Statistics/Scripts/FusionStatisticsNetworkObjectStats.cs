namespace Fusion.Statistics {
#if FUSION_ENABLE_UGUI
  using UnityEngine;
  using UnityEngine.UI;

  public class FusionStatisticsNetworkObjectStats : MonoBehaviour {
    [SerializeField] private LineChart _inB;
    [SerializeField] private LineChart _outB;
    [SerializeField] private Text _title;
    [SerializeField] private Button _closeButton;

    /// <summary>
    /// ID of the monitored NetworkObject.
    /// </summary>
    public NetworkId ID;

    private float _timer;

    /// <summary>
    /// Setups the NetworkObject statistics object.
    /// </summary>
    public void Setup(FusionStatisticsNetworkObjectPage objectPage, string title, NetworkId id) {
      _inB.Setup("In Bandwidth", FusionStatsLookup.LOOKUP_TABLE_0_BYTES, "{0} B");
      _outB.Setup("Out Bandwidth", FusionStatsLookup.LOOKUP_TABLE_0_BYTES, "{0} B");
      _title.text      = title;
      ID               = id;
      _closeButton.onClick.RemoveAllListeners();
      _closeButton.onClick.AddListener(delegate { objectPage.RemoveMonitoredNetworkObject(this); });
    }

    /// <summary>
    /// Collect statistics values for the monitored NetworkObject.
    /// </summary>
    public void SetData(FusionStatisticsManager statisticsManager) {
      if (statisticsManager.ObjectSnapshot.NetworkObjectStatistics.TryGetValue(ID, out var objStats) == false) {
        _inB.AddValue(0);
        _outB.AddValue(0);
        return;
      }

      if (objStats.TryGetValue(FusionObjectStatType.InBandwidth, out var statIn)) {
        _inB.AddValue(statIn);
      }

      if (objStats.TryGetValue(FusionObjectStatType.OutBandwidth, out var statOut)) {
        _outB.AddValue(statOut);
      }
    }

    private void Update() {
      if (_timer > 0) {
        _timer -= Time.deltaTime;
      }
    }

    /// <summary>
    /// Refresh charts display.
    /// </summary>
    public void RefreshView() {
      _inB.RefreshDisplay();
      _outB.RefreshDisplay();
    }
  }
#endif
}