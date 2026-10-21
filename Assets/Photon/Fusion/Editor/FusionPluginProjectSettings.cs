namespace Fusion.Editor {
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.Text.RegularExpressions;
  using System.Xml.Linq;
  using JetBrains.Annotations;
  using UnityEditor;
  using UnityEditor.Callbacks;
  using UnityEditor.Compilation;
  using UnityEditor.SceneManagement;
  using UnityEngine;
  using UnityEngine.Events;
  using UnityEngine.SceneManagement;
  using UnityEngine.Serialization;
  using Assembly = System.Reflection.Assembly;
  using Debug = UnityEngine.Debug;

  [CreateAssetMenu(menuName = "Fusion/Plugin Project Settings")]
  [FusionGlobalScriptableObject("Assets/Photon/Fusion/Resources/PluginProjectSettings.asset")]
  public partial class FusionPluginProjectSettings : FusionGlobalScriptableObject<FusionPluginProjectSettings> {

    const string CustomPluginProjectName = "Fusion.Plugin.Types";
    const string PreviousPidKey = "PhotonServerPID";

    /// <summary>
    /// Label used to mark assets or scripts to be included in the plugin.
    /// </summary>
    public const string IncludeLabel = "FusionPluginInclude";

    /// <summary>
    /// The name of the partial project file, used to point to scripts in the Unity project.
    /// </summary>
    public const string PartialProjectName = "UnityProjectLinks.props";

    const string GeneratedSourceFilesPrefix = "UserTypes.";
    const string DBFilesPrefix = "DB.";


    /// <summary>
    /// Path to the Fusion Plugin SDK. Relative to the project root. Environment variables can be used (%NAME%).
    /// </summary>
    [Header("Plugin SDK")]
    [InlineHelp]
    [DirectoryPath]
    public string PluginSdkPath = "../FusionPluginSdk";

    /// <summary>
    /// The plugin needs to know about simulation types used on Unity side. Code export converts all user networked
    /// types to a plugin-compatible version with matching memory layout.
    ///
    /// Assemblies to be excluded when exporting network types. Use * as a wildcard.
    /// </summary>
    [Space]
    [InlineHelp]
    public List<string> IgnoredAssemblies = new() { "*.Tests" };

    /// <summary>
    /// Additional files and folders can either be marked with <c>FusionPluginInclude</c> label or included here. They will
    /// be added to the project file.
    /// </summary>
    [InlineHelp]
    public string[] IncludePaths = Array.Empty<string>();

    /// <summary>
    /// Auto export settings. Due to API limitations this is not fully automatic, most notably for scenes. When run in batch mode, call <see cref="ExportPrefabsAndScenes"/> to ensure exported data
    /// is fully in sync.
    /// </summary>
    [InlineHelp]
    [Space]
    [WarnIf(nameof(IsGlobal), false, "Only settings with " + nameof(FusionGlobalScriptableObjectUtils.GlobalAssetLabel) + " label is used for auto-exporting")]
    public FusionPluginAutoExportFlags AutoExport;

    /// <summary>
    /// Serialization callbacks. Create a scriptable object with matching methods.
    /// </summary>
    [InlineHelp]
    [SpaceAfter]
    [Space]
    public FusionPluginExportCallbacks ExportCallbacks = new();

    /// <summary>
    /// Attempts to get the global instance, if exists.
    /// </summary>
    /// <param name="global"></param>
    /// <returns></returns>
    public static bool TryGetGlobal(out FusionPluginProjectSettings global) {
      return TryGetGlobalInternal(out global);
    }

    /// <summary>
    /// Exports code and data.
    /// </summary>
    [EditorButton("Export", priority: 10)]
    public void ExportAll() {
      ExportCode();
      ExportPrefabsAndScenes();
    }

    /// <summary>
    /// Exports code.
    /// </summary>
    [EditorButton("Export/Export Code", priority: 11)]
    public void ExportCode() {
      var result = ExportCode(assemblyNames: null, removeMissingFiles: true);
      if (result != null) {
        FusionEditorLog.LogPlugin($"Exported user types: {result}");
      }
    }

    /// <summary>
    /// Exports data.
    /// </summary>
    [EditorButton("Export/Export Prefabs and Scenes", priority: 12)]
    public void ExportPrefabsAndScenes() {
      var path = GetValidGeneratedFolderPathOrThrow();
      var result = ExportAllScenesAndPrefabs(path);
      if (result != null) {
        FusionEditorLog.LogPlugin($"Exported data: {result}");
      }
    }

    /// <summary>
    /// Builds a Release version and runs the Photon Server.
    /// </summary>
    [EditorButton("Build and Run", priority: 20)]
    public void BuildAndRun() {
      ShutDownPhotonServer();
      BuildRelease();
      StartPhotonServer();
    }

    /// <summary>
    /// Builds a Debug version.
    /// </summary>
    [EditorButton("Build and Run/Build and Run (Debug)", priority: 21)]
    public void BuildAndRunDebug() {
      ShutDownPhotonServer();
      BuildDebug();
      StartPhotonServer();
    }

    /// <summary>
    /// Builds a Debug version.
    /// </summary>
    [EditorButton("Build and Run/Build (Debug)", priority: 30)]
    public void BuildDebug() {
      RunDotnetProcess($"build {GetValidSdkPathOrThrow()} -c Debug");
    }


    /// <summary>
    /// Builds a Release version.
    /// </summary>
    [EditorButton("Build and Run/Build (Release)", priority: 31)]
    public void BuildRelease() {
      RunDotnetProcess($"build {GetValidSdkPathOrThrow()} -c Release");
    }

    /// <summary>
    /// Runs the Photon.Server.
    /// </summary>
    [EditorButton("Build and Run/Run", priority: 32)]
    public void Run() {
      ShutDownPhotonServer();
      StartPhotonServer();
    }

    /// <summary>
    /// Opens the plugin project. It can be run and debug from the default IDE.
    /// </summary>
    [EditorButton("Open Plugin Project", priority: 40)]
    public void OpenProject() {
      Process.Start(new ProcessStartInfo() { FileName = Path.Combine(GetValidSdkPathOrThrow(), "Fusion.Plugin.Custom.sln"), UseShellExecute = true });
    }


    /// <summary>
    /// Exports data from a single scene.
    /// </summary>
    /// <param name="scene"></param>
    string ExportScene(Scene scene) {
      var exporter = new FusionPluginAssetExporter();
      var data = exporter.CaptureScene(scene);
      ExportCallbacks.PostprocessSceneData.Invoke(scene, data);
      var filePath = GetSceneDataPath(GetValidGeneratedFolderPathOrThrow(), data.UnityAssetGuid);
      WriteOutput(filePath, data);
      return filePath;
    }

    /// <summary>
    /// Exports data from a single prefab.
    /// </summary>
    public string ExportPrefab(NetworkObject prefab) {
      var exporter = new FusionPluginAssetExporter();
      var data = exporter.CapturePrefab(prefab);
      ExportCallbacks.PostprocessPrefabData.Invoke(prefab, data);
      var filePath = GetPrefabDataPath(GetValidGeneratedFolderPathOrThrow(), data.UnityAssetGuid);
      WriteOutput(filePath, data);
      return filePath;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="scriptableObject"></param>
    /// <returns></returns>
    public string ExportScriptableObject(ScriptableObject scriptableObject) {
      var exporter = new FusionPluginAssetExporter();
      var data = exporter.CaptureAsset(scriptableObject);
      ExportCallbacks.PostprocessScriptableObjectData.Invoke(scriptableObject, data);
      var filePath = GetScriptableObjectDataPath(GetValidGeneratedFolderPathOrThrow(), data.UnityAssetGuid, data.UnityFileId);
      WriteOutput(filePath, data);
      return filePath;
    }

    /// <summary>
    /// Exports code from all or from selected assemblies.
    /// </summary>
    /// <param name="assemblyNames">Optional. If specified, only those assemblies will be scanned and exported.</param>
    /// <param name="removeMissingFiles">If true, remove all files exported from assemblies that are not in <paramref name="assemblyNames"/></param>
    public FusionPluginCodeExportSummary ExportCode([CanBeNull] string[] assemblyNames, bool removeMissingFiles = true) {
      var outputFolder = GetValidGeneratedFolderPathOrThrow();

      var stopwatch = Stopwatch.StartNew();

      var exporter = new FusionPluginCodeExporter(FusionPluginCodeExporter.Options.AddJsonNETAttributes);

      var filters = IgnoredAssemblies
        .Select(x => Regex.Escape(x).Replace("\\*", ".*?"))
        .Select(x => new Regex(x)).ToArray();

      Predicate<Assembly> filter = x => !filters.Any(f => f.IsMatch(x.FullName)) &&
                                        (assemblyNames == null || assemblyNames.Length == 0 || Array.IndexOf(assemblyNames, x.Location) >= 0);

      HashSet<string> generatedFilesToRemove = null;

      if (removeMissingFiles) {
        generatedFilesToRemove = new HashSet<string>(Directory.GetFiles(outputFolder, $"{GeneratedSourceFilesPrefix}*.cs", SearchOption.TopDirectoryOnly));
      }

      int filesGeneratedCount = 0;

      foreach (var (scope, contents) in exporter.Export(outputFolder, filter)) {
        var userTypesPath = Path.Combine(outputFolder, $"{GeneratedSourceFilesPrefix}{scope}.cs");
        File.WriteAllText(userTypesPath, contents);
        FusionEditorLog.TracePlugin($"Exported assembly {scope} types to {userTypesPath}");
        generatedFilesToRemove?.Remove(userTypesPath);
        ++filesGeneratedCount;
      }

      // now export the project file
      var linkPath = Path.Combine(outputFolder, PartialProjectName);
      File.WriteAllText(linkPath, CreateUnityProjectLinks(Path.GetDirectoryName(outputFolder)!, IncludeLabel, IncludePaths));
      FusionEditorLog.TracePlugin($"Exported partial project file to {linkPath}");

      if (generatedFilesToRemove?.Count > 0) {
        foreach (var path in generatedFilesToRemove) {
          FusionEditorLog.TracePlugin($"Deleting outdated {path}");
          File.Delete(path);
        }
      }

      return new FusionPluginCodeExportSummary(stopwatch.Elapsed, filesGeneratedCount, generatedFilesToRemove?.Count ?? 0);
    }

    static bool ShouldExportScene(string scenePath) {
      var buildIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);

      if (buildIndex >= 0 && buildIndex < EditorBuildSettings.scenes.Length && EditorBuildSettings.scenes[buildIndex].enabled) {
        return true;
      }

      var sceneGuid = AssetDatabase.AssetPathToGUID(scenePath);
      if (string.IsNullOrEmpty(sceneGuid)) {
        return false;
      }

      var sceneAddress = AssetDatabaseUtils.GetAddress(sceneGuid);
      if (string.IsNullOrEmpty(sceneAddress)) {
        return false;
      }

      return true;
    }

    public FusionPluginExportDataSummary ExportAllScenesAndPrefabs(string outputFolder) {

      SceneSetup[] scenesToRestore = null;

      // scenes may be included based on their build index or address
      List<string> scenes = AssetDatabaseUtils.IterateAssets<SceneAsset>()
        .Select(x => AssetDatabase.GetAssetPath(x.pptrValue))
        .Where(ShouldExportScene)
        .ToList();

      if (scenes.Any()) {
        // close scene if user wants to
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
          return null;
        }

        // store all laoded scenes
        scenesToRestore = EditorSceneManager.GetSceneManagerSetup().ToArray();
      }

      var stopwatch = Stopwatch.StartNew();

      // first go through assets and prefabs

      var exporter = new FusionPluginAssetExporter();
      var filesToRemove = new HashSet<string>(Directory.GetFiles(outputFolder, $"{DBFilesPrefix}*.json"));

      int prefabCount = 0;
      int sceneCount = 0;
      int assetCount = 0;

      try {

        // first make sure all the labeled non-networked assets are exported
        var assetsMarkedWithLabels = AssetDatabaseUtils.IterateAssets<ScriptableObject>(label: IncludeLabel).Select(x => (ScriptableObject)x.pptrValue).ToList();
        // how about types?
        var assetsMarkedByAttributes = TypeCache.GetTypesWithAttribute<PluginAssetExportSettingsAttribute>()
          .Where(x => x.GetCustomAttribute<PluginAssetExportSettingsAttribute>(true).Options == PluginExportOptions.Export)
          .Where(x => x.IsSubclassOf(typeof(ScriptableObject)))
          .SelectMany(x => AssetDatabaseUtils.IterateAssets(type: x).Select(it => (ScriptableObject)it.pptrValue))
          .ToList();

        foreach (var obj in assetsMarkedWithLabels.Concat(assetsMarkedByAttributes).Distinct()) {
          UpdateProgressBar($"Exporting Asset {obj.name}...");
          var data = exporter.CaptureAsset(obj);
          ExportCallbacks.PostprocessScriptableObjectData.Invoke(obj, data);
          var guidFileId = AssetDatabaseUtils.GetGUIDAndLocalFileIdentifierOrThrow(obj);
          WriteOutputAndUpdateFileList(GetScriptableObjectDataPath(outputFolder, guidFileId.GuidStr, guidFileId.FileId), data);
          ++assetCount;
        }

        // now prefabs
        foreach (var obj in AssetDatabaseUtils.IterateAssets<GameObject>(label: NetworkProjectConfigImporter.FusionPrefabTag)
                   .Select(x => (GameObject)x.pptrValue)
                   .Select(x => x.GetComponent<NetworkObject>())
                   .Where(x => x)) {
          UpdateProgressBar($"Exporting Prefab {obj.name}...");
          var data = exporter.CapturePrefab(obj);
          ExportCallbacks.PostprocessPrefabData.Invoke(obj, data);
          WriteOutputAndUpdateFileList(GetPrefabDataPath(outputFolder, data.UnityAssetGuid), data);
          ++prefabCount;
        }

        foreach (var scenePath in scenes) {
          UpdateProgressBar($"Exporting Scene {scenePath}...");
          var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
          var data = exporter.CaptureScene(scene);
          ExportCallbacks.PostprocessSceneData.Invoke(scene, data);
          WriteOutputAndUpdateFileList(GetSceneDataPath(outputFolder, data.UnityAssetGuid), data);
          ++sceneCount;
        }

        if (filesToRemove.Any()) {
          foreach (var path in filesToRemove) {
            try {
              UpdateProgressBar($"Deleting {path}...");
              File.Delete(path);
            } catch (Exception ex) {
              FusionEditorLog.ErrorPlugin($"Failed to delete {path}: {ex}");
            }
          }
        }
      } finally {
        EditorUtility.ClearProgressBar();

        // restore settings
        if (scenesToRestore?.Length > 0) {
          EditorSceneManager.RestoreSceneManagerSetup(scenesToRestore);
        }
      }

      return new(stopwatch.Elapsed, prefabCount, sceneCount, assetCount);


      void WriteOutputAndUpdateFileList(string path, object obj) {
        WriteOutput(path, obj);
        filesToRemove.Remove(path);
      }

      void UpdateProgressBar(string info) {
        FusionEditorLog.TracePlugin($"Progress: {info}");

        if (EditorUtility.DisplayCancelableProgressBar("Exporting Network Object DB", info, -1)) {
          throw new OperationCanceledException();
        }
      }
    }

    static string GetPrefabDataPath(string outputFolder, string prefabGuid) {
      return Path.Combine(outputFolder, $"{DBFilesPrefix}Prefab.{prefabGuid}.json");
    }

    static string GetSceneDataPath(string outputFolder, string sceneGuid) {
      return Path.Combine(outputFolder, $"{DBFilesPrefix}Scene.{sceneGuid}.json");
    }

    static string GetScriptableObjectDataPath(string outputFolder, string guid, long fileId) {
      return Path.Combine(outputFolder, $"{DBFilesPrefix}ScriptableObject.{guid}-{fileId}.json");
    }

    static string[] GetScriptableObjectDataFiles(string outputFolder, string guid) {
      return Directory.GetFiles(outputFolder, $"{DBFilesPrefix}ScriptableObject.{guid}-*.json");
    }


    string WriteOutput(string path, object obj) {
      var json = JsonUtility.ToJson(obj, true);
      File.WriteAllText(path, json);
      return path;
    }

    void StartPhotonServer() {
      var arguments = "/run LoadBalancing";
      var path = Path.Combine(GetValidSdkPathOrThrow(), "Photon.Server", "bin");
      var photonServer = Path.Combine(path, "PhotonServer.exe");
      FusionEditorLog.LogPlugin($"Launching Photon Server at: {photonServer} {arguments}");

      var startInfo = new ProcessStartInfo() {
        FileName         = "PhotonServer.exe",
        Arguments        = arguments,
        WorkingDirectory = path
      };

      var process = Process.Start(startInfo);

      if (process?.HasExited == false) {
        SessionState.SetInt(PreviousPidKey, process!.Id);
      }
    }

    static void ShutDownPhotonServer() {

      var previousProcessPid = SessionState.GetInt(PreviousPidKey, default);

      if (previousProcessPid != 0) {
        SessionState.EraseInt(PreviousPidKey);

        try {
          var previousProcess = Process.GetProcessById(previousProcessPid);
          previousProcess.Kill();
          WaitForProcessToExit(previousProcess, "Shutting down Photon.Server", $"Waiting for process {previousProcessPid} to shut down");
        } catch {
          // ignore all the errors
        }
      }
    }

    static string CreateUnityProjectLinks(string referencePath, string includeLabel, IEnumerable<string> includePaths) {
      var includes = new List<string>();

      // add any file marked with a label
      if (!string.IsNullOrEmpty(includeLabel)) {
        var labeledAssets = AssetDatabase.FindAssets($"l:{IncludeLabel}")
          .Select(AssetDatabase.GUIDToAssetPath);
        includes.AddRange(labeledAssets);
      }

      // now remove duplicates and files otherwise included explicitly
      includes = includes.Distinct().ToList();

      // add explicit paths
      foreach (var path in includePaths) {
        if (Directory.Exists(path)) {
          // remove any path that starts with this folder
          var dirWithTrailingSlash = path.TrimEnd('/') + "/";
          includes.RemoveAll(x => x.StartsWith(dirWithTrailingSlash));
        }

        includes.Add(path);
      }

      // turn current path into path relative to outputPath
      return GenerateCsProjInclude(
        Path.GetRelativePath(referencePath, Environment.CurrentDirectory),
        includes.ToArray()
      );
    }

    static string GenerateCsProjInclude(string pathPrefix, string[] includes) {

      pathPrefix = PathUtils.Normalize(pathPrefix);
      var projectElement = new XElement("Project");

      projectElement.Add(new XComment("Properties"));
      var properties = new XElement("PropertyGroup");
      projectElement.Add(properties);

      projectElement.Add(new XComment("Includes"));

      var group = new XElement("ItemGroup");

      foreach (var source in includes.Select(Environment.ExpandEnvironmentVariables)) {
        if (Directory.Exists(source)) {
          group.Add(new XElement("Compile",
            new XAttribute("Include", $"{pathPrefix}/{source}/**/*.cs"),
            new XAttribute("LinkBase", CreateLinkName(source))));
        } else if (Path.GetExtension(source) == ".cs") {
          group.Add(new XElement("Compile",
            new XAttribute("Include", $"{pathPrefix}/{source}"),
            new XElement("Link", CreateLinkName(source)))
          );
        }
      }

      projectElement.Add(group);

      var document = new XDocument(projectElement);
      return document.ToString();

      string CreateLinkName(string path) {
        // first of all, drop the Assets/ prefix, if present
        if (path.StartsWith("Assets/")) {
          path = path.Substring("Assets/".Length);
        }

        // now get rid of all "/../" as these will not work
        var parts = path.Split('/');

        for (var i = 0; i < parts.Length; ++i) {
          if (parts[i] != "..") {
            continue;
          }

          if (i == 0) {
            // just remove
            ArrayUtility.RemoveAt(ref parts, 0);
            i -= 1;
          } else {
            // remove this and the previous
            ArrayUtility.RemoveAt(ref parts, i);
            ArrayUtility.RemoveAt(ref parts, i - 1);
            i -= 2;
          }
        }

        return string.Join("/", parts);
      }
    }

    string GetValidSdkPathOrThrow() {
      var expanded = Environment.ExpandEnvironmentVariables(PluginSdkPath);

      if (string.IsNullOrEmpty(expanded)) {
        throw new InvalidOperationException($"Invalid Plugin SDK Path");
      }

      if (!Directory.Exists(expanded)) {
        throw new InvalidOperationException($"Plugin SDK Path does not exist");
      }

      return expanded;
    }

    string GetValidGeneratedFolderPathOrThrow() {
      var sdkPath = GetValidSdkPathOrThrow();
      var projectPath = Path.Combine(sdkPath, CustomPluginProjectName);

      if (!Directory.Exists(projectPath)) {
        throw new InvalidOperationException($"Expected to find {CustomPluginProjectName} in the Plugin SDK");
      }

      var generatedPath = Path.Combine(projectPath, "Generated");

      if (!Directory.Exists(generatedPath)) {
        Directory.CreateDirectory(generatedPath);
      }

      return generatedPath;
    }

    static void RunDotnetProcess(string arguments) {
#if UNITY_EDITOR_WIN
      var path = "dotnet";
#else
      // search paths are minimal without a login shell on Mac
      // likely the same for Linux
      var path = "sh";
      arguments = $" --login -c 'dotnet {arguments}'";
#endif
      var startInfo = new ProcessStartInfo() {
        FileName               = path,
        Arguments              = arguments,
        UseShellExecute        = false,
        RedirectStandardError  = true,
        RedirectStandardInput  = true,
        RedirectStandardOutput = true,
        CreateNoWindow         = true
      };

      var p = new Process() { StartInfo = startInfo };

      List<string> output = new();
      p.OutputDataReceived += (sender, args) => {
        output.Add(args.Data);
      };
      p.ErrorDataReceived += (sender, args) => {
        output.Add(args.Data);
      };

      p.Start();
      p.BeginErrorReadLine();
      p.BeginOutputReadLine();

      WaitForProcessToExit(p, "Executing", $"{startInfo.FileName} {startInfo.Arguments}");

      if (p.ExitCode != 0) {
        FusionEditorLog.ErrorPlugin($"{startInfo.FileName} {startInfo.Arguments} exited with {p.ExitCode}. Output:\n{string.Join("\n", output)}");
        throw new InvalidOperationException($"Process failed ({p.ExitCode}), check log for details");
      }

      FusionEditorLog.TracePlugin($"{startInfo.FileName} {startInfo.Arguments} output:\n{string.Join("\n", output)}");
    }

    static void WaitForProcessToExit(Process p, string title, string info) {
      try {
        while (p.WaitForExit(10) == false) {
          if (EditorUtility.DisplayCancelableProgressBar(title, info, -1)) {
            throw new OperationCanceledException();
          }
        }
      } finally {
        EditorUtility.ClearProgressBar();
      }
    }

    #region Code Auto-Export

    const string SessionStateKey = "FusionPluginProjectSettingsPendingAssemblies";

    [InitializeOnLoadMethod]
    static void StaticInitialize() {
      CompilationPipeline.assemblyCompilationFinished += (name, messages) => {
        var pendingAssemblies = SessionState.GetString(SessionStateKey, string.Empty);
        pendingAssemblies += $"{name};";
        SessionState.SetString(SessionStateKey, pendingAssemblies);
      };
    }

    [DidReloadScripts]
    static void ScriptsReloaded() {
      var pendingAssemblies = SessionState.GetString(SessionStateKey, string.Empty);
      SessionState.EraseString(SessionStateKey);

      if (!TryGetGlobalInternal(out var global) || (global.AutoExport & FusionPluginAutoExportFlags.Code) == 0) {
        // nothing to do
        return;
      }

      var assemblies = pendingAssemblies.Split(";", StringSplitOptions.RemoveEmptyEntries)
        .Select(Path.GetFullPath)
        .ToArray();

      try {
        FusionEditorLog.TracePlugin($"Auto-exporting assemblies: {string.Join(",", assemblies)}");
        global.ExportCode(assemblies, false);
      } catch (Exception ex) {
        FusionEditorLog.ErrorPlugin($"Failed to auto-export: {ex}");
      }
    }

    #endregion

    #region Prefabs and ScriptableObjects Auto-Export

    class PostProcessor : AssetPostprocessor {
      public override int GetPostprocessOrder() {
        return NetworkObjectPostprocessor.PostprocessOrder + 1000;
      }

      static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
        if (!TryGetGlobalInternal(out var global)) {
          // nothing to do
          return;
        }

        if ((global.AutoExport & FusionPluginAutoExportFlags.Prefabs) == FusionPluginAutoExportFlags.Prefabs) {
          foreach (var path in importedAssets) {
            if (!IsPrefabPath(path)) {
              continue;
            }

            if (AssetDatabaseUtils.HasLabel(path, NetworkProjectConfigImporter.FusionPrefabTag)) {
              ExportPrefabSafe(path);
            } else {
              RemovePrefabSafe(path);
            }
          }

          foreach (var path in movedAssets) {
            if (!IsPrefabPath(path) || !AssetDatabaseUtils.HasLabel(path, NetworkProjectConfigImporter.FusionPrefabTag)) {
              continue;
            }

            ExportPrefabSafe(path);
          }

          foreach (var path in deletedAssets) {
            if (!IsPrefabPath(path)) {
              continue;
            }

            RemovePrefabSafe(path);
          }
        }

        if ((global.AutoExport & FusionPluginAutoExportFlags.ScriptableObjects) == FusionPluginAutoExportFlags.ScriptableObjects) {
          foreach (var path in importedAssets) {
            if (!IsScriptableObject(path)) {
              continue;
            }

            if (AssetDatabaseUtils.HasLabel(path, IncludeLabel)) {
              SyncScriptableObjects(path, AssetDatabase.LoadAllAssetsAtPath(path).OfType<ScriptableObject>().ToArray());
            } else if (IsExportedWithAttribute(AssetDatabase.GetMainAssetTypeAtPath(path))) {
              SyncScriptableObjects(path, AssetDatabase.LoadAllAssetsAtPath(path).OfType<ScriptableObject>()
                .Where(x => IsExportedWithAttribute(x?.GetType()))
                .ToArray());
            } else {
              SyncScriptableObjects(path, Array.Empty<ScriptableObject>());
            }
          }

          foreach (var path in movedAssets) {
            if (!IsScriptableObject(path)) {
              continue;
            }

            if (AssetDatabaseUtils.HasLabel(path, IncludeLabel)) {
              SyncScriptableObjects(path, AssetDatabase.LoadAllAssetsAtPath(path).OfType<ScriptableObject>().ToArray());
            } else if (IsExportedWithAttribute(AssetDatabase.GetMainAssetTypeAtPath(path))) {
              SyncScriptableObjects(path, AssetDatabase.LoadAllAssetsAtPath(path).OfType<ScriptableObject>()
                .Where(x => IsExportedWithAttribute(x?.GetType()))
                .ToArray());
            }
          }

          foreach (var path in deletedAssets) {
            SyncScriptableObjects(path, Array.Empty<ScriptableObject>());
          }
        }


        static bool IsExportedWithAttribute([CanBeNull] Type type) {
          return type?.GetCustomAttribute<PluginAssetExportSettingsAttribute>(true)?.Options == PluginExportOptions.Export;
        }

        static bool IsPrefabPath(string path) {
          return path.EndsWith(".prefab");
        }

        static bool IsScriptableObject(string path) {
          return !path.EndsWith(".prefab") && !path.EndsWith(".unity");
        }

        void ExportPrefabSafe(string path) {
          var gameObject = (GameObject)AssetDatabase.LoadMainAssetAtPath(path);
          if (!gameObject) {
            FusionEditorLog.TracePlugin($"Not auto-exporting {path}: unable to translate {path} to guid");
            return;
          }

          var networkObject = gameObject.GetComponent<NetworkObject>();
          if (!networkObject) {
            FusionEditorLog.TracePlugin($"Not auto-exporting {path}: unable to load prefab");
            return;
          }

          FusionEditorLog.TracePlugin($"Auto-exporting prefab {path}");
          try {
            global.ExportPrefab(networkObject);
          } catch (Exception ex) {
            FusionEditorLog.ErrorPlugin($"Auto-export of {path} failed: {ex}");
          }
        }

        void RemovePrefabSafe(string path) {
          var guid = AssetDatabase.AssetPathToGUID(path);

          if (string.IsNullOrEmpty(guid)) {
            FusionEditorLog.TracePlugin($"Not auto-removing {path}: unable to translate {path} to guid");
            return;
          }

          try {
            var filePath = GetPrefabDataPath(global.GetValidGeneratedFolderPathOrThrow(), guid);
            if (File.Exists(filePath)) {
              FusionEditorLog.TracePlugin($"Auto-removing {path} from {filePath}");
              File.Delete(filePath);
            } else {
              FusionEditorLog.TracePlugin($"Not auto-removing {path}: {filePath} does not exist");
            }
          } catch (Exception ex) {
            FusionEditorLog.ErrorPlugin($"Auto-remove of {path} failed: {ex}");
          }
        }


        void SyncScriptableObjects(string path, ScriptableObject[] objects) {
          var guid = AssetDatabase.AssetPathToGUID(path);
          if (string.IsNullOrEmpty(guid)) {
            FusionEditorLog.TracePlugin($"Auto-exporting failed {path}: unable to translate {path} to guid");
          }

          HashSet<string> filesToRemove = new(GetScriptableObjectDataFiles(global.GetValidGeneratedFolderPathOrThrow(), guid));

          try {
            foreach (var obj in objects) {
              if (!obj) {
                continue;
              }

              FusionEditorLog.TracePlugin($"Auto-exporting scriptable object {path} ({obj.name})");
              filesToRemove.Remove(global.ExportScriptableObject(obj));
            }

            foreach (var toRemove in filesToRemove) {
              FusionEditorLog.TracePlugin($"Auto-removing scriptable object {path} from {toRemove}");
              File.Delete(toRemove);
            }
          } catch (Exception ex) {
            FusionEditorLog.ErrorPlugin($"Auto-export of {path} failed: {ex}");
          }
        }
      }
    }

    #endregion

    #region Scene Auto-Export

    [InitializeOnLoadMethod]
    static void RegisterSceneManagerListeners() {
      EditorSceneManager.sceneSaved += scene => {
        if (!TryGetGlobalInternal(out var global) || (global.AutoExport & FusionPluginAutoExportFlags.Scenes) == 0) {
          return;
        }

        // check if this scene is in build settings or addressable
        if (!ShouldExportScene(scene.path)) {
          FusionEditorLog.TracePlugin($"Ignoring scene {scene.path}: not enabled in build settings and not addressable");
          return;
        }

        FusionEditorLog.TracePlugin($"Auto-exporting scene {scene.path}");
        try {
          global.ExportScene(scene);
        } catch (Exception ex) {
          FusionEditorLog.ErrorPlugin($"Auto-exporting scene {scene.path} error: {ex}");
        }
      };
    }

    #endregion

    #region Menu Items

    const int MenuItemPriority = 2000;

    [MenuItem("Tools/Fusion/Plugin/Open C# Project", priority = MenuItemPriority)]
    static void MenuItemOpenProject() => GlobalInternal.OpenProject();

    [MenuItem("Tools/Fusion/Plugin/Export Prefabs", priority = MenuItemPriority + 40)]
    static void MenuExportPrefabs() => GlobalInternal.ExportPrefabsAndScenes();

    [MenuItem("Tools/Fusion/Plugin/Export Code", priority = MenuItemPriority + 41)]
    static void MenuExportScenes() => GlobalInternal.ExportCode();

    [MenuItem("Tools/Fusion/Plugin/Export All", priority = MenuItemPriority + 42)]
    static void MenuExportAll() => GlobalInternal.ExportAll();

    [MenuItem("Tools/Fusion/Plugin/Build and Run", priority = MenuItemPriority + 60)]
    static void MenuBuildAndRun() => GlobalInternal.BuildAndRun();
    [MenuItem("Tools/Fusion/Plugin/Build and Run (Debug)", priority = MenuItemPriority + 60)]
    static void MenuBuildAndRunDebug() => GlobalInternal.BuildAndRun();

    [MenuItem("Tools/Fusion/Plugin/Open C# Project", isValidateFunction: true)]
    [MenuItem("Tools/Fusion/Plugin/Export Prefabs", isValidateFunction: true)]
    [MenuItem("Tools/Fusion/Plugin/Export Code", isValidateFunction: true)]
    [MenuItem("Tools/Fusion/Plugin/Export All", isValidateFunction: true)]
    [MenuItem("Tools/Fusion/Plugin/Build and Run", isValidateFunction: true)]
    [MenuItem("Tools/Fusion/Plugin/Build and Run (Debug)", isValidateFunction: true)]
    static bool ValidateMenuItems() {
      return TryGetGlobalInternal(out var global);
    }

    #endregion
  }

  /// <summary>
  /// Auto-export settings for <see cref="FusionPluginProjectSettings"/>.
  /// </summary>
  [Flags]
  public enum FusionPluginAutoExportFlags {
    /// <summary>
    /// Export each Fusion prefab that gets updated/moved or remove any data left-over from a removed prefab.
    /// </summary>
    Prefabs = 1,

    /// <summary>
    /// Export scriptable objects.
    /// </summary>
    ScriptableObjects = 2,

    /// <summary>
    /// Exports active scene when it is being saved. Note that this will not cause any imported scenes to be exported automatically; a scene needs to be opened and saved for the export to happen.
    /// </summary>
    Scenes = 4,

    /// <summary>
    /// After each recompile, affected assemblies get reexported. Note that this might not work in batch mode, since [DidReloadScript] has to be run.
    /// </summary>
    Code = 8
  }

  /// <summary>
  /// A collection of callbacks invoked whenever an export happens. Make sure "Editor and Runtime" is chosen for each.
  /// </summary>
  [Serializable]
  public class FusionPluginExportCallbacks {
    /// <summary>
    /// Invoked whenever a prefab data is captured.
    /// </summary>
    [InlineHelp]
    public UnityEvent<NetworkObject, JsonNetworkObjectDB.PrefabData> PostprocessPrefabData;
    /// <summary>
    /// Invoked whenever a scene is captured.
    /// </summary>
    [InlineHelp]
    public UnityEvent<Scene, JsonNetworkObjectDB.SceneData> PostprocessSceneData;
    /// <summary>
    /// Invoked whenever an asset is captured.
    /// </summary>
    [InlineHelp]
    public UnityEvent<ScriptableObject, JsonNetworkObjectDB.ScriptableObjectData> PostprocessScriptableObjectData;
  }

  /// <summary>
  /// Summary of the plugin data export.
  /// </summary>
  public record FusionPluginExportDataSummary(TimeSpan Duration, int PrefabCount, int SceneCount, int AssetCount) {
    /// <summary/>
    public TimeSpan Duration { get; } = Duration;
    /// <summary/>
    public int PrefabCount { get; } = PrefabCount;
    /// <summary/>
    public int SceneCount { get; } = SceneCount;
    /// <summary/>
    public int AssetCount { get; } = AssetCount;
  }

  /// <summary>
  /// Summary of the plugin code export.
  /// </summary>
  public record FusionPluginCodeExportSummary(TimeSpan Duration, int FilesGenerated, int FilesRemoved) {
    /// <summary/>
    public TimeSpan Duration { get; } = Duration;
    /// <summary/>
    public int FilesGenerated { get; } = FilesGenerated;
    /// <summary/>
    public int FilesRemoved { get; } = FilesRemoved;
  }
}