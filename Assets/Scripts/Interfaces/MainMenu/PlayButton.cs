using Interfaces.MainMenu;
using UnityEngine;

public class PlayButton : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "Game";

    public void PlayGame()
    {
        SceneTransitionService.TransitionToScene(targetSceneName);
    }
}
