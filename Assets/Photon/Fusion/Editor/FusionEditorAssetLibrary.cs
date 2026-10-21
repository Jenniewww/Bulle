namespace Fusion.Editor {
  using UnityEditor;
  using UnityEngine;

  /// <summary>
  /// Library of assets to be used by Fusion Editor.
  /// </summary>
  [CreateAssetMenu(fileName = "FusionEditorAssetLibrary", menuName = "Fusion/Editor/Fusion Editor Asset Library")]
  public class FusionEditorAssetLibrary : ScriptableObject {
    /// <summary>
    /// This has to always point at the correct created asset.
    /// </summary>
    private static readonly string _assetPath = new("Assets/Photon/Fusion/Editor/EditorResources/FusionEditorAssetLibrary.asset");

    [Header("Textures & Icons")] public Texture2D NetworkPropertyIcon;

    private static FusionEditorAssetLibrary _instance;
    private static bool _hasTriedToLoad = false;
    public static FusionEditorAssetLibrary Instance {
      get {
        if (_instance == null && _hasTriedToLoad == false) {
          _instance = AssetDatabase.LoadAssetAtPath<FusionEditorAssetLibrary>(_assetPath);

          if (_instance == null) {
            Debug.LogError($"FusionEditorAssetLibrary asset is missing, provide the asset and restart the project. (Expected Path: {_assetPath})");
          }
          _hasTriedToLoad = true;
        }

        return _instance;
      }
    }
  }
}