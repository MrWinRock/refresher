using UnityEngine;

public class PlayButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void PlayGame()
    {
        // Load the next scene (assuming the next scene is at index 1)
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
