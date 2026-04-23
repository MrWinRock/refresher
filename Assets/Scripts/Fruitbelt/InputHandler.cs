using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public event Action OnSpacePressed;

    private bool isListening = false;

    public void EnableListening()  => isListening = true;
    public void DisableListening() => isListening = false;

    private void Update()
    {
        if (isListening && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            OnSpacePressed?.Invoke();
    }
}