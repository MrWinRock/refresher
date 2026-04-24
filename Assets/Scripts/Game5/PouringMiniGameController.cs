using System;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using Refresh;

namespace Game5
{
    [System.Serializable]
    public struct DrinkMinigameEntry
    {
        public DrinkData drinkData;
        public GameObject drinkObject;
    }

    [DisallowMultipleComponent]
    public class PouringMiniGameController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Transform shaker;
        [SerializeField] private ParticleSystem streamParticles;

        [Header("Drink Entries")]
        [SerializeField] private DrinkMinigameEntry[] drinkEntries;

        // Bound at runtime in BeginMinigame — not assigned in Inspector
        private Transform waterObject;
        private WaterFillController waterFill;

        [Header("Water Range (Local Y)")]
        [SerializeField] private float minY = -1f;
        [SerializeField] private float maxY = 1f;

        [Header("Pour Setup")]
        private float fillSpeed = 0.5f;
        [SerializeField] private float pourAngle = 60f;
        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private bool clearParticlesOnStop;

        [Header("Scoring (Time)")]
        private float perfectTimeSeconds = 5.539817f;
        [SerializeField] private float perfectEarlyTolerance = 0.12f;
        [SerializeField] private float perfectLateTolerance = 0.05f;
        [SerializeField] private float goodEarlyTolerance = 0.35f;
        [SerializeField] private float goodLateTolerance = 0.20f;
        [SerializeField] private float maxTotalPoints = 1f;

        [Header("Lifecycle")]
        [SerializeField] private bool autoBeginOnEnable = false;

        [Header("FreshTime")]
        [SerializeField] private float feverFillSpeedMultiplier = 1.5f;

        [Header("Audio")]
        [SerializeField] private AudioSource sfxAudioSource;
        [SerializeField] private AudioSource pourAudioSource; // New source for looping pour sound
        [SerializeField] private AudioClip pourSfx;          // (5)POURเทน้ำ
        [SerializeField] private AudioClip potHeatSfx;
        [SerializeField] private AudioClip perfectResultSfx;  // (3-5) Perfect
        [SerializeField] private AudioClip otherResultSfx;    // TINGG SLOT
        [SerializeField] private float potHeatTriggerPercent = 80f;

        private Quaternion _initialShakerRotation;
        private bool _hasStartedPouring;
        private bool _overflowed;
        private bool _isFinished;
        private float _pourStartTime;
        private PointManager _pointManager;
        private BoostMode _boostMode;
        private bool _isMinigameActive;
        private float _currentY;
        private bool _isPourInputArmed;
        private bool _feverMode;
        private bool _hasPlayedPotHeat;

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

        public void SetFeverMode(bool active) => _feverMode = active;

