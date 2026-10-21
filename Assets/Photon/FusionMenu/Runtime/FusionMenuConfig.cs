namespace Fusion.Menu {
#if FUSION_ENABLE_UGUI
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using UnityEngine;

  /// <summary>
  /// Photon menu config file implements <see cref="FusionMenuConfig"/>.
  /// Stores static options that affect parts of the menu behavior and selectable configurations.
  /// </summary>
  [ScriptHelp(BackColor = ScriptHeaderBackColor.Blue)]
  [CreateAssetMenu(menuName = "Fusion/Menu/Menu Config")]
  public partial class FusionMenuConfig : FusionScriptableObject {
    /// <summary>
    /// The maximum player count allowed for all game modes.
    /// </summary>
    [InlineHelp, SerializeField] protected int _maxPlayers = 6;
    /// <summary>
    /// Force 60 FPS during menu animations.
    /// </summary>
    [InlineHelp, SerializeField] protected bool _adaptFramerateForMobilePlatform = true;
    /// <summary>
    /// The available Photon AppVersions to be selectable by the user.
    /// An empty list will hide the related dropdown on the settings screen.
    /// </summary>
    [InlineHelp, SerializeField] protected List<string> _availableAppVersions = new List<string> { "1.0" };
    /// <summary>
    /// Static list of regions available in the settings.
    /// An empty entry symbolizes best region option.
    /// An empty list will hide the related dropdown on the settings screen.
    /// </summary>
    [InlineHelp, SerializeField] protected List<string> _availableRegions = new List<string> { "asia", "eu", "sa", "us" };
    /// <summary>
    /// Has been replaced by <see cref="AvailableSceneAssets"/>.
    /// <para>
    /// Static list of scenes available in the scenes menu.
    /// An empty list will hide the related button in the main screen.
    /// PhotonMenuSceneInfo.Name = displayed name
    /// PhotonMenuSceneInfo.ScenePath = the actual Unity scene (must be included in BuildSettings)
    /// PhotonMenuSceneInfo.Preview = a sprite with a preview of the scene (screenshot) that is displayed in the main menu and scene selection screen (can be null)
    /// </para>
    /// </summary>
    [InlineHelp, SerializeField] protected List<PhotonMenuSceneInfo> _availableScenes = new List<PhotonMenuSceneInfo>();
    /// <summary>
    /// The <see cref="FusionMenuMachineId"/> ScriptableObject that stores local ids to use as an option in for AppVersion.
    /// Designed as a convenient development feature.
    /// Can be null.
    /// </summary>
    [InlineHelp, SerializeField] protected FusionMenuMachineId _machineId;
    /// <summary>
    /// The <see cref="FusionMenuPartyCodeGenerator"/> ScriptableObject that is required for party code generation.
    /// Also used to create random player names.
    /// </summary>
    [InlineHelp, SerializeField] protected FusionMenuPartyCodeGenerator _codeGenerator;
    /// <summary>
    /// The default thumbnail sprite.
    /// </summary>
    [InlineHelp] public Sprite DefaultScenePreview;

    /// <summary>
    /// Return the available app versions.
    /// </summary>
    public List<string> AvailableAppVersions => _availableAppVersions;
    /// <summary>
    /// Returns the available regions.
    /// </summary>
    public List<string> AvailableRegions => _availableRegions;
    /// <summary>
    /// Returns the available scenes.
    /// </summary>
    [Obsolete("Use QuantumMenuSceneInfo assets inside a Resources folders instead.")]
    public List<PhotonMenuSceneInfo> AvailableScenes => _availableScenes;
    /// <summary>
    /// Cached scene info assets from Resources.
    /// </summary>
    public List<FusionMenuSceneInfo> AvailableSceneAssets { get; set; }
    /// <summary>
    /// Returns the max player count.
    /// </summary>
    public int MaxPlayerCount => _maxPlayers;
    /// <summary>
    /// Returns an id that should be unique to this machine.
    /// </summary>
    public virtual string MachineId => _machineId != null ? _machineId.Id : null;
    /// <summary>
    /// Returns the code generator.
    /// </summary>
    public FusionMenuPartyCodeGenerator CodeGenerator => _codeGenerator;
    /// <summary>
    /// Returns true if the framerate should be adapted for mobile platforms to force the menu animations to run at 60 FPS.
    /// </summary>
    public bool AdaptFramerateForMobilePlatform => _adaptFramerateForMobilePlatform;

    /// <summary>
    /// Must be called to load scene info assets
    /// </summary>
    public void Init() {
      AvailableSceneAssets = Resources.LoadAll<FusionMenuSceneInfo>("").ToList();

#pragma warning disable CS0618 // Type or member is obsolete
      // Add obsolete available scenes.
      AvailableSceneAssets.AddRange(AvailableScenes.Select(s => {
        var info = ScriptableObject.CreateInstance<FusionMenuSceneInfo>();
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(s), info);
        //Debug.Log($"Added '{info.Name}' scene info from the obsolete AvailableScenes member, consider deleting it and creating a QuantumMenuSceneInfo asset instead.", this);
        return info;
      }));
#pragma warning restore CS0618 // Type or member is obsolete

      if (AvailableSceneAssets.Count > 1) {
        AvailableSceneAssets.Sort((a, b) => string.Compare(a.Name, b.Name));
      }
    }
  }
#endif
}
