namespace Fusion.Statistics {
#if FUSION_ENABLE_UGUI
  using UnityEngine;

  public class FusionStatisticsMemoryPage : FusionStatisticsPage {
    /// <inheritdoc />
    public override string PageName => "Memory";

    [Header("Object Memory")]
    [SerializeField] private RadialChart _objectMemoryChart;
    [SerializeField] private RadialChart _objectFreeBlocksChart;
    [Space]
    [Header("General Memory")]
    [SerializeField] private RadialChart _generalMemoryChart;
    [SerializeField] private RadialChart _generalFreeBlocksChart;

    /// <inheritdoc />
    public override void Init() {
      _objectMemoryChart.Setup("Memory Usage");
      _objectFreeBlocksChart.Setup("Blocks Usage");
      _generalMemoryChart.Setup("Memory Usage");
      _generalFreeBlocksChart.Setup("Blocks Usage");
    }

    /// <inheritdoc />
    public override void Render() {
      _objectMemoryChart.RefreshDisplay();
      _objectFreeBlocksChart.RefreshDisplay();
      _generalMemoryChart.RefreshDisplay();
      _generalFreeBlocksChart.RefreshDisplay();
    }

    /// <inheritdoc />
    public override void AfterFusionUpdate() {
      var memorySnapshot = StatisticsManager.MemorySnapshot;

      // object memory
      var objectBytesUsed = memorySnapshot.ObjectAllocatorMemorySnapshot.TotalBytesUsed;
      var objectTotalBytes = objectBytesUsed + memorySnapshot.ObjectAllocatorMemorySnapshot.TotalBytesFree;
      _objectMemoryChart.SetValue(objectBytesUsed, objectTotalBytes);

      var objectTotalBlocks = memorySnapshot.ObjectAllocatorMemorySnapshot.TotalBlocks;
      var objectUsedBlocks = objectTotalBlocks - memorySnapshot.ObjectAllocatorMemorySnapshot.TotalFreeBlocks;
      _objectFreeBlocksChart.SetValue(objectUsedBlocks, objectTotalBlocks);

      // general memory
      var generalBytesUsed = memorySnapshot.GeneralAllocatorMemorySnapshot.TotalBytesUsed;
      var generalTotalBytes = generalBytesUsed + memorySnapshot.GeneralAllocatorMemorySnapshot.TotalBytesFree;
      _generalMemoryChart.SetValue(generalBytesUsed, generalTotalBytes);

      var generalTotalBlocks = memorySnapshot.GeneralAllocatorMemorySnapshot.TotalBlocks;
      var generalUsedBlocks = generalTotalBlocks - memorySnapshot.GeneralAllocatorMemorySnapshot.TotalFreeBlocks;
      _generalFreeBlocksChart.SetValue(generalUsedBlocks, generalTotalBlocks);
    }
  }
#endif
}