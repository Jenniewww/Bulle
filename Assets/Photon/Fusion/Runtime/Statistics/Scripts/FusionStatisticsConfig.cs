namespace Fusion.Statistics {
#if FUSION_ENABLE_UGUI
  using System.Collections.Generic;
  using UnityEngine;

  [CreateAssetMenu(menuName = "Fusion/Config/Statistics Config", fileName = "FusionStatisticsConfig")]
  public class FusionStatisticsConfig : ScriptableObject {
    /// <summary>
    /// Side of the screen that the statistics root is anchored to.
    /// </summary>
    public enum Side {Right, Left}

    /// <summary>
    /// Background opacity of the root panel.
    /// </summary>
    [Range(0, 1)]
    public float BackgroundOpacity = .9f;
    /// <summary>
    /// Refresh rate used to call <see cref="FusionStatisticsPage.Render()"/> on the active <see cref="FusionStatisticsPage"/>.
    /// </summary>
    public int PageRefreshRate = 30;

    /// <summary>
    /// Default color gradient to render values on <see cref="LineChart"/>.
    /// </summary>
    [SerializeField]
    public Gradient DefaultGradient = new(){colorKeys = new GradientColorKey[]{new(new Color(0.2f, 1, 0.2f), 0f), new(new Color(0.2f, 0.2f, 1), 1f)}};
    /// <summary>
    /// Special color gradient to render values on <see cref="LineChart"/> when they are above the specified threshold.
    /// </summary>
    [SerializeField]
    public Gradient ThresholdGradient = new(){colorKeys = new GradientColorKey[]{new(new Color(1, 1, 0.2f), 0f), new(new Color(1, 0.2f, 0.2f), 1f)}};

    /// <summary>
    /// Should render zero as transparent on <see cref="LineChart"/>.
    /// </summary>
    public bool RenderZeroAsTransparent = true;

    /// <summary>
    /// Avoid zero on last value of <see cref="LineChart"/>.
    /// </summary>
    public bool DontDisplayZeroOnLastValue = true;

    /// <summary>
    /// List of <see cref="FusionStatisticsPage"/> elements to be instantiated and rendered.
    /// </summary>
    public List<FusionStatisticsPage> StatisticsPages;
  }
#endif
}