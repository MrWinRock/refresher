using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game5
{
    [DisallowMultipleComponent]
    public class Shaker : MonoBehaviour
    {
        [Header("Pouring")]
        [SerializeField, Required] private ParticleSystem waterParticles;
        [SerializeField] private float pourStartAngle = 60f;
        [SerializeField] private float pourEndAngle = 180f;
        [SerializeField, MinValue(0f)] private float pourRate = 50f;
        [SerializeField, MinValue(0f)] private float idleRate;

        [Header("Optional Test Rotation")]
        [SerializeField] private bool enableTestRotation;
        [SerializeField, ShowIf(nameof(enableTestRotation)), MinValue(0f)] private float rotationSpeed = 120f;

        [Header("Debug (Runtime)")]
        [SerializeField, ReadOnly] private float currentZRotation;
        [SerializeField, ReadOnly] private bool isPouringNow;

        private void Awake()
        {
            if (waterParticles == null)
            {
                waterParticles = GetComponentInChildren<ParticleSystem>();
            }

            if (waterParticles == null)
            {
                Debug.LogWarning("Shaker has no ParticleSystem assigned.", this);
            }
        }

        private void Update()
        {
            if (enableTestRotation)
            {
                ApplyTestRotation();
            }

            UpdatePouringEmission();
        }

        private void ApplyTestRotation()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            float rotationInput = 0f;

            if (Keyboard.current.downArrowKey.isPressed)
            {
                rotationInput += 1f;
            }

            if (Keyboard.current.upArrowKey.isPressed)
            {
                rotationInput -= 1f;
            }

            if (Mathf.Approximately(rotationInput, 0f))
            {
                return;
            }

            transform.Rotate(0f, 0f, rotationInput * rotationSpeed * Time.deltaTime, Space.Self);
        }

        private void UpdatePouringEmission()
        {
            if (waterParticles == null)
            {
                return;
            }

            float zRotation = NormalizeAngle(transform.eulerAngles.z);
            bool isPouring = IsAngleInRange(zRotation, pourStartAngle, pourEndAngle);
            currentZRotation = zRotation;
            isPouringNow = isPouring;

            var emission = waterParticles.emission;
            emission.rateOverTime = isPouring ? pourRate : idleRate;
        }

        private static float NormalizeAngle(float angle)
        {
            return Mathf.DeltaAngle(0f, angle);
        }

        private static bool IsAngleInRange(float angle, float startAngle, float endAngle)
        {
            float normalizedAngle = NormalizeAngle(angle);
            float normalizedStart = NormalizeAngle(startAngle);
            float normalizedEnd = NormalizeAngle(endAngle);

            // If start is greater than end, the range crosses -180/180.
            if (normalizedStart > normalizedEnd)
            {
                return normalizedAngle >= normalizedStart || normalizedAngle <= normalizedEnd;
            }

            return normalizedAngle >= normalizedStart && normalizedAngle <= normalizedEnd;
        }
    }
}



