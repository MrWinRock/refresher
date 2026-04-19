using System;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public event Action OnSpacePressed;

    private bool isListening = false;

    public void EnableListening()  => isListening = true;
    public void DisableListening() => isListening = false;

    private void Update()
    {
        if (isListening && Input.GetKeyDown(KeyCode.Space))
            OnSpacePressed?.Invoke();
    }
}