namespace Fusion.Statistics {
#if FUSION_ENABLE_UGUI
  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;
  using UnityEngine.UI;

  [ScriptHelp(BackColor = FusionStatistics.StatisticsBackColor)]
  public class FusionStatisticsRoot : FusionMonoBehaviour {
    public static FusionStatisticsRoot ActiveRoot { get; private set; }
    public static List<FusionStatisticsRoot> Roots = new();
    [SerializeField] private MultipleOptionsPanel _multipleOptionsPrefab;
    [SerializeField] private Text _peerText;
    [SerializeField] private Button _collapseButton;
    [SerializeField] private Button _multiPeerButton;
    [SerializeField] private Button _anchorButton;
    [SerializeField] private RectTransform _sideBar;
    [SerializeField] private Dropdown _pagesDropdown;

    public RectTransform PagesContent;
    public FusionStatistics Statistics => _statistics;
    public bool IsVisible => _collapsed == false;
    private FusionStatistics _statistics;

    private MultipleOptionsPanel _peerOptionsInstance;
    private bool _collapsed;
    private RectTransform _rectTransform;
    private Vector2 _originAnchoredPosition;
    private static FusionStatisticsConfig.Side _anchorSide = FusionStatisticsConfig.Side.Right;

    // reset static fields to allow to disable domain reload
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticFields() {
      _anchorSide = FusionStatisticsConfig.Side.Right;
      Roots       = new List<FusionStatisticsRoot>();
      ActiveRoot  = null;
    }

    private void Start() {
      EnsureCorrectAnchor();
    }

    /// <summary>
    /// Setup statistics root panel.
    /// </summary>
    public void SetupStatistics(FusionStatistics statistics) {
      _statistics    = statistics;
      _peerText.text = _statistics.Runner.LocalPlayer.ToString();

      _collapsed = false;
      GetComponentInParent<CanvasScaler>();
      _rectTransform          = (RectTransform)transform;
      _originAnchoredPosition = _rectTransform.anchoredPosition;
    }

    /// <summary>
    /// Create the pages dropdown based on available statistics pages.
    /// </summary>
    public void SetupPagesDropdown() {
      _pagesDropdown.ClearOptions();
      foreach (var page in _statistics.Pages) {
        _pagesDropdown.options.Add(new Dropdown.OptionData { text = page.PageName });
      }

      void OnDropdownChanged(int selected) {
        _statistics.ChangePage(selected);
      }

      _pagesDropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    /// <summary>
    /// Open panel to select between all available peers statistics panels.
    /// </summary>
    public void ToggleMultiPeerPanel() {
      if (_statistics == false || _peerOptionsInstance || _collapsed) return;

      var allRoots = transform.parent.GetComponentsInChildren<FusionStatisticsRoot>(true);
      var multipleOptionsPanel = Instantiate(_multipleOptionsPrefab, transform.parent);
      multipleOptionsPanel.Setup("Select Peer", allRoots, root => root.Statistics?.Runner.LocalPlayer.ToString(), root => {
        gameObject.SetActive(false);
        SetActiveRoot(root);
      });

      _peerOptionsInstance = multipleOptionsPanel;
    }

    internal static void SetActiveRoot(FusionStatisticsRoot root) {
      root.gameObject.SetActive(true);
      ActiveRoot = root;
    }

    /// <summary>
    /// Toggle collapse logic for the panel.
    /// </summary>
    public void ToggleCollapse() {
      _collapsed = !_collapsed;
      var rt = (RectTransform)transform;

      var collapseDir = _anchorSide == FusionStatisticsConfig.Side.Right ? Vector2.right : Vector2.left;
      var pos = _collapsed ? _originAnchoredPosition + collapseDir * rt.rect.width : _originAnchoredPosition;
      StartCoroutine(MoveToPosition(pos, .2f));
      var zRotation = _anchorSide == FusionStatisticsConfig.Side.Right ? -90 : 90;
      _collapseButton.transform.GetChild(0).rotation = Quaternion.Euler(0f, 0f, _collapsed ? zRotation : -zRotation);

      _anchorButton.interactable = _collapsed == false;
      _multiPeerButton.interactable = _collapsed == false;
    }

    /// <summary>
    /// Toggle between left of right anchor side.
    /// </summary>
    public void ToggleAnchorSide() {
      _anchorSide = _anchorSide == FusionStatisticsConfig.Side.Right ? FusionStatisticsConfig.Side.Left : FusionStatisticsConfig.Side.Right;
      // toggle for all roots.
      var roots = transform.parent.GetComponentsInChildren<FusionStatisticsRoot>(true);
      foreach (var root in roots) {
        root.EnsureCorrectAnchor();
      }
    }

    internal void EnsureCorrectAnchor() {
      var currentSide = _anchorSide;

      var minAnchor = _sideBar.anchorMin;
      var maxAnchor = _sideBar.anchorMax;
      var xValue = currentSide == FusionStatisticsConfig.Side.Right ? 0 : 1;
      minAnchor.x        = xValue;
      maxAnchor.x        = xValue;
      _sideBar.anchorMin = minAnchor;
      _sideBar.anchorMax = maxAnchor;

      var sideBarPivot = _sideBar.pivot;
      sideBarPivot.x            = currentSide == FusionStatisticsConfig.Side.Right ? 1 : 0;
      _sideBar.pivot            = sideBarPivot;
      _sideBar.anchoredPosition = Vector3.zero;

      var rootMin = _rectTransform.anchorMin;
      var rootMax = _rectTransform.anchorMax;

      rootMin.x                = currentSide == FusionStatisticsConfig.Side.Right ? .75f : 0f;
      rootMax.x                = currentSide == FusionStatisticsConfig.Side.Right ? 1f : .25f;
      _rectTransform.anchorMin = rootMin;
      _rectTransform.anchorMax = rootMax;

      var collapseButtonIconRot = _collapseButton.transform.GetChild(0).rotation;
      collapseButtonIconRot.z                        *= -1;
      _collapseButton.transform.GetChild(0).rotation =  collapseButtonIconRot;
    }

    private void Update() {
      _multiPeerButton.gameObject.SetActive(Roots.Count > 1);
    }

    private IEnumerator MoveToPosition(Vector2 target, float duration)
    {
      float time = 0;
      Vector2 startPosition = _rectTransform.anchoredPosition;

      while (time < duration)
      {
        _rectTransform.anchoredPosition =  Vector2.Lerp(startPosition, target, time / duration);
        time                           += Time.deltaTime;
        yield return null;
      }

      _rectTransform.anchoredPosition = target;
    }
  }
#endif
}