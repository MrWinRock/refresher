using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Minigame.ShakerMinigame
{
    public class ShakerMinigameDebugBootstrap : MonoBehaviour
    {
        [SerializeField] private ShakerMinigameController shakerController;
        [SerializeField] private MinigameManager minigameManager;

        private void Update()
        {
            if (IsKeyDownThisFrame(KeyCode.F1))
            {
                shakerController?.BeginMinigame();
            }

            if (IsKeyDownThisFrame(KeyCode.F2))
            {
                shakerController?.EndMinigame();
            }

            if (IsKeyDownThisFrame(KeyCode.F3) && minigameManager != null)
            {
                minigameManager.SetFeverMode(!minigameManager.IsFeverMode);
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
                KeyCode.F1 => keyboard.f1Key.wasPressedThisFrame,
                KeyCode.F2 => keyboard.f2Key.wasPressedThisFrame,
                KeyCode.F3 => keyboard.f3Key.wasPressedThisFrame,
                _ => false
            };
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(keyCode);
#else
            return false;
#endif
        }
    }
}



