using Fusion;
using Fusion.Photon.Realtime;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FusionIntroShared {
  public class SimpleStartMenu : MonoBehaviour{
    bool _gameStarting = false;

    public void StartGameSingle() {
      StartGame(GameMode.Single);
    }
  
    public void StartGameShared() {
      CheckHasAppId();
      StartGame(GameMode.Shared);
    }

    private void CheckHasAppId() {
      if (string.IsNullOrEmpty(PhotonAppSettings.Global.AppSettings.AppIdFusion)) {
#if  UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();   
#endif
        throw new Exception("App ID is needed for online play. Return to the Photon Hub (Tools > Fusion > Fusion Hub in the Unity toolbar on the top) and follow the setup steps.");
      }
    }

    private async void StartGame(GameMode gameMode) {
      if (!_gameStarting) {
        _gameStarting = true;
        gameObject.SetActive(false); // we don't need the menu now

        var runnerGameObject = new GameObject("NetworkRunner");
        var runner = runnerGameObject.AddComponent<NetworkRunner>();

        // When starting the runner the scene in which it will start needs to be provided.
        // In this example the runner is started in the currently active scene.
        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex));

        var startArguments = new StartGameArgs() {
          GameMode = gameMode,
          SessionName = "",
          Scene = sceneInfo,
        };

        var startTask = runner.StartGame(startArguments);
        await startTask;

        if (!startTask.Result.Ok) {
          gameObject.SetActive(true); // re-enable the menu to try again
          Debug.LogError($"Connection Failed: {startTask.Result.ShutdownReason}");
        }
        _gameStarting = false;
      }
    }
  }
}

