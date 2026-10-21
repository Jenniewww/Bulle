using Fusion;
using UnityEngine;

namespace FusionIntroShared {
  /// <summary>
  /// A simple networked player movement class for shared mode.
  /// </summary>
  [RequireComponent(typeof(CharacterController))]
  public class PlayerMovement : NetworkBehaviour {
    private CharacterController _cc;

    public float MovementSpeed = 5f;
    public float RotationSpeed = 15.0f;

    public override void Spawned() {
      _cc = GetComponent<CharacterController>();
    }

    public override void FixedUpdateNetwork() {
      var input = IntroInput.GetMovement();
      var movementDirection = new Vector3(input.x, 0f, input.y);

      // Normalize so that walking diagonally at the same speed
      movementDirection.Normalize();

      // Use Runner.DeltaTime in FixedUpdateNetwork instead of Time.DeltaTime/FixedDeltaTime
      _cc.Move(movementDirection * MovementSpeed * Runner.DeltaTime);
      if (movementDirection != Vector3.zero) {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movementDirection), RotationSpeed * Runner.DeltaTime);
      }
    }
  }
}