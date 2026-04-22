using System;
using System.Collections;
using UnityEngine;

public class BoostMode : MonoBehaviour
{
    private static BoostMode Instance { get; set; }

    public static event Action<float, float> BoostPointsChanged;
    public static event Action BoostActivated;
    public static event Action BoostEnded;

    private const float DefaultThreshold = 5f;
    private const float BoostDuration = 10f;

    private float _boostPoints;
    private bool _isBoostActive;
    private Coroutine _timerRoutine;

    public float BoostPoints => _boostPoints;
    public float Threshold => DefaultThreshold;
    public float Duration => BoostDuration;
    public bool IsBoostActive => _isBoostActive;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
        BoostPointsChanged = null;
        BoostActivated = null;
        BoostEnded = null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void AddBoostPoints(float points)
    {
        if (points <= 0f || _isBoostActive) return;

        _boostPoints = Mathf.Min(_boostPoints + points, DefaultThreshold);
        BoostPointsChanged?.Invoke(_boostPoints, DefaultThreshold);

        if (_boostPoints >= DefaultThreshold) ApplyBoost();
    }

    public void ConsumeBoostPoints(float amount)
    {
        if (!_isBoostActive || amount <= 0f) return;

        _boostPoints = Mathf.Max(0f, _boostPoints - amount);
        BoostPointsChanged?.Invoke(_boostPoints, DefaultThreshold);

        if (_boostPoints <= 0f) EndBoost();
    }

    public void CancelBoost()
    {
        if (!_isBoostActive) return;

        _boostPoints = 0f;
        BoostPointsChanged?.Invoke(_boostPoints, DefaultThreshold);
        EndBoost();
    }

    private void ApplyBoost()
    {
        _isBoostActive = true;
        BoostActivated?.Invoke();

        if (_timerRoutine != null) StopCoroutine(_timerRoutine);
        _timerRoutine = StartCoroutine(BoostTimer());
    }

    private IEnumerator BoostTimer()
    {
        yield return new WaitForSeconds(boostDuration);

        _boostPoints = 0f;
        BoostPointsChanged?.Invoke(_boostPoints, DefaultThreshold);
        EndBoost();
    }

    private void EndBoost()
    {
        if (_timerRoutine != null)
        {
            StopCoroutine(_timerRoutine);
            _timerRoutine = null;
        }

        _isBoostActive = false;
        BoostEnded?.Invoke();
    }
}
