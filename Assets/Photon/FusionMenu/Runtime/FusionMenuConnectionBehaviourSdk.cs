namespace FusionDemo {
#if FUSION_ENABLE_UGUI
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;
  using Fusion;
  using Fusion.Menu;
  using Fusion.Photon.Realtime;
  using UnityEngine;
  using UnityEngine.SceneManagement;

  public class FusionMenuConnectionBehaviourSdk : FusionMenuConnectionBehaviour {
    [SerializeField] private FusionMenuConfig _config;
    [SerializeField] private Camera _menuCamera;
    public override string SessionName => _sessionName;
    public override int MaxPlayerCount => _maxPlayerCount;
    public override string Region => _region;
    public override string AppVersion => _appVersion;
    public override List<string> Usernames => _usernames;
    public override bool IsConnected => _runner && _runner.IsRunning;
    public override int Ping => (int)(IsConnected ? _runner.GetPlayerRtt(_runner.LocalPlayer) * 1000 : 0);

    [NonSerialized] private string _sessionName;
    [NonSerialized] private int _maxPlayerCount;
    [NonSerialized] private string _region;
    [NonSerialized] private string _appVersion;
    [NonSerialized] private List<string> _usernames;

    [NonSerialized] private NetworkRunner _runner;
    [NonSerialized] private bool _connectingSafeCheck;
    [NonSerialized] private CancellationTokenSource _cancellationTokenSource;
    [NonSerialized] private CancellationToken _cancellationToken;

    private void Awake() {
      if (!_menuCamera) {
        _menuCamera = Camera.current;
      }

      if (!_config) {
        Log.Error("Fusion menu configuration file not provided.");
      }

      OnBeforeDisconnect.AddListener(EnableMenuCamera);
    }

    private void ToggleMenuCamera(bool value) {
      _menuCamera.gameObject.SetActive(value);
    }

    private void DisableMenuCamera() {
      ToggleMenuCamera(false);
    }

    private void EnableMenuCamera(int error) {
      ToggleMenuCamera(true);
    }

    protected override async Task<ConnectResult> ConnectAsyncInternal(FusionMenuConnectArgs connectArgs) {
      // Safety
      if (_connectingSafeCheck) {
        return new ConnectResult {CustomResultHandling = true, Success = false, FailReason = ConnectFailReason.None};
      }

      _connectingSafeCheck = true;
      if (_runner && _runner.IsRunning) {
        await _runner.Shutdown();
      }

      // Create and prepare Runner object
      _runner = CreateRunner();
      var sceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
      sceneManager.IsSceneTakeOverEnabled = false;

      // Copy and update AppSettings
      var appSettings = CopyAppSettings(connectArgs);

      // Solve StartGameArgs
      var args = new StartGameArgs();
      args.CustomPhotonAppSettings = appSettings;
      args.GameMode = connectArgs.GameMode ?? ResolveGameMode(connectArgs);
      args.SessionName = _sessionName = connectArgs.Session;
      args.PlayerCount = _maxPlayerCount = connectArgs.MaxPlayerCount;

      // Scene info
      var sceneInfo = new NetworkSceneInfo();
      sceneInfo.AddSceneRef(sceneManager.GetSceneRef(connectArgs.Scene.ScenePath), LoadSceneMode.Additive);
      args.Scene = sceneInfo;

      // Cancellation Token
      _cancellationTokenSource?.Dispose();
      _cancellationTokenSource = new CancellationTokenSource();
      _cancellationToken = _cancellationTokenSource.Token;
      args.StartGameCancellationToken = _cancellationToken;

      var regionIndex = _config.AvailableRegions.IndexOf(connectArgs.Region);
      args.SessionNameGenerator = () => _config.CodeGenerator.EncodeRegion(_config.CodeGenerator.Create(), regionIndex);
      var startGameResult = default(StartGameResult);
      var connectResult = new ConnectResult();
      startGameResult = await _runner.StartGame(args);

      connectResult.Success = startGameResult.Ok;
      connectResult.FailReason = ResolveConnectFailReason(startGameResult.ShutdownReason);
      _connectingSafeCheck = false;

      if (connectResult.Success) {
        _sessionName = _runner.SessionInfo.Name;
        DisableMenuCamera();
      }

      FusionMppm.MainEditor?.Send(new FusionMenuMppmJoinCommand() {
        Region = _region,
        Session = _sessionName,
        AppVersion = _appVersion,
        IsSharedMode = args.GameMode == GameMode.Shared,
      });

      return connectResult;
    }

    static Task AsTask(AsyncOperation op) {
      var tcs = new TaskCompletionSource<bool>();
      if (op.isDone)
        tcs.SetResult(true);
      else
        op.completed += _ => tcs.TrySetResult(true);
      return tcs.Task;
    }

    protected override async Task DisconnectAsyncInternal(int reason) {
      var peerMode = _runner.Config?.PeerMode;
      _cancellationTokenSource.Cancel();
      await _runner.Shutdown(shutdownReason: ResolveShutdownReason(reason));

      if (peerMode is NetworkProjectConfig.PeerModes.Multiple) return;

      List<Task> awaitables = new List<Task>();
      for (int i = SceneManager.sceneCount-1; i > 0; i--) {
        awaitables.Add(AsTask(SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i))));
      }
      await Task.WhenAll(awaitables);
    }

    public override Task<List<FusionMenuOnlineRegion>> RequestAvailableOnlineRegionsAsync(FusionMenuConnectArgs connectArgs) {
      // Force best region
      return Task.FromResult(new List<FusionMenuOnlineRegion>(){new FusionMenuOnlineRegion(){Code = string.Empty, Ping = 0}});
    }

    public void SetSessionUsernames(List<string> usernames) {
      _usernames = usernames;
    }

    private GameMode ResolveGameMode(FusionMenuConnectArgs args) {
      bool isSharedSession = args.Scene.SceneName.Contains("Shared");
      if (args.Creating) {
        // Create session
        return isSharedSession ? GameMode.Shared : GameMode.Host;
      }

      if (string.IsNullOrEmpty(args.Session)) {
        // QuickJoin
        return isSharedSession ? GameMode.Shared : GameMode.AutoHostOrClient;
      }

      // Join session
      return isSharedSession ? GameMode.Shared : GameMode.Client;
    }

    private ShutdownReason ResolveShutdownReason(int reason) {
      switch (reason) {
        case ConnectFailReason.UserRequest:
          return ShutdownReason.Ok;
        case ConnectFailReason.ApplicationQuit:
          return ShutdownReason.Ok;
        case ConnectFailReason.Disconnect:
          return ShutdownReason.DisconnectedByPluginLogic;
        default:
          return ShutdownReason.Error;
      }
    }

    private int ResolveConnectFailReason(ShutdownReason reason) {
      switch (reason) {
        case ShutdownReason.Ok:
        case ShutdownReason.OperationCanceled:
          return ConnectFailReason.UserRequest;
        case ShutdownReason.DisconnectedByPluginLogic:
        case ShutdownReason.Error:
          return ConnectFailReason.Disconnect;
        default:
          return ConnectFailReason.None;
      }
    }

    private NetworkRunner CreateRunner() {
      var runner = new GameObject("NetworkRunner", typeof(NetworkRunner)).GetComponent<NetworkRunner>();
      return runner;
    }

    private FusionAppSettings CopyAppSettings(FusionMenuConnectArgs connectArgs) {
      FusionAppSettings appSettings = new FusionAppSettings();
      PhotonAppSettings.Global.AppSettings.CopyTo(appSettings);
      appSettings.FixedRegion = _region = connectArgs.Region;
      appSettings.AppVersion = _appVersion = connectArgs.AppVersion;
      return appSettings;
    }
  }
#endif
}
