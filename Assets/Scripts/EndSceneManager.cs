using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class EndSceneManager : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            ReturnToMainMenu();
        }
    }

    // Alternatively, if using generic Submit action
    public void OnSubmit(InputValue value)
    {
        if (value.isPressed)
        {
            ReturnToMainMenu();
        }
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    // Support for manual check if PlayerInput is not used with SendMessages
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ReturnToMainMenu();
        }
    }
}
