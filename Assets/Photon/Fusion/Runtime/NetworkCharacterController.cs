namespace Fusion {
  using System.Runtime.CompilerServices;
  using System.Runtime.InteropServices;
  using UnityEngine;

  [StructLayout(LayoutKind.Explicit)]
  [NetworkStructWeaved(WORDS)]
  public unsafe struct NetworkCCData : INetworkStruct {
    public const int WORDS = 4;
    public const int SIZE  = WORDS * 4;

    [FieldOffset(0 * Allocator.REPLICATE_WORD_SIZE)]
    int _grounded;

    [FieldOffset(1 * Allocator.REPLICATE_WORD_SIZE)]
    Vector3Compressed _velocityData;

    public bool Grounded {
      get => _grounded == 1;
      set => _grounded = (value ? 1 : 0);
    }

    public Vector3 Velocity {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _velocityData;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => _velocityData = value;
    }
  }

  [DisallowMultipleComponent]
  [RequireComponent(typeof(CharacterController))]
  [NetworkBehaviourWeaved(NetworkTRSPData.WORDS + NetworkCCData.WORDS)]
  // ReSharper disable once CheckNamespace
  public sealed unsafe class NetworkCharacterController : NetworkTRSP, INetworkTRSPTeleport, IBeforeAllTicks, IAfterAllTicks, IBeforeCopyPreviousState {
    ref NetworkCCData CCData => ref ReinterpretState<NetworkCCData>(NetworkTRSPData.WORDS);

    [Header("Character Controller Settings")]
    public float gravity = -20.0f;
    public float jumpImpulse   = 8.0f;
    public float acceleration  = 10.0f;
    public float braking       = 10.0f;
    public float maxSpeed      = 2.0f;
    public float rotationSpeed = 15.0f;

    Tick                _initial;
    CharacterController _controller;

    public Vector3 Velocity {
      get => CCData.Velocity;
      set => CCData.Velocity = value;
    }

    public bool Grounded {
      get => CCData.Grounded;
      set => CCData.Grounded = value;
    }

    public void Teleport(Vector3? position = null, Quaternion? rotation = null) {
      _controller.enabled = false;
      NetworkTRSP.Teleport(this, transform, position, rotation);
      _controller.enabled = true;
    }


    public void Jump(bool ignoreGrounded = false, float? overrideImpulse = null) {
      if (CCData.Grounded || ignoreGrounded) {
        var newVel = CCData.Velocity;
        newVel.y      += overrideImpulse ?? jumpImpulse;
        CCData.Velocity =  newVel;
      }
    }

    public void Move(Vector3 direction) {
      var deltaTime    = Runner.DeltaTime;
      var previousPos  = transform.position;
      var moveVelocity = CCData.Velocity;

      direction = direction.normalized;

      if (CCData.Grounded && moveVelocity.y < 0) {
        moveVelocity.y = 0f;
      }

      moveVelocity.y += gravity * Runner.DeltaTime;

      var horizontalVel = default(Vector3);
      horizontalVel.x = moveVelocity.x;
      horizontalVel.z = moveVelocity.z;

      if (direction == default) {
        horizontalVel = Vector3.Lerp(horizontalVel, default, braking * deltaTime);
      } else {
        horizontalVel      = Vector3.ClampMagnitude(horizontalVel + direction * acceleration * deltaTime, maxSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Runner.DeltaTime);
      }

      moveVelocity.x = horizontalVel.x;
      moveVelocity.z = horizontalVel.z;

      _controller.Move(moveVelocity * deltaTime);

      CCData.Velocity = (transform.position - previousPos) * Runner.TickRate;
      CCData.Grounded = _controller.isGrounded;
    }
    
    public override void Spawned() {
      _initial = default;
      TryGetComponent(out _controller);
      
      // Without disabling and re-enabling the CharacterController here, the first Move call will reset the position to 0,0,0 instead of
      // keeping the position it was spawned at. Presumably disabling it clears some kind of internally cached "previous position" value
      _controller.enabled = false;
      
      // This object could already exist on shared mode, just out of interest. So we need to check if it has snapshots.
      if (Object.HasStateAuthority && Object.LastReceiveTick == default) {
        CopyToBuffer();
      } else {
        CopyToEngine();
      }
      
      _controller.enabled = true;
    }

    public override void Render() {
      NetworkTRSP.Render(this, transform, false, false, false, ref _initial);
    }

    void IBeforeAllTicks.BeforeAllTicks(bool resimulation, int tickCount) {
      CopyToEngine();
    }

    void IAfterAllTicks.AfterAllTicks(bool resimulation, int tickCount) {
      CopyToBuffer();
    }

    void IBeforeCopyPreviousState.BeforeCopyPreviousState() {
      CopyToBuffer();
    }
    
    void Awake() {
      TryGetComponent(out _controller);
    }

    void CopyToBuffer() {
      ref var state = ref State;
      state.Position = transform.position;
      state.Rotation = transform.rotation;
    }

    void CopyToEngine() {
      // CC must be disabled before resetting the transform state
      _controller.enabled = false;
      
      // set position and rotation
      ref var state = ref State;
      transform.SetPositionAndRotation(state.Position, state.Rotation);

      // Re-enable CC
      _controller.enabled = true;
    }
  }
}