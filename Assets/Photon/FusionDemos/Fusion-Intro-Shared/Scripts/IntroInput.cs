using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace FusionIntroShared {

  [RequireComponent(typeof(EventSystem))]
  class IntroInput : MonoBehaviour {

    void Awake() {
      if (GetComponent<BaseInputModule>() != null) return;

      //Switch the Input Module depending on which input system we're using
#if ENABLE_INPUT_SYSTEM
      gameObject.AddComponent<InputSystemUIInputModule>();
#elif ENABLE_LEGACY_INPUT_MANAGER
      gameObject.AddComponent<StandaloneInputModule>();
#endif
    }

    public static Vector2 GetMovement() {
#if ENABLE_INPUT_SYSTEM
      var v = Vector2.zero;
      var kb = Keyboard.current;
      if (kb != null) {
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) v.x += 1f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  v.x -= 1f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    v.y += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  v.y -= 1f;
      }
      v += Gamepad.current?.leftStick.ReadValue() ?? Vector2.zero;
      return v;
#elif ENABLE_LEGACY_INPUT_MANAGER
      return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#else
      return default;
#endif
    }

    public static bool GetInteractPressed() {
#if ENABLE_INPUT_SYSTEM
      return Keyboard.current?.eKey.wasPressedThisFrame == true;
#elif ENABLE_LEGACY_INPUT_MANAGER
      return Input.GetKeyDown(KeyCode.E);
#else
      return default;
#endif
    }
  }
}
