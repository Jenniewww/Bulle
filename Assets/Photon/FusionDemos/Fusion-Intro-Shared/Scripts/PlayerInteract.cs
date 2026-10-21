using System;
using Fusion;
using UnityEngine;

namespace FusionIntroShared {
  /// <summary>
  /// Class responsible for making the player pick up or deposit a color.
  /// </summary>
  public class PlayerInteract : NetworkBehaviour {
    
    // Interact sphere radius.
    public float InteractRadius = 1.25f;
    
    // Collider array to store the interaction overlap query result.
    private Collider[] _interactQueryResult = new Collider[5];
    
    public void Update() {
      
      // This check is needed in Update but not in FixedUpdateNetwork (FixedUpdateNetwork is only executed on the StateAuthority).
      // It checks that only the client owning this object (the StateAuthority) runs this code.
      if (Object.HasStateAuthority == false) return;

      if (IntroInput.GetInteractPressed()) {
        Interact();
      }
    }

    private void Interact() {
      
      var count = Physics.OverlapSphereNonAlloc(transform.position + transform.forward * 1.5f, InteractRadius, _interactQueryResult);
      // For each hit detected, if the object implements IInteractable interface call the interact object for the first one detected.
      if (count > 0) {
        for (int i = 0; i < count && i < _interactQueryResult.Length; i++) {
          
          if (_interactQueryResult[i].TryGetComponent<IInteractable>(out var interactable)) {
            interactable.Interact(Object);
          
            // Make sure to only interact with one object.
            break;
          }
        }
      }
    }
    
  }
}