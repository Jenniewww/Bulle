namespace Fusion.Editor {
  using System.Collections.Generic;
  using UnityEngine;

  /// <summary>
  /// Collection of <see cref="FusionEditorHubPage"/>.
  /// </summary>
  [CreateAssetMenu(fileName = "FusionEditorHubPage", menuName = "Fusion/Editor/Fusion Editor Hub Page")]
  // ReSharper disable once InconsistentNaming
  public class FusionEditorHubPageSO : ScriptableObject {
    /// <summary/>
    public List<FusionEditorHubPage> Content;
  }
}
