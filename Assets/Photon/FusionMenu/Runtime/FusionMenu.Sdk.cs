#if !FUSION_DEV

#region Assets/Photon/FusionMenu/Runtime/FusionMenuConnectArgs.cs

namespace Fusion.Menu {
  partial class FusionMenuConnectArgs {
    [System.NonSerialized]
    public GameMode? GameMode;
  }
}

#endregion


#region Assets/Photon/FusionMenu/Runtime/FusionMenuMppmJoinCommand.Partial.cs

namespace Fusion.Menu {
#if FUSION_ENABLE_UGUI
  partial class FusionMenuMppmJoinCommand {
    public bool IsSharedMode;

    partial void ApplyUser(FusionMenuConnectArgs args) {
      args.GameMode = IsSharedMode ? GameMode.Shared : GameMode.Client;
    }
  }
#endif
}

#endregion

#endif
