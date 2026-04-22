using System;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;

namespace Game5
{
    [DisallowMultipleComponent]
    public class PouringMiniGameController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Transform shaker;
        [SerializeField] private ParticleSystem streamParticles;
        [SerializeField] private Transform waterObject;
        [SerializeField] private WaterFillController waterFill;

        [Header("Water Range (Local Y)")]
        [SerializeField] private float minY = -1f;
        [SerializeField] private float maxY = 1f;

        [Header("Pour Setup")]
        [SerializeField] private float fillSpeed = 0.5f;
        [SerializeField] private float pourAngle = 60f;
        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private bool clearParticlesOnStop;

        [Header("Scoring (Time)")]
        [SerializeField] private float perfectTimeSeconds = 5.539817f;
        [SerializeField] private float perfectEarlyTolerance = 0.12f;
        [SerializeField] private float perfectLateTolerance = 0.05f;
        [SerializeField] private float goodEarlyTolerance = 0.35f;
        [SerializeField] private float goodLateTolerance = 0.20f;
        [SerializeField] private float maxTotalPoints = 1f;

        [Header("Lifecycle")]
        [SerializeField] private bool autoBeginOnEnable = false;

        private Quaternion _initialShakerRotation;
        private bool _hasStartedPouring;
        private bool _overflowed;
        private bool _isFinished;
        private float _pourStartTime;
        private PointManager _pointManager;
        private BoostMode _boostMode;
        private bool _isMinigameActive;
        private float _currentY;

        [Header("Debug (Runtime)")]
        [SerializeField, ReadOnly] private float pointsEarned;
        [SerializeField, ReadOnly] private float totalPoints;
        [SerializeField, ReadOnly] private float normalizedPoints;
        [SerializeField, ReadOnly] private float boostPointsToAdd;
        [SerializeField, ReadOnly] private float pouredPercent;
        [SerializeField, ReadOnly] private float targetTimeDebug;
        //ส่วนต่างเวลาเทียบเป้า ติดลบ = เร็วกว่าเป้า, ติดบวก = ช้ากว่าเป้า
        [SerializeField, ReadOnly] private float deltaTimeDebug;
        //เวลาที่กำลังเท
        [SerializeField, ReadOnly] private float elapsedPourTime;
        [SerializeField, ReadOnly] private float finalPourTime;
        [SerializeField, ReadOnly] private string lastResult = "-";

        public bool IsFinished => _isFinished;
        public bool IsMinigameActive => _isMinigameActive;
        public event Action MinigameFinished;

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
            _pointManager = new PointManager(maxTotalPoints > 0f ? maxTotalPoints : 1f);
            _boostMode = FindFirstObjectByType<BoostMode>();

            SetWaterY(minY);
            StopStream();
            RefreshDebugPourMetrics();
        }

        private void OnEnable()
        {
            if (autoBeginOnEnable)
            {
                BeginMinigame();
            }
            else
            {
                ResetMinigame();
            }
        }

        private void Update()
        {
            if (!_isMinigameActive)
            {
                ReturnShakerToInitialRotation();
                StopStream();
                return;
            }

            if (_isFinished)
            {
                ReturnShakerToInitialRotation();
                return;
            }

            bool isHoldingPourKey = Keyboard.current != null && Keyboard.current.downArrowKey.isPressed;

            if (isHoldingPourKey)
            {
                if (!_hasStartedPouring)
                {
                    _hasStartedPouring = true;
                    _pourStartTime = Time.time;
                }

                elapsedPourTime = Time.time - _pourStartTime;
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
            float nextY = Mathf.MoveTowards(_currentY, maxY, fillSpeed * Time.deltaTime);
            SetWaterY(nextY);
            return Mathf.Approximately(nextY, maxY) || nextY >= maxY;
        }

        private void SetWaterY(float y)
        {
            _currentY = Mathf.Clamp(y, minY, maxY);

            if (waterFill != null)
            {
                waterFill.SetFillAmount(Mathf.InverseLerp(minY, maxY, _currentY));
            }
            else if (waterObject != null)
            {
                // Legacy fallback: move transform when no WaterFillController is wired up.
                Vector3 localPos = waterObject.localPosition;
                localPos.y = _currentY;
                waterObject.localPosition = localPos;
            }

            RefreshDebugPourMetrics();
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
            _isMinigameActive = false;
            finalPourTime = elapsedPourTime;
            StopStream();
            EvaluateResult();
            MinigameFinished?.Invoke();
        }

        public void BeginMinigame()
        {
            _isMinigameActive = true;
            _hasStartedPouring = false;
            _overflowed = false;
            _isFinished = false;
            _pourStartTime = 0f;
            elapsedPourTime = 0f;
            finalPourTime = 0f;
            pointsEarned = 0f;
            boostPointsToAdd = 0f;
            lastResult = "-";
            SetWaterY(minY);
            StopStream();
            ReturnShakerToInitialRotation();
            RefreshDebugPourMetrics();
        }

        public void ResetMinigame()
        {
            _isMinigameActive = false;
            _hasStartedPouring = false;
            _overflowed = false;
            _isFinished = false;
            _pourStartTime = 0f;
            elapsedPourTime = 0f;
            finalPourTime = 0f;
            pointsEarned = 0f;
            boostPointsToAdd = 0f;
            lastResult = "-";
            SetWaterY(minY);
            StopStream();
            ReturnShakerToInitialRotation();
            RefreshDebugPourMetrics();
        }

        private void EvaluateResult()
        {
            float earlyDelta = perfectTimeSeconds - finalPourTime;
            float lateDelta = finalPourTime - perfectTimeSeconds;

            RefreshDebugPourMetrics();

            string result;

            bool isOverflow = _overflowed || _currentY >= maxY;
            pointsEarned = 0f;

            if (isOverflow)
            {
                result = "Bad (Overflow)";
            }
            else if (earlyDelta <= perfectEarlyTolerance && lateDelta <= perfectLateTolerance)
            {
                result = "Perfect";
                pointsEarned = 1f;
            }
            else if (earlyDelta <= goodEarlyTolerance && lateDelta <= goodLateTolerance)
            {
                result = "Good";
                pointsEarned = 0.5f;
            }
            else
            {
                result = "Bad";
            }

            if (_pointManager != null)
            {
                _pointManager.AddPoints(pointsEarned);
                totalPoints = _pointManager.TotalPoints;
                normalizedPoints = _pointManager.CalculatePoints();
                boostPointsToAdd = normalizedPoints;
            }

            if (_boostMode == null)
            {
                _boostMode = FindFirstObjectByType<BoostMode>();
            }

            if (_boostMode != null)
            {
                _boostMode.AddBoostPoints(boostPointsToAdd);
            }

            lastResult = result;
            Debug.Log($"Pour Result: {lastResult} | points={pointsEarned:F1}, total={totalPoints:F1}, normalized={normalizedPoints:F2}, boostAdd={boostPointsToAdd:F2} | pourTime={finalPourTime:F3}s target={targetTimeDebug:F3}s delta={deltaTimeDebug:+0.000;-0.000;0.000}s | fill={pouredPercent:F1}%", this);
        }

        private void RefreshDebugPourMetrics()
        {
            pouredPercent = Mathf.Clamp01(Mathf.InverseLerp(minY, maxY, _currentY)) * 100f;
            targetTimeDebug = perfectTimeSeconds;
            deltaTimeDebug = finalPourTime - perfectTimeSeconds;
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


