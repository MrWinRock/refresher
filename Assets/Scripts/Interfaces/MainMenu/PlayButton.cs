using UnityEngine;
using UnityEngine.Playables;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Interfaces.MainMenu
{
    public class PlayButton : MonoBehaviour
    {
        [SerializeField] private string targetSceneName = "Game";
        [SerializeField] private PlayableDirector timelineBeforeTransition;
        private bool _hasTriggered;

        private void Update()
        {
            if (_hasTriggered)
            {
                return;
            }

            if (IsSpacePressedThisFrame())
            {
                PlayGame();
            }
        }

        private static bool IsSpacePressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Space);
#else
            return false;
#endif
        }

        public void PlayGame()
        {
            if (_hasTriggered)
            {
                return;
            }

            _hasTriggered = true;
            SceneTransitionService.TransitionToScene(targetSceneName, timelineBeforeTransition);
        }
    }
}
