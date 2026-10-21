namespace Fusion.Menu {
#if FUSION_ENABLE_UGUI
  using System.Linq;
  using System.Threading.Tasks;
#if FUSION_ENABLE_TEXTMESHPRO
  using Dropdown = TMPro.TMP_Dropdown;
#else
  using Dropdown = UnityEngine.UI.Dropdown;
#endif
  using UnityEngine;

  /// <summary>
  /// The scene selection screen.
  /// </summary>
  public partial class FusionMenuUIScenes : FusionMenuUIScreen {
    /// <summary>
    /// The dropdown element for the scene selection.
    /// </summary>
    [InlineHelp, SerializeField] protected Dropdown _availableScenes;
    /// <summary>
    /// The image element for the screenshot preview.
    /// </summary>
    [InlineHelp, SerializeField] protected UnityEngine.UI.Image _preview;
    /// <summary>
    /// The default sprite to use when there is no scene preview.
    /// </summary>
    [InlineHelp, SerializeField] protected Sprite _defaultSprite;
    /// <summary>
    /// The back button.
    /// </summary>
    [InlineHelp, SerializeField] protected UnityEngine.UI.Button _backButton;
    /// <summary>
    /// Set this to automatically transition to main menu after selecting a map.
    /// </summary>
    public int WaitAfterSelectionAndReturnToMainMenuInMs = 0;

    partial void AwakeUser();
    partial void InitUser();
    partial void ShowUser();
    partial void HideUser();
    partial void SaveChangesUser();

    /// <summary>
    /// The Unity awake method. Calls partial method <see cref="AwakeUser"/> to be implemented on the SDK side.
    /// </summary>
    public override void Awake() {
      base.Awake();
      AwakeUser();
    }

    /// <summary>
    /// The screen init method. Calls partial method <see cref="InitUser"/> to be implemented on the SDK side.
    /// </summary>
    public override void Init() {
      base.Init();
      InitUser();
    }

    /// <summary>
    /// The screen show method. Calls partial method <see cref="ShowUser"/> to be implemented on the SDK side.
    /// </summary>
    public override void Show() {
      base.Show();

      _availableScenes.ClearOptions();
      _availableScenes.AddOptions(Config.AvailableSceneAssets.Select(m => m.NameOrSceneName).ToList());

      var sceneIndex = Config.AvailableSceneAssets.FindIndex(m => m.ScenePath == ConnectionArgs.Scene.ScenePath);
      if (sceneIndex >= 0) {
        _availableScenes.SetValueWithoutNotify(sceneIndex);
        RefreshPreviewSprite();
      } else if (Config.AvailableSceneAssets.Count > 0) {
        _availableScenes.SetValueWithoutNotify(0);
        ConnectionArgs.Scene = Config.AvailableSceneAssets[0];
      }

      ShowUser();
    }

    /// <summary>
    /// The screen hide method. Calls partial method <see cref="HideUser"/> to be implemented on the SDK side.
    /// </summary>
    public override void Hide() {
      base.Hide();
      HideUser();
    }

    /// <summary>
    /// Is called when the <see cref="_backButton"/> is pressed using SendMessage() from the UI object.
    /// </summary>
    public virtual void OnBackButtonPressed() {
      Controller.Show<FusionMenuUIMain>();
    }


    /// <summary>
    /// Is called when <see cref="_availableScenes"/> has changed it's value from the UI using SendMessage().
    /// </summary>
#if UNITY_WEBGL
    protected virtual void OnSaveChanges() {
#else
    protected virtual async void OnSaveChanges() {
#endif
      RefreshPreviewSprite();

      ConnectionArgs.Scene = Config.AvailableSceneAssets[_availableScenes.value];
      ConnectionArgs.SaveToPlayerPrefs();

      SaveChangesUser();

#if !UNITY_WEBGL
      if (WaitAfterSelectionAndReturnToMainMenuInMs > 0) {
        await Task.Delay(WaitAfterSelectionAndReturnToMainMenuInMs);
        Controller.Show<FusionMenuUIMain>();
      }
#endif
    }

    /// <summary>
    /// Updates the preview sprite and calls <see cref="FusionMenuImageFitter.OnResolutionChanged"/> via SendMessage().
    /// </summary>
    protected void RefreshPreviewSprite() {
      if (Config.AvailableSceneAssets.Count == 0) {
        return;
      }

      _preview.sprite = Config.AvailableSceneAssets[_availableScenes.value].Preview;

      if (_preview.sprite == null) {
        _preview.sprite = Config.DefaultScenePreview;
      }

      if (_preview.sprite == null) {
        _preview.sprite = _defaultSprite;
      }

      _preview.gameObject.SendMessage("OnResolutionChanged");
    }
  }
#endif
}
