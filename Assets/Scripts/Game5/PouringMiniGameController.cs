using UnityEngine;
using UnityEngine.InputSystem;

namespace Game5
{
    [DisallowMultipleComponent]
    public class PouringMiniGameController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Transform shaker;
        [SerializeField] private ParticleSystem streamParticles;
        [SerializeField] private Transform waterObject;

        [Header("Water Range (Local Y)")]
        [SerializeField] private float minY = -1f;
        [SerializeField] private float maxY = 1f;

        [Header("Pour Setup")]
        [SerializeField] private float fillSpeed = 0.5f;
        [SerializeField] private float pourAngle = 60f;
        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private bool clearParticlesOnStop;

        private const float TargetFillNormalized = 0.8f;

        private Quaternion _initialShakerRotation;
        private bool _hasStartedPouring;
        private bool _overflowed;
        private bool _isFinished;

        public bool IsFinished => _isFinished;

        private void Awake()
        {
            if (shaker == null)
            {
                shaker = transform;
            }

            if (waterObject == null)
            {
                Debug.LogError("PouringMiniGameController requires a waterObject reference.", this);
                enabled = false;
                return;
            }

            _initialShakerRotation = shaker.localRotation;
            SetWaterY(minY);
            StopStream();
        }

        private void Update()
        {
            if (_isFinished)
            {
                ReturnShakerToInitialRotation();
                return;
            }

            bool isHoldingPourKey = Keyboard.current != null && Keyboard.current.downArrowKey.isPressed;

            if (isHoldingPourKey)
            {
                _hasStartedPouring = true;
                RotateShakerToPourAngle();
                StartStream();

                if (RaiseWater())
                {
                    _overflowed = true;
                    FinishPouring();
                }

                return;
            }

            if (_hasStartedPouring)
            {
                FinishPouring();
            }
            else
            {
                ReturnShakerToInitialRotation();
                StopStream();
            }
        }

        private void RotateShakerToPourAngle()
        {
            Quaternion target = _initialShakerRotation * Quaternion.Euler(0f, 0f, pourAngle);
            shaker.localRotation = Quaternion.RotateTowards(
                shaker.localRotation,
                target,
                rotationSpeed * Time.deltaTime);
        }

        private void ReturnShakerToInitialRotation()
        {
            shaker.localRotation = Quaternion.RotateTowards(
                shaker.localRotation,
                _initialShakerRotation,
                rotationSpeed * Time.deltaTime);
        }

        private bool RaiseWater()
        {
            float currentY = waterObject.localPosition.y;
            float nextY = Mathf.MoveTowards(currentY, maxY, fillSpeed * Time.deltaTime);
            SetWaterY(nextY);
            return Mathf.Approximately(nextY, maxY) || nextY >= maxY;
        }

        private void SetWaterY(float y)
        {
            if (waterObject == null)
            {
                return;
            }

            Vector3 localPos = waterObject.localPosition;
            localPos.y = Mathf.Clamp(y, minY, maxY);
            waterObject.localPosition = localPos;
        }

        private void StartStream()
        {
            if (streamParticles == null)
            {
                return;
            }

            var emission = streamParticles.emission;
            emission.enabled = true;

            if (!streamParticles.isPlaying)
            {
                streamParticles.Play();
            }
        }

        private void StopStream()
        {
            if (streamParticles == null)
            {
                return;
            }

            var emission = streamParticles.emission;
            emission.enabled = false;

            if (clearParticlesOnStop)
            {
                streamParticles.Clear(true);
            }
        }

        private void FinishPouring()
        {
            if (_isFinished)
            {
                return;
            }

            _isFinished = true;
            StopStream();
            EvaluateResult();
        }

        private void EvaluateResult()
        {
            float tHit = Mathf.InverseLerp(minY, maxY, waterObject.localPosition.y);
            float deltaT = Mathf.Abs(tHit - TargetFillNormalized);

            string result;

            if (_overflowed)
            {
                result = "Bad (Overflow)";
            }
            else if (deltaT < 0.05f)
            {
                result = "Perfect";
            }
            else if (deltaT < 0.15f)
            {
                result = "Good";
            }
            else
            {
                result = "Bad";
            }

            Debug.Log($"Pour Result: {result} | tHit={tHit:F2}, target={TargetFillNormalized:F2}, delta={deltaT:F2}", this);
        }

        [ContextMenu("Calibration/Set Water Empty")]
        private void SetWaterToEmpty()
        {
            if (waterObject == null)
            {
                return;
            }

            RecordCalibrationChange();
            SetWaterY(minY);
        }

        [ContextMenu("Calibration/Set Water Full")]
        private void SetWaterToFull()
        {
            if (waterObject == null)
            {
                return;
            }

            RecordCalibrationChange();
            SetWaterY(maxY);
        }

#if UNITY_EDITOR
        private void RecordCalibrationChange()
        {
            UnityEditor.Undo.RecordObject(waterObject, "Calibrate Water Level");
            UnityEditor.EditorUtility.SetDirty(waterObject);
        }
#else
        private void RecordCalibrationChange()
        {
        }
#endif
    }
}


