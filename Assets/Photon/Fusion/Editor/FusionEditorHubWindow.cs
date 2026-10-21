namespace Fusion.Editor {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using UnityEditor;
  using UnityEditor.Callbacks;
  using UnityEngine;
  using Object = UnityEngine.Object;

  /// <summary>
  /// The main class to manage the Fusion Hub window.
  /// The Hub for example completes the installation of a user project by creating vital config files that are not overwritten by upgrades.
  /// It can also install samples and addons that are packaged with the SDK.
  /// </summary>
  [InitializeOnLoad]
  internal partial class FusionEditorHubWindow : EditorWindow {
    const int NavWidth = 256 + 2;
    const int NavButtonHeight = 56;
    const int NavButtonWidth = 260;
    const int ContentMargin = 22;
    const int IconSize = 32;
    const int IconMargin = 14;
    const float HeartbeatIntervalInSeconds = 1.0f;
    protected const string CurrentPagePlayerPrefsKey = "Fusion.Hub.CurrentPage";
    protected const string ScrollRectPlayerPrefsKey = "Fusion.Hub.ScrollRect";

    static partial void CreateWindowUser(ref FusionEditorHubWindow window);
    static partial void OnImportPackageCompletedUser(string packageName);
    static partial void CheckPopupConditionUser(ref bool shouldPopup, ref int page);
    static partial void FindPagesUser(List<FusionEditorHubPage> pages);

    static Vector2 _windowSize = new Vector2(850, 600);
    static Vector2 _windowPosition = new Vector2(100, 100);
    static List<FusionEditorHubPage> _pages;
    static bool _pagesInitialized;
    static int? _currentPage = null;

    double _nextForceRepaint;
    double _heartbeatTimestamp;
    Vector2? _scrollRect;

    /// <summary>
    /// Get and sets current page from PlayerPrefs. It's handy for domain reloads.
    /// </summary>
    static int CurrentPage {
      get {
        if (_currentPage.HasValue == false) {
          _currentPage = PlayerPrefs.GetInt(CurrentPagePlayerPrefsKey, 0);
        }
        return _currentPage.Value;
      }
      set {
        if (_currentPage.HasValue == false || _currentPage.Value != value) {
          PlayerPrefs.SetInt(CurrentPagePlayerPrefsKey, value);
        }
        _currentPage = value;
      }
    }

    /// <summary>
    /// Get and sets current scrolling from PlayerPrefs. It's handy for domain reloads.
    /// </summary>
    Vector2 ScrollRect {
      get {
        if (_scrollRect.HasValue == false) {
          try {
            _scrollRect = JsonUtility.FromJson<Vector2>(PlayerPrefs.GetString(ScrollRectPlayerPrefsKey, ""));
          } catch {
            _scrollRect = Vector2.zero;
          }
        }
        return _scrollRect.Value;
      }
      set {
        if (_scrollRect.HasValue == false || _scrollRect.Value != value) {
          PlayerPrefs.SetString(ScrollRectPlayerPrefsKey, JsonUtility.ToJson(value));
        }
        _scrollRect = value;
      }
    }

    public static void FindPages(List<FusionEditorHubPage> pages, string assetLabel) {
      foreach (var c in AssetDatabase.FindAssets($"l:{assetLabel} t:{nameof(FusionEditorHubPageSO)}")
                 .Select(x => AssetDatabase.GUIDToAssetPath(x))
                 .Select(path => AssetDatabase.LoadAssetAtPath<FusionEditorHubPageSO>(path).Content)) {
        pages.AddRange(c);
      }
    }

    public static void CloseOpenInstance() {
      if (HasOpenInstances<FusionEditorHubWindow>()) {
        var window = GetWindow<FusionEditorHubWindow>();
        window?.Close();
      }
    }

    protected static List<FusionEditorHubPage> Pages {
      get {
        if (_pagesInitialized == false) {
          // Cache all editor hub layouts founds in the project
          try {
            _pages = new List<FusionEditorHubPage>();
            FindPagesUser(_pages);

            if (_pages.Count == 0) {
              FindPages(_pages, FusionEditorHubPage.AssetLabel);
            }

            // pages can be hidden based on custom conditions
            FusionEditorHubWindow window = null;
            if (EditorWindow.HasOpenInstances<FusionEditorHubWindow>()) {
              window = GetWindow<FusionEditorHubWindow>();
            }

            if (window != null) {
              for (int i = _pages.Count - 1; i >= 0; i--) {
                var page = _pages[i];
                if (page.CheckIsHidden(window.CustomConditionCheck)) {
                  _pages.RemoveAt(i);
                }
              }
            }

            // Pages can overwrite each other by title
            for (int i = _pages.Count - 1; i >= 0; i--) {
              if (string.IsNullOrEmpty(_pages[i].OverwritePage) == false) {
                var index = _pages.FindIndex(p => string.Equals(p.Title, _pages[i].OverwritePage, StringComparison.Ordinal));
                if (index >= 0 && index != i) {
                  _pages[index] = _pages[i];
                  _pages.RemoveAt(i);
                }
              }
            }
            
            // Remove pages with no elements. This allows for overwriting a page to hide it
            for (int i = _pages.Count - 1; i >= 0; i--) {
              var page = _pages[i];
              if (page.Elements.Count == 0) {
                _pages.RemoveAt(i);
              }
            }

            _pagesInitialized = _pages.Count > 0;
          } catch (Exception e) {
            Log.Exception(e);
          }
        }

        return _pages;
      }
    }

    public virtual string AppId { get; set; } = string.Empty;
    
    public virtual string AppIdVoice { get; set; } = string.Empty;

    public virtual Object SdkAppSettingsAsset { get; }

    public Vector2 ContentSize => new Vector2(position.width - NavWidth - ContentMargin * 2, position.height);

    [InitializeOnLoadMethod]
    static void InitializedPackageImportCallbacks() {
      // Package import callbacks are removed during domain reload, here we globally set one.
      AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
    }

#if !FUSION_DISABLE_HUB_POPUP
    [UnityEditor.Callbacks.DidReloadScripts]
    static void OnDidReloadScripts() {
      EditorApplication.delayCall += CheckPopupCondition;
    }
#endif

    /// <summary>
    /// The QPrototypes have to be reloaded to properly work.
    /// Reimporting assets only works after the package import.
    /// </summary>
    static void OnImportPackageCompleted(string packageName) {
      if (EditorWindow.HasOpenInstances<FusionEditorHubWindow>()) {
        Pages[Math.Clamp(CurrentPage, 0, Pages.Count)].OnImportPackageCompleted(packageName);

        OnImportPackageCompletedUser(packageName);
      }
    }

    protected static void OpenCurrentPage() {
      OpenPage(CurrentPage);
    }

    protected static void OpenPage(int page) {
      if (Application.isPlaying) {
        return;
      }

      FusionEditorHubWindow window = null;

      CreateWindowUser(ref window);

      if (window == null) {
        window = GetWindow<FusionEditorHubWindow>(true, "Photon Hub", true);
      }

      if (CurrentPage != page) {
        window.ScrollRect = Vector2.zero;
      }

      CurrentPage = page;
      window.OnGuiHeartbeat();
      window.Show();
    }

    void OnEnable() {
      minSize = _windowSize;
      wantsMouseMove = true;

      Styles ??= new HubStyles(HubSkin, GetBoxStyle);
    }

    void OnGUI() {

      GUI.skin = HubSkin;

      try {
        if (HeartbeatIntervalInSeconds > 0 && EditorApplication.timeSinceStartup > _heartbeatTimestamp + HeartbeatIntervalInSeconds) {
          _heartbeatTimestamp = EditorApplication.timeSinceStartup;
          OnGuiHeartbeat();
        }

        _windowPosition = this.position.position;

        // full window wrapper
        using (new EditorGUILayout.HorizontalScope(GUI.skin.window)) {

          // Left Nav menu
          using (new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(NavWidth), GUILayout.MinWidth(NavWidth))) {
            DrawHeader();
            DrawLeftNavMenu();
          }

          // Right Main Content
          using (new EditorGUILayout.VerticalScope()) {
            if (Pages?.Count > 0) {
              var page = Pages[CurrentPage];
              GUILayout.Label(page.Description, Styles.HeaderText);

              using (new EditorGUILayout.VerticalScope(HubSkin.box)) {
                ScrollRect = EditorGUILayout.BeginScrollView(ScrollRect);
                page.Draw(this, CustomDrawWidget, CustomConditionCheck);
                EditorGUILayout.EndScrollView();
              }
            }
          }
        }

        DrawFooter();

      } catch (ExitGUIException) {
        // hide gui exception
      } catch (Exception e) {
        FusionEditorLog.Exception($"Exception when drawing the Hub Window", e);
      }

      // Force repaints while mouse is over the window, to keep Hover graphics working (Unity quirk)
      var timeSinceStartup = Time.realtimeSinceStartupAsDouble;
      if (Event.current.type == EventType.MouseMove && timeSinceStartup > _nextForceRepaint) {
        // Cap the repaint rate a bit since we are forcing repaint on mouse move
        _nextForceRepaint = timeSinceStartup + .05f;
        Repaint();
      }
    }

    void OnProjectChange() {
      HubUtils.GlobalInstanceMissing.Clear();
    }

    protected virtual void CustomDrawWidget(FusionEditorHubPage page, FusionEditorHubWidget widget) {
    }

    protected virtual bool CustomConditionCheck(FusionEditorHubCondition condition) {
      return true;
    }

#if !FUSION_DISABLE_HUB_POPUP
    /// <summary>
    /// Is used to check if important user files are installed and opens the Hub otherwise.
    /// </summary>
    internal static void CheckPopupCondition() {
      if (EditorApplication.isPlayingOrWillChangePlaymode) {
        return;
      }

      var shouldPopup = false;
      var page = CurrentPage;
      CheckPopupConditionUser(ref shouldPopup, ref page);
      if (shouldPopup == false) {
        return;
      }

      if (EditorApplication.isCompiling || EditorApplication.isUpdating) {
        EditorApplication.delayCall += CheckPopupCondition;
        return;
      }

      EditorApplication.delayCall += () => OpenPage(page);
    }
#endif

    protected virtual void OnGuiHeartbeat() {
    }
  }

#if !FUSION_DISABLE_HUB_POPUP
  class FusionEditorHubWindowAssetPostprocessor : AssetPostprocessor {
    [RunAfterClass(typeof(FusionGlobalScriptableObjectUtils.PostProcessor))]
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
      // Unity handling for post asset processing callback. Checks existence of settings assets every time assets change.
      FusionEditorHubWindow.CheckPopupCondition();
    }
  }
#endif
}
