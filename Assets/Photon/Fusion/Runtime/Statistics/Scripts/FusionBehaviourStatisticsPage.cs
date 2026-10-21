namespace Fusion.Statistics {
#if FUSION_ENABLE_UGUI
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using UnityEngine;

  public class FusionBehaviourStatisticsPage : FusionStatisticsPage {
    public override string PageName => "Behaviour";

    [Header("References")]
    [SerializeField] private RectTransform _content;
    [SerializeField] private FusionBehaviourStats _behaviourStatsPrefab;
    [SerializeField] private MultipleOptionsPanel _addBehaviourPanelPrefab;
    private MultipleOptionsPanel _addBehaviourPanelInstance;

    /// <summary>
    /// If the list is displaying fixed update network, displaying render if false.
    /// </summary>
    public bool DisplayingFun => _showFun;

    private Type[] _allBehaviours;
    private List<FusionBehaviourStats> _stats = new();
    private bool _showFun;

    /// <summary>
    /// Specify whether to display FixedUpdateNetwork stats or Render stats if not.
    /// </summary>
    public void DisplayFixedUpdateNetwork(bool value) {
      _showFun = value;
    }

    /// <inheritdoc />
    public override void Init() {
      var behaviourList = new List<Type>();
      Assembly[] allLoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
      TypeInfo baseTypeInfo = typeof(SimulationBehaviour).GetTypeInfo();
      foreach (var assembly in allLoadedAssemblies) {
        foreach (var type in assembly.DefinedTypes) {
          if (type.IsSubclassOf(baseTypeInfo)) {
            behaviourList.Add(type);
          }
        }
      }
      _allBehaviours = behaviourList.ToArray();

      DisplayFixedUpdateNetwork(true);
    }

    /// <summary>
    /// Open the panel to monitor a new behaviour statistics.
    /// </summary>
    public void OpenAddBehaviourPanel() {
      if (_addBehaviourPanelInstance) return;

      _addBehaviourPanelInstance = Instantiate(_addBehaviourPanelPrefab, FusionStatistics.GlobalStatisticsCanvas.transform);
      _addBehaviourPanelInstance.Setup("Select Behaviour", _allBehaviours, t => t.Name, AddBehaviourStat);
    }

    private void AddBehaviourStat(Type type) {
      // already have a behaviour stat for that type.
      if (_stats.Select(b => b.BehaviourType == type).Any(r => r)) return;

      var instance = Instantiate(_behaviourStatsPrefab, _content);
      _stats.Add(instance);
      instance.Setup(type, this);
    }

    /// <inheritdoc />
    public override void Render() {
      foreach (var stat in _stats) {
        stat.RefreshView();
      }
    }

    /// <inheritdoc />
    public override void AfterFusionUpdate() {
      foreach (var stat in _stats) {
        stat.AccumulateRunAndTime(this);
      }
    }

    /// <summary>
    /// Remove a behaviour from the list.
    /// </summary>
    public void DeleteStat(FusionBehaviourStats fusionBehaviourStats) {
      _stats.Remove(fusionBehaviourStats);
      Destroy(fusionBehaviourStats.gameObject);
    }
  }
#endif
}