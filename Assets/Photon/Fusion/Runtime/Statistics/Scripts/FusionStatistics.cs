namespace Fusion.Statistics {
#if FUSION_ENABLE_UGUI
  using System.Collections.Generic;
  using UnityEngine;
  using UnityEngine.UI;


  /// <summary>
  /// Add this class to a <see cref="NetworkRunner"/> prefab and when a game is started a statistics canvas will be initialized to display internal stats. Stats are only collected when using DEBUG dll.
  /// </summary>
  [ScriptHelp(BackColor = StatisticsBackColor)]
  [DisallowMultipleComponent]
  public class FusionStatistics : SimulationBehaviour, ISpawned, IDespawned, IAfterUpdate {
    public const ScriptHeaderBackColor StatisticsBackColor = ScriptHeaderBackColor.Orange;

    /// <summary>
    /// The global statistics canvas shared between all peers.
    /// </summary>
    public static Canvas GlobalStatisticsCanvas { get; private set; }
    /// <summary>
    /// Configuration used by the statistics system.
    /// </summary>
    public static FusionStatisticsConfig Config { get; private set; }
    /// <summary>
    /// Estimate amount of <see cref="IAfterUpdate"/> calls per second. Used to extrapolate threshold values from update to seconds.
    /// </summary>
    public static int EstimateFusionAfterUpdatesPerSecond { get; private set; }
    /// <summary>
    /// The root panel for peer statistics.
    /// </summary>
    public FusionStatisticsRoot Root => _statsRootInstance;
    /// <summary>
    /// List of available statistics pages.
    /// </summary>
    public List<FusionStatisticsPage> Pages => _pages;

    private const string STATS_ROOT_PREFAB_PATH = "FusionStatsResources/FusionStatisticsRoot";
    private const string STATS_DEFAULT_CONFIG_ASSET_PATH = "FusionStatsResources/FusionStatisticsDefaultConfig";
    private FusionStatisticsRoot _statsRootPrefab;
    private FusionStatisticsRoot _statsRootInstance;
    private List<FusionStatisticsPage> _pages;
    private int _currentPage;

    private float _refreshTime;

    // Required to try to extrapolate the per update thresholds to a per second data.
    private int _updatesPerSecond;
    private float _updateTime = 1f;

    private void Awake() {
      GetResources();

      if (_statsRootPrefab == null) {
        DestroyWithError("Error loading the required assets for Fusion Statistics. Make sure that the following paths are valid for the Fusion Statistics resource assets: " +
                         $"\n 1. {STATS_ROOT_PREFAB_PATH} \n 2. {STATS_DEFAULT_CONFIG_ASSET_PATH}");
      }
    }

    private void GetResources() {
      Config           = Resources.Load<FusionStatisticsConfig>(STATS_DEFAULT_CONFIG_ASSET_PATH);
      _statsRootPrefab = Resources.Load<FusionStatisticsRoot>(STATS_ROOT_PREFAB_PATH);
    }

    private void DestroyWithError(string error) {
      Debug.LogError(error, this);
      Runner?.RemoveGlobal(this);
      Destroy(this);
    }

    void ISpawned.Spawned() {
      if (Runner.TryGetFusionStatistics(out var statisticsManager) == false) {
        DestroyWithError("Could not get Fusion Statistics Manager.");
        return;
      }

      InitCanvasRoot();
      SetupPages(statisticsManager);
      HandleActiveRoot();
      Root.SetupPagesDropdown();

      if (_pages.Count > 0) {
        _pages[_currentPage].Open();
      }
    }

    private void HandleActiveRoot() {
      var allRoots = GlobalStatisticsCanvas.GetComponentsInChildren<FusionStatisticsRoot>(true);
      // not the first statistics root, disable to allow for multipeer selection.
      if (allRoots.Length > 1) {
        _statsRootInstance.gameObject.SetActive(false);
      } else {
        FusionStatisticsRoot.SetActiveRoot(_statsRootInstance);
      }
    }

    private void SetupPages(FusionStatisticsManager statisticsManager) {
      var allPages = Config.StatisticsPages;
      _pages = new List<FusionStatisticsPage>();
      foreach (var page in allPages) {
        var instance = Instantiate(page, Root.PagesContent);
        _pages.Add(instance);
      }
      _currentPage = 0;

      foreach (FusionStatisticsPage page in _pages) {
        page.SetupPage(Runner, statisticsManager, this);
      }
    }

    private void InitCanvasRoot() {
      if (GlobalStatisticsCanvas == false) {
        GlobalStatisticsCanvas = CanvasCreator.CreateRootCanvas("StatisticsRootCanvas");
        DontDestroyOnLoad(GlobalStatisticsCanvas);
      }

      _statsRootInstance = Instantiate(_statsRootPrefab, GlobalStatisticsCanvas.transform);
      _statsRootInstance.GetComponent<FusionStatisticsRoot>().SetupStatistics(this);
      FusionStatisticsRoot.Roots.Add(_statsRootInstance);

      // root opacity setup
      var image = _statsRootInstance.GetComponent<Image>();
      var color = image.color;
      color.a     = Config.BackgroundOpacity;
      image.color = color;
    }

    /// <summary>
    /// Change the active statistics page.
    /// </summary>
    public void ChangePage(int newPage) {
      if (newPage < 0 || newPage >= _pages.Count) return;
      _pages[_currentPage].Close();
      _currentPage = newPage;
      _pages[_currentPage].Open();
    }

    /// <summary>
    /// Change the active statistics page.
    /// </summary>
    public void ChangePage(FusionStatisticsPage page) {
      for (int i = 0; i < _pages.Count; i++) {
        if (_pages[i] != page) continue;

        ChangePage(i);
        return;
      }
    }

    private void Update() {
      if (Runner == false || _pages == null) return;

      // canvas root was deleted.
      if (_statsRootInstance == false)
      {
        Runner.RemoveStatistics();
        return;
      }

      // disabled root, does not update.
      if (_statsRootInstance.gameObject.activeInHierarchy == false) return;

      if (_refreshTime <= 0) {
        _refreshTime = 1f / Config.PageRefreshRate;
        if (_statsRootInstance.IsVisible) {
          _pages[_currentPage]?.Render();
        }
      } else {
        _refreshTime -= Time.deltaTime;
      }
    }

    /// <inheritdoc />
    public void AfterUpdate() {
      // Calculate after updates per second, for thresholds.
      if (_updateTime > 0) {
        _updateTime -= Time.deltaTime;
        _updatesPerSecond++;

        if (_updateTime <= 0) {
          _updateTime              = 1f;
          EstimateFusionAfterUpdatesPerSecond = _updatesPerSecond;
          _updatesPerSecond = 0;
        }
      }

      _pages?[_currentPage]?.AfterFusionUpdate();
    }

    /// <inheritdoc />
    public void Despawned(NetworkRunner runner, bool hasState) {
      if (_statsRootInstance) {
        FusionStatisticsRoot.Roots.Remove(_statsRootInstance);
        Destroy(_statsRootInstance.gameObject);
      }
    }
  }

  /// <summary>
  /// Extension methods to handle statistics.
  /// </summary>
  public static class StatisticsExtensions {
    /// <summary>
    /// Set up a <see cref="FusionStatistics"/> and create a <see cref="FusionStatisticsRoot"/> to display the runner statistics.
    /// </summary>
    public static FusionStatistics SetupStatistics(this NetworkRunner runner, FusionStatisticsConfig customConfig = null) {
      if (runner == false || runner.IsRunning == false) {
        Debug.LogWarning($"NetworkRunner should be running to setup {typeof(FusionStatistics)}");
      }

      var statistics = runner.gameObject.AddComponent<FusionStatistics>();
      if (statistics) {
        runner.AddGlobal(statistics);
      }

      return statistics;
    }

    /// <summary>
    /// Destroy and clean any <see cref="FusionStatistics"/> and <see cref="FusionStatisticsRoot"/> for this <see cref="NetworkRunner"/>
    /// </summary>
    public static void RemoveStatistics(this NetworkRunner runner) {
      var statistics = runner.GetComponent<FusionStatistics>();

      if (runner && runner.IsRunning) {
        runner.RemoveGlobal(statistics);
      }
      Object.Destroy(statistics);

      var canvasRoot = FusionStatistics.GlobalStatisticsCanvas;

      // Canvas root was already destroyed. Probably in world space.
      if (canvasRoot == false) return;

      FusionStatisticsRoot nextValidRoot = FusionStatisticsRoot.ActiveRoot == statistics.Root ? null : FusionStatisticsRoot.ActiveRoot;

      // find next valid canvas root.
      if (nextValidRoot == false) {
        var childCount = canvasRoot.transform.childCount;
        for (int i = 0; i < childCount; i++) {
          var child = canvasRoot.transform.GetChild(i);
          if (child.TryGetComponent<FusionStatisticsRoot>(out var childStatRoot) && childStatRoot.Statistics != statistics) {
            nextValidRoot = childStatRoot;
          }
        }
      }

      // destroy base canvas.
      if (nextValidRoot == false) {
        Object.Destroy(canvasRoot.gameObject);
      } else {
        FusionStatisticsRoot.SetActiveRoot(nextValidRoot);
      }
    }

    /// <summary>
    /// Define a transform as world anchor for the statistics canvas. Provide a null transform to reset to screen space.
    /// </summary>
    public static void SetStatisticsWorldAnchor(this NetworkRunner runner, Transform anchor) {
      var canvas = FusionStatistics.GlobalStatisticsCanvas;
      if (canvas == false) return;

      canvas.transform.SetParent(anchor, false);
      var scaler = canvas.GetComponent<CanvasScaler>();
      // restoring to screen space
      if (anchor == false) {
        CanvasCreator.SetCanvasToScreenSpace(canvas, scaler);
      } else {
        CanvasCreator.SetCanvasToWorldSpace(canvas, scaler);
      }
    }
  }
#endif
}