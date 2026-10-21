namespace Fusion {
  using System;
  using System.Reflection;

  /// <summary>
  /// The default implementation of <see cref="INetworkObjectProvider"/>. Uses <see cref="NetworkRunner.Prefabs"/> to acquire prefab instances. Pooling is not implemented by default, but
  /// can be implemented in <see cref="InstantiatePrefab"/> virtual method.
  /// </summary>
  public class NetworkObjectProviderDefault : Fusion.Behaviour, INetworkObjectProvider {
    /// <summary>
    /// If enabled, the provider will delay acquiring a prefab instance if the scene manager is busy.
    /// </summary>
    [InlineHelp]
    public bool DelayIfSceneManagerIsBusy = true;
    
    /// <summary>
    /// If <see cref="NetworkObjectAcquireContext.TypeId"/> points to a scene object, returns <see cref="NetworkObjectAcquireContext.AttachableInstance"/>.
    /// Otherwise, uses <see cref="NetworkRunner.Prefabs"/> to go acquire a prefab and then calls <see cref="InstantiatePrefab"/>. 
    /// </summary>
    public virtual NetworkObjectAcquireResult AcquireInstance(NetworkRunner runner, in NetworkObjectAcquireContext context, out NetworkObject instance) {

      instance = null;

      if (DelayIfSceneManagerIsBusy && runner.SceneManager.IsBusy) {
        return NetworkObjectAcquireResult.Retry;
      }

      if (context.TypeId.IsSceneObject) {
        if (context.AttachableInstance) {
          instance = context.AttachableInstance;
          return NetworkObjectAcquireResult.Success;
        } else {
          // the scene will be loaded, eventually
          return NetworkObjectAcquireResult.Retry;
        }
      }
      
      Assert.Check(context.AttachableInstance == null, "AttachableInstance is not supported for prefabs");

      if (!context.TypeId.IsPrefab) {
        return NetworkObjectAcquireResult.Failed;
      }
      
      if (IsAcquirePrefabInstanceOverriden()) {
#pragma warning disable CS0618 // Type or member is obsolete
        return AcquirePrefabInstance(runner, new NetworkPrefabAcquireContext(context), out instance);
#pragma warning restore CS0618 // Type or member is obsolete
      }

      try {
        instance = CreatePrefabInstance(runner, context.TypeId.AsPrefabId, context.IsSynchronous, context.DontDestroyOnLoad);
      } catch (Exception ex) {
        Log.Error($"Failed to load prefab: {ex}");
        return NetworkObjectAcquireResult.Failed;
      }

      return instance ? NetworkObjectAcquireResult.Success : NetworkObjectAcquireResult.Retry;
    }
    
    /// <summary>
    /// Uses <see cref="NetworkRunner.Prefabs"/> to load a prefab instance and then calls <see cref="InstantiatePrefab"/> to create an instance.
    /// </summary>
    protected NetworkObject CreatePrefabInstance(NetworkRunner runner, NetworkPrefabId prefabId, bool synchronous, bool dontDestroyOnLoad) {
      var prefabs = runner.Prefabs;
      
      var prefab = prefabs.Load(prefabId, isSynchronous: synchronous);
      if (!prefab) {
        // this is ok, as long as Fusion does not require the prefab to be loaded immediately;
        // if an instance for this prefab is still needed, this method will be called again next update
        return null;
      }

      var instance = InstantiatePrefab(runner, prefab);
      Assert.Check(instance);

      if (dontDestroyOnLoad) {
        runner.MakeDontDestroyOnLoad(instance.gameObject);
      } else {
        runner.MoveToRunnerScene(instance.gameObject);
      }

      prefabs.AddInstance(prefabId);
      return instance;
    }
    
    /// <summary>
    /// If <see cref="NetworkObjectReleaseContext.IsBeingDestroyed"/> is false, calls one of Destroy methods, depending what type of object is being dealt with. To implement pooling, it may
    /// be better to override Destroy methods instead.
    /// </summary>
    public virtual void ReleaseInstance(NetworkRunner runner, in NetworkObjectReleaseContext context) {
      var instance = context.Object;

      if (!context.IsBeingDestroyed) {
        switch (context.TypeId.Kind) {
          case NetworkTypeIdKind.Prefab:
            DestroyPrefabInstance(runner, context.TypeId.AsPrefabId, instance);
            break;
          case NetworkTypeIdKind.PrefabNested:
            var (prefab, index) = context.TypeId.AsNestedPrefabId;
            DestroyPrefabNestedObject(runner, prefab, index, instance);
            break;
          case NetworkTypeIdKind.SceneObject:
            DestroySceneObject(runner, context.TypeId.AsSceneObjectId, instance);
            break;
          default:
            throw new NotImplementedException($"Unknown type id {context.TypeId}");
        }
      }

      if (context.TypeId.IsPrefab) {
        runner.Prefabs.RemoveInstance(context.TypeId.AsPrefabId);
      }
    }

    void INetworkObjectProvider.Shutdown(NetworkRunner runner) {
      var prefabs = runner.Prefabs;
      if (prefabs?.Options.UnloadUnusedPrefabsOnShutdown == true) {
        prefabs.UnloadUnreferenced(includeIncompleteLoads: true);
      }
    }
    
    /// <summary>
    /// Uses <see cref="NetworkRunner.Prefabs"/> to translate <see cref="NetworkObjectGuid"/> to <see cref="NetworkPrefabId"/>.
    /// </summary>
    public NetworkPrefabId GetPrefabId(NetworkRunner runner, NetworkObjectGuid prefabGuid) {
      return runner.Prefabs.GetId(prefabGuid);
    }

    /// <summary>
    /// Uses <see cref="NetworkRunner.Prefabs"/> to translate prefab name to <see cref="NetworkPrefabId"/>.
    /// </summary>
    public NetworkPrefabId GetPrefabId(NetworkRunner runner, string prefabName) {
      return runner.Prefabs.GetId(prefabName);
    }
    
    /// <summary>
    /// Calls <see cref="UnityEngine.Object.Instantiate(UnityEngine.Object)"/>. Override to alter this behaviour.
    /// </summary>
    protected virtual NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab) {
      return Instantiate(prefab);
    }

    /// <summary>
    /// Calls <see cref="UnityEngine.Object.Destroy(UnityEngine.Object)"/>. Override to alter this behaviour.
    /// </summary>
    protected virtual void DestroyPrefabInstance(NetworkRunner runner, NetworkPrefabId prefabId, NetworkObject instance) {
      Destroy(instance.gameObject);
    }
    
    /// <summary>
    /// Calls <see cref="UnityEngine.Object.Destroy(UnityEngine.Object)"/>. Override to alter this behaviour.
    /// </summary>
    protected virtual void DestroyPrefabNestedObject(NetworkRunner runner, NetworkPrefabId prefabId, int index, NetworkObject instance) {
#pragma warning disable CS0618 // Type or member is obsolete
      DestroyPrefabNestedObject(runner, instance);
#pragma warning restore CS0618 // Type or member is obsolete
    }
    
    /// <summary>
    /// Calls <see cref="UnityEngine.Object.Destroy(UnityEngine.Object)"/>. Override to alter this behaviour.
    /// </summary>
    [Obsolete("Use the overload with prefabId and index instead")]
    protected virtual void DestroyPrefabNestedObject(NetworkRunner runner, NetworkObject instance) {
      Destroy(instance.gameObject);
    }

    /// <summary>
    /// Calls <see cref="UnityEngine.Object.Destroy(UnityEngine.Object)"/>. Override to alter this behaviour.
    /// </summary>
    protected virtual void DestroySceneObject(NetworkRunner runner, NetworkSceneObjectId sceneObjectId, NetworkObject instance) {
      Destroy(instance.gameObject);
    }
    
    /// <summary>
    /// Legacy API implementation.
    /// </summary>
    [Obsolete("Override AcquireInstance instead. It deals with scene objects as well, due to context.AttachableInstance field.")]
    public virtual NetworkObjectAcquireResult AcquirePrefabInstance(NetworkRunner runner, in NetworkPrefabAcquireContext context, out NetworkObject instance) {
      if (DelayIfSceneManagerIsBusy && runner.SceneManager.IsBusy) {
        instance = null;
        return NetworkObjectAcquireResult.Retry;
      }
      
      try {
        instance = CreatePrefabInstance(runner, context.PrefabId, context.IsSynchronous, context.DontDestroyOnLoad);
      } catch (Exception ex) {
        Log.Error($"Failed to load prefab: {ex}");
        instance = null;
        return NetworkObjectAcquireResult.Failed;
      }

      return instance ? NetworkObjectAcquireResult.Success : NetworkObjectAcquireResult.Retry;
    }
    
    bool? _isAcquirePrefabInstanceOverriden;
    
    private bool IsAcquirePrefabInstanceOverriden() {
      if (_isAcquirePrefabInstanceOverriden != null) {
        return _isAcquirePrefabInstanceOverriden.Value;
      }
      
      var method = GetType().GetMethod(nameof(AcquirePrefabInstance), BindingFlags.Public | BindingFlags.Instance);
      Assert.Check(method != null);
      _isAcquirePrefabInstanceOverriden = method.DeclaringType != typeof(NetworkObjectProviderDefault);
      
      return _isAcquirePrefabInstanceOverriden.Value;
    }
  }
}