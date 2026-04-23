using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Minigame.ShakerMinigame
{
    public class ShakerInputHandler : MonoBehaviour
    {
        public event Action<ArrowDirection, float> ArrowPressed;
        public event Action<float> AnyKeyPressed;

        private void Update()
        {
            var now = Time.time;

            if (IsKeyDownThisFrame(KeyCode.UpArrow)) ArrowPressed?.Invoke(ArrowDirection.Up, now);
            if (IsKeyDownThisFrame(KeyCode.DownArrow)) ArrowPressed?.Invoke(ArrowDirection.Down, now);
            if (IsKeyDownThisFrame(KeyCode.LeftArrow)) ArrowPressed?.Invoke(ArrowDirection.Left, now);
            if (IsKeyDownThisFrame(KeyCode.RightArrow)) ArrowPressed?.Invoke(ArrowDirection.Right, now);

            if (IsAnyKeyDownThisFrame())
            {
                AnyKeyPressed?.Invoke(now);
            }
        }

        private static bool IsKeyDownThisFrame(KeyCode keyCode)
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            return keyCode switch
            {
                KeyCode.UpArrow => keyboard.upArrowKey.wasPressedThisFrame,
                KeyCode.DownArrow => keyboard.downArrowKey.wasPressedThisFrame,
                KeyCode.LeftArrow => keyboard.leftArrowKey.wasPressedThisFrame,
                KeyCode.RightArrow => keyboard.rightArrowKey.wasPressedThisFrame,
                _ => false
            };
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(keyCode);
#else
            return false;
#endif
        }

        private static bool IsAnyKeyDownThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.anyKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.anyKeyDown;
#else
            return false;
#endif
        }
    }
}



