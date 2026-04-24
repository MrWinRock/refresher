using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTimerTransition : MonoBehaviour
{
    [Tooltip("Time in seconds before transition (default 240s = 4 minutes)")]
    [SerializeField] private float durationSeconds = 240f;
    
    [Tooltip("Name of the scene to load")]
    [SerializeField] private string targetSceneName = "End";

    private float _timer;

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= durationSeconds)
        {
            Transition();
        }
    }

    private void Transition()
    {
        // Prevent multiple calls
        this.enabled = false;
        SceneManager.LoadScene(targetSceneName);
    }
}
