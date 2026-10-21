namespace Fusion.Editor {
  using System;
  using System.Collections.Generic;
  using Photon.Realtime;
  using UnityEditor;
  using UnityEditor.PackageManager.Requests;
  using UnityEditor.SceneManagement;
  using UnityEngine;
  using UnityEngine.SceneManagement;
  using Object = UnityEngine.Object;

  internal partial class FusionEditorHubWindow {

    internal const string AssetLabelBeginner = "HubContentBeginner";

    protected static NetworkProjectConfig.FusionHubMode Mode => NetworkProjectConfig.Global.HubMode;

    private static bool AreImportantUserFilesInstalled {
      get {
        return PhotonAppSettings.TryGetGlobal(out _) &&
               NetworkProjectConfigAsset.TryGetGlobal(out _);
      }
    }


    static partial void FindPagesUser(List<FusionEditorHubPage> pages) {
      FindPages(pages, FusionEditorHubPage.AssetLabel);
      if (Mode == NetworkProjectConfig.FusionHubMode.Beginner) {
        FindPages(pages, AssetLabelBeginner);
      }
    }
    static partial void CreateWindowUser(ref FusionEditorHubWindow window) {
      window = GetWindow<FusionEditorHubWindowSdk>(true, "Photon Fusion Hub", true);
      PlayerPrefs.DeleteKey("RequireAddonReload");
    }

    static partial void CheckPopupConditionUser(ref bool shouldPopup, ref int page) {
      // Installation requires popup
      if (AreImportantUserFilesInstalled == false) {
        shouldPopup = false;
        EditorApplication.delayCall += FusionEditorHubWindowSdk.Open;
        return;
      }

      // Layouts requires popup
      for (int i = 0; i < Pages.Count; i++) {
        if (Pages[i].IsPopupRequired) {
          shouldPopup = true;
          page = i;
          break;
        }
      }

      if (PlayerPrefs.HasKey("RequireAddonReload")) {
        shouldPopup = true;
        page = Pages.FindIndex(x => x.Title.Contains("Addons"));
      }
    }

    static partial void OnImportPackageCompletedUser(string packageName) {
      if (packageName == "TMP Essential Resources") {
        // Workaround uninitialized TMP text after installing TMP essential resources
        // Ask to reload current scene to fix the issue
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
          try {
            EditorSceneManager.OpenScene(SceneManager.GetActiveScene().path);
          } catch {
            // Fail silently
          }
        }
      }
      NetworkProjectConfigUtilities.RebuildPrefabTable();
    }

    /// <summary>
    /// Clears the page cache of the Fusion Hub. Allows to change the set of loaded pages.
    /// </summary>
    public static void ClearPagesCache() {
      _pagesInitialized = false;
    }

    internal static void SwitchHubMode(NetworkProjectConfig.FusionHubMode mode) {
      var npc = NetworkProjectConfigAsset.Global;
      npc.Config.HubMode = mode;
      NetworkProjectConfigUtilities.SaveGlobalConfig(npc.Config);

      GetWindow<FusionEditorHubWindowSdk>()?.Close();
      ClearPagesCache();
      FusionEditorHubWindowSdk.Open();
    }
  }

  internal partial class FusionEditorHubWidgetTypeDrawer {
    static partial void RegisterTypesUser(List<string> types) {
      types.Add(FusionEditorHubWindowSdk.CustomWidgetTypes.ClearFusionPlayerPrefs);
      types.Add(FusionEditorHubWindowSdk.CustomWidgetTypes.SwitchHubMode);
#if FUSION_ENABLE_UGUI
      types.Add(nameof(FusionStatisticsHubWidget));
#endif
      types.Add(nameof(EditorHubWidget));
    }
  }

  internal partial class FusionEditorHubConditionDrawer {
    static partial void RegisterTypesUser(List<string> types) {
      types.Add(FusionEditorHubWindowSdk.CustomConditions.AppIdCreated);
      types.Add(FusionEditorHubWindowSdk.CustomConditions.SdkInstalled);
    }
  }

  internal class FusionEditorHubWindowSdk : FusionEditorHubWindow {
    static AddRequest MppmAddRequest;

    internal static class CustomWidgetTypes {
      internal const string ClearFusionPlayerPrefs = "ClearFusionPlayerPrefs";
      internal const string SwitchHubMode = "SwitchHubMode";
    }

    internal static class CustomConditions {
      internal const string AppIdCreated = "AppIdCreated";
      internal const string SdkInstalled = "SdkInstalled";
    }

    public override string AppId {
      get {
        try {
          var photonSettings = PhotonAppSettings.Global;
          return photonSettings.AppSettings.AppIdFusion;
        } catch {
          return string.Empty;
        }
      }
      set {
        var photonSettings = PhotonAppSettings.Global;
        photonSettings.AppSettings.AppIdFusion = value;
        EditorUtility.SetDirty(photonSettings);
        AssetDatabase.SaveAssets();
      }
    }

    public override string AppIdVoice {
      get {
        try {
          var photonSettings = PhotonAppSettings.Global;
          return photonSettings.AppSettings.AppIdVoice;
        } catch {
          return string.Empty;
        }
      }
      set {
        var photonSettings = PhotonAppSettings.Global;
        photonSettings.AppSettings.AppIdVoice = value;
        EditorUtility.SetDirty(photonSettings);
        AssetDatabase.SaveAssets();
      }
    }

    public override Object SdkAppSettingsAsset => PhotonAppSettings.Global;

    public override GUIStyle GetBoxStyle => HubSkin.GetStyle("SteelBox");
    public override GUIStyle GetButtonPaneStyle => HubSkin.GetStyle("ButtonPane");

    static bool _statusInstallationComplete;
    public static bool _statusAppIdSetup;

    protected override bool CustomConditionCheck(FusionEditorHubCondition condition) {
      if (condition.Value == CustomConditions.AppIdCreated) {
        return _statusAppIdSetup;
      } else if (condition.Value == CustomConditions.SdkInstalled) {
        return _statusInstallationComplete;
      }

      return false;
    }



    protected override void CustomDrawWidget(FusionEditorHubPage page, FusionEditorHubWidget widget) {
      // if (widget.WidgetMode.Value == CustomWidgetTypes.CreateSimpleConnectionScene) {
      //
      //   DrawButtonAction(widget.Icon, widget.Text, widget.Subtext,
      //     statusIcon: widget.GetStatusIcon(this),
      //     callback: () => {
      //       QuantumEditorMenuCreateScene.CreateSimpleConnectionScene(widget.Scene);
      //       GUIUtility.ExitGUI();
      //     });
      //
      // } else
      if (widget.WidgetMode.Value == CustomWidgetTypes.SwitchHubMode) {
        var labelText = "Switch to Beginner Mode";
        var buttonText = "Fusion Hub is in Advanced Mode. Switch to Beginner Mode to hide advanced content.";
        Action callback = () => { SwitchHubMode(NetworkProjectConfig.FusionHubMode.Beginner); };

        if (Mode == NetworkProjectConfig.FusionHubMode.Beginner) {
          labelText = "Switch to Advanced Mode";
          buttonText = "Fusion Hub is in Beginner Mode. Switch to Advanced Mode to display more content.";
          callback = () => { SwitchHubMode(NetworkProjectConfig.FusionHubMode.Advanced); };
        }

        DrawButtonAction(widget.Icon, labelText, buttonText, statusIcon: widget.GetStatusIcon(this), callback: callback);

      }
      else if (widget.WidgetMode.Value == CustomWidgetTypes.ClearFusionPlayerPrefs) {
        DrawButtonAction(widget.Icon, widget.Text, widget.Subtext,
          statusIcon: widget.GetStatusIcon(this),
          callback: () => {
            ClearAllPlayerPrefs();
          });
      }
#if FUSION_ENABLE_UGUI
      else if (widget.WidgetMode.Value == nameof(FusionStatisticsHubWidget)) {
        FusionStatisticsHubWidget.DrawStatisticsWidget(widget);
      }
#endif
      else if (widget.WidgetMode.Value == nameof(EditorHubWidget)) {
        EditorHubWidget.DrawEditorWidget(widget);
      }
    }

    protected override void OnGuiHeartbeat() {
      _statusAppIdSetup = HubUtils.IsValidGuid(AppId);
    }

    void ClearAllPlayerPrefs() {
      // Hub
      foreach (var page in Pages) {
        page.DeleteAllPlayerPrefKeys();
      }

      PlayerPrefs.DeleteKey(CurrentPagePlayerPrefsKey);
      PlayerPrefs.DeleteKey(ScrollRectPlayerPrefsKey);

      // Menu
      ClearFusionMenuPlayerPrefs();

      // Fusion
      // TODO best region playerprefs?

      var npc = NetworkProjectConfig.Global;
      npc.HubMode = NetworkProjectConfig.FusionHubMode.None;
    }

    // TODO: call after importing menu
    public static void ClearFusionMenuPlayerPrefs() {
      PlayerPrefs.DeleteKey("Photon.Menu.Username");
      PlayerPrefs.DeleteKey("Photon.Menu.Region");
      PlayerPrefs.DeleteKey("Photon.Menu.AppVersion");
      PlayerPrefs.DeleteKey("Photon.Menu.MaxPlayerCount");
      PlayerPrefs.DeleteKey("Photon.Menu.Scene");
      PlayerPrefs.DeleteKey("Photon.Menu.SceneName");
      PlayerPrefs.DeleteKey("Photon.Menu.Framerate");
      PlayerPrefs.DeleteKey("Photon.Menu.Fullscreen");
      PlayerPrefs.DeleteKey("Photon.Menu.Resolution");
      PlayerPrefs.DeleteKey("Photon.Menu.VSync");
      PlayerPrefs.DeleteKey("Photon.Menu.QualityLevel");
    }
    /// <summary>
    /// Open the Fusion Hub window.
    /// </summary>
    [MenuItem("Window/Fusion/Fusion Hub")]
    // NB: ^H -> forces Ctrl+H on Mac. We don't want to use Cmd-H as this is the OS-level Hide shortcut
    [MenuItem("Tools/Fusion/Fusion Hub ^H", false, (int)FusionEditorMenuPriority.TOP)]
    public static void Open() {
      FusionGlobalScriptableObjectUtils.EnsureAssetExists<NetworkProjectConfigAsset>();
      FusionGlobalScriptableObjectUtils.EnsureAssetExists<PhotonAppSettings>();

      var npc = NetworkProjectConfig.Global;
      if (npc.HubMode == NetworkProjectConfig.FusionHubMode.None) {
        FusionEditorHubModeSelectionWindow.ShowWindow();
      } else {
        OpenCurrentPage();
      }
    }
  }
}