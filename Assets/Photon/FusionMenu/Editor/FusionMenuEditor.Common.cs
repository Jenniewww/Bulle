// merged MenuEditor

#region FusionMenuUIControllerEditor.cs

namespace Fusion.Editor {
#if FUSION_ENABLE_UGUI
  using Fusion;
  using Fusion.Menu;
  using UnityEditor;

  /// <summary>
  /// The custom editor searches and displays all <see cref="FusionMenuSceneInfo"/> assets in the project."/>
  /// </summary>
  [CustomEditor(typeof(FusionMenuUIController))]
  public class FusionMenuUIControllerEditor : Editor {
    bool _sceneInfoFoldout;
    FusionMenuSceneInfo[] _sceneInfo;

    /// <summary>
    /// Adding scene info asset selection foldout.
    /// </summary>
    public override void OnInspectorGUI() {
      base.OnInspectorGUI();

      _sceneInfoFoldout = EditorGUILayout.Foldout(_sceneInfoFoldout, "Scene Info Files");
      if (_sceneInfoFoldout) {
        _sceneInfo ??= UnityEngine.Resources.LoadAll<FusionMenuSceneInfo>("");
        foreach (var info in _sceneInfo) {
          EditorGUILayout.ObjectField(info, typeof(FusionMenuSceneInfo), false);
        }
      }
    }
  }
#endif
}

#endregion


#region FusionMenuUIScreenEditor.cs

namespace Fusion.Editor {
#if FUSION_ENABLE_UGUI
  using Menu;
  using UnityEditor;

  /// <summary>
  /// Debug FusionMenuUIScreen content.
  /// </summary>
  [CustomEditor(typeof(FusionMenuUIScreen), true)]
  public class FusionMenuUIScreenEditor : Editor {
    /// <inheritdoc/>
    public override void OnInspectorGUI() {
      base.OnInspectorGUI();

      var data = (FusionMenuUIScreen)target;

      if (data.ConnectionArgs != null) {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Connect Args", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(true)) {
          EditorGUILayout.TextField("Username", data.ConnectionArgs.Username);
          EditorGUILayout.TextField("Session", data.ConnectionArgs.Session);
          EditorGUILayout.TextField("PreferredRegion", data.ConnectionArgs.PreferredRegion);
          EditorGUILayout.TextField("Region", data.ConnectionArgs.Region);
          EditorGUILayout.TextField("AppVersion", data.ConnectionArgs.AppVersion);
          EditorGUILayout.TextField("Scene", data.ConnectionArgs.Scene != null ? data.ConnectionArgs.Scene.ScenePath : null);
          EditorGUILayout.IntField("MaxPlayerCount", data.ConnectionArgs.MaxPlayerCount);
          EditorGUILayout.Toggle("Creating", data.ConnectionArgs.Creating);
        }
      }
    }
  }
#endif
}

#endregion