        private void Awake()
        {
            if (shaker == null)
            {
                shaker = transform;
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
                var defaultData = drinkEntries != null && drinkEntries.Length > 0 ? drinkEntries[0].drinkData : null;
                BeginMinigame(defaultData);
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

            if (!_isPourInputArmed)
            {
                // Prevent key carry-over from the previous minigame.
                if (AreAllArrowKeysReleased())
                {
                    _isPourInputArmed = true;
                }

                ReturnShakerToInitialRotation();
                StopStream();
                return;
            }

            bool isPourKeyPressedThisFrame = Keyboard.current != null && Keyboard.current.downArrowKey.wasPressedThisFrame;
            bool isHoldingPourKey = Keyboard.current != null && Keyboard.current.downArrowKey.isPressed;

            // In Fever Mode, we simulate a constant hold of the down arrow key
            if (_feverMode)
            {
                isHoldingPourKey = true;
                if (!_hasStartedPouring) isPourKeyPressedThisFrame = true;
            }

            if (!_hasStartedPouring)
{
                if (isPourKeyPressedThisFrame)
                {
                    _hasStartedPouring = true;
                    _pourStartTime = Time.time;
                }
                else
                {
                    ReturnShakerToInitialRotation();
                    StopStream();
                    return;
                }
            }

            if (isHoldingPourKey)
            {

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
            float speed = _feverMode ? fillSpeed * feverFillSpeedMultiplier : fillSpeed;
            float nextY = Mathf.MoveTowards(_currentY, maxY, speed * Time.deltaTime);
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

            if (!_hasPlayedPotHeat && pouredPercent >= potHeatTriggerPercent && _isMinigameActive)
            {
                _hasPlayedPotHeat = true;
                if (potHeatSfx != null && sfxAudioSource != null)
                {
                    sfxAudioSource.PlayOneShot(potHeatSfx);
                }
            }
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

            // Start looping pour sound
            if (pourAudioSource != null && pourSfx != null && !pourAudioSource.isPlaying)
            {
                pourAudioSource.clip = pourSfx;
                pourAudioSource.loop = true;
                pourAudioSource.Play();
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

            // Stop looping pour sound
            if (pourAudioSource != null && pourAudioSource.isPlaying)
            {
                pourAudioSource.Stop();
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

        public void BeginMinigame(DrinkData data)
        {
            ActivateAndBindDrink(data);
            _pointManager?.ResetPoints();
            _isMinigameActive = true;
            _hasStartedPouring = false;
            _isPourInputArmed = false;
            _overflowed = false;
            _isFinished = false;
            _hasPlayedPotHeat = false;
            _pourStartTime = 0f;
            elapsedPourTime = 0f;
            finalPourTime = 0f;
            pointsEarned = 0f;
            totalPoints = 0f;
            normalizedPoints = 0f;
            boostPointsToAdd = 0f;
            lastResult = "-";
            SetWaterY(minY);
            StopStream();
            ReturnShakerToInitialRotation();
            RefreshDebugPourMetrics();
        }

        public void ResetMinigame()
        {
            _pointManager?.ResetPoints();
            _isMinigameActive = false;
            _hasStartedPouring = false;
            _isPourInputArmed = false;
            _overflowed = false;
            _isFinished = false;
            _hasPlayedPotHeat = false;
            _pourStartTime = 0f;
            elapsedPourTime = 0f;
            finalPourTime = 0f;
            pointsEarned = 0f;
            totalPoints = 0f;
            normalizedPoints = 0f;
            boostPointsToAdd = 0f;
            lastResult = "-";
            SetWaterY(minY);
            StopStream();
            ReturnShakerToInitialRotation();
            RefreshDebugPourMetrics();
        }

        private void ActivateAndBindDrink(DrinkData data)
        {
            waterObject = null;
            waterFill = null;

            if (drinkEntries == null || drinkEntries.Length == 0)
            {
                Debug.LogWarning("PouringMiniGameController: No DrinkEntries configured. Assign drink entries in the Inspector.", this);
                return;
            }

            for (var i = 0; i < drinkEntries.Length; i++)
            {
                var entry = drinkEntries[i];
                if (entry.drinkObject == null)
                {
                    continue;
                }

                var isMatch = entry.drinkData == data;
                entry.drinkObject.SetActive(isMatch);

                if (isMatch)
                {
                    waterFill = entry.drinkObject.GetComponentInChildren<WaterFillController>(true);
                    waterObject = waterFill != null ? waterFill.transform : entry.drinkObject.transform;
                }
            }

            if (data == null)
            {
                return;
            }

            fillSpeed = data.fillSpeed;
            perfectTimeSeconds = data.perfectTimeSeconds;

            if (streamParticles != null)
            {
                var main = streamParticles.main;
                main.startColor = data.particleStartColor;
            }

            if (waterObject == null)
            {
                Debug.LogWarning($"PouringMiniGameController: No DrinkEntry matched DrinkData '{data.name}'. Check DrinkEntries in the Inspector.", this);
            }
        }

        private void EvaluateResult()
        {
            float earlyDelta = perfectTimeSeconds - finalPourTime;
            float lateDelta = finalPourTime - perfectTimeSeconds;

            RefreshDebugPourMetrics();

            string result;

            bool isOverflow = _overflowed || _currentY >= maxY;
            pointsEarned = 0f;

            if (_feverMode)
            {
                result = "Perfect (FreshTime)";
                pointsEarned = 1f;
            }
            else if (isOverflow)
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
                normalizedPoints = _pointManager.CalculateBoostPoint();
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

            // Play Result SFX
            if (sfxAudioSource != null)
            {
                if (lastResult.Contains("Perfect"))
                {
                    if (perfectResultSfx != null) sfxAudioSource.PlayOneShot(perfectResultSfx);
                }
                else
                {
                    if (otherResultSfx != null) sfxAudioSource.PlayOneShot(otherResultSfx);
                }
            }

            Debug.Log($"Pour Result: {lastResult} | points={pointsEarned:F1}, total={totalPoints:F1}, normalized={normalizedPoints:F2}, boostAdd={boostPointsToAdd:F2} | pourTime={finalPourTime:F3}s target={targetTimeDebug:F3}s delta={deltaTimeDebug:+0.000;-0.000;0.000}s | fill={pouredPercent:F1}%", this);
            }

        private void RefreshDebugPourMetrics()
        {
            pouredPercent = Mathf.Clamp01(Mathf.InverseLerp(minY, maxY, _currentY)) * 100f;
            targetTimeDebug = perfectTimeSeconds;
            deltaTimeDebug = finalPourTime - perfectTimeSeconds;
        }

        private static bool AreAllArrowKeysReleased()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return true;
            }

            return !keyboard.upArrowKey.isPressed
                   && !keyboard.downArrowKey.isPressed
                   && !keyboard.leftArrowKey.isPressed
                   && !keyboard.rightArrowKey.isPressed;
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
