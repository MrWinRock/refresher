using UnityEngine;
using UnityEngine.Playables;

namespace Interfaces.MainMenu
{
    public class PlayButton : MonoBehaviour
    {
        [SerializeField] private string targetSceneName = "Game";
        [SerializeField] private PlayableDirector timelineBeforeTransition;

        public void PlayGame()
        {
            SceneTransitionService.TransitionToScene(targetSceneName, timelineBeforeTransition);
        }
    }
}
