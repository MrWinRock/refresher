using UnityEngine;

public class RotationObj : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 50f; // Degrees per second
    [SerializeField] private int rotationDirection = 1; // 1 = forward, -1 = backward

    void Update()
    {
        // Rotate the object around the Z-axis
        transform.Rotate(0, 0, rotationSpeed * rotationDirection * Time.deltaTime);
    }
}
