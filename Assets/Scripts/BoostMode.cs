using System;
using System.Collections;
using UnityEngine;

public class BoostMode : MonoBehaviour
{
    public static BoostMode Instance { get; private set; }

    public static event Action<float, float> BoostPointsChanged;
public static event Action BoostActivated;
    public static event Action BoostEnded;

    [Header("Settings")]
    [SerializeField] private float threshold = 5f;
    [SerializeField] private float boostDuration = 10f;
    [SerializeField] private float postBoostCooldown = 5f;

    private float _boostPoints;
    private bool _isBoostActive;
    private bool _shouldExtendBoost;
    private float _cooldownRemaining;
    private Coroutine _timerRoutine;

    public float BoostPoints => _boostPoints;
    public float Threshold => threshold;
    public float Duration => boostDuration;
    public bool IsBoostActive => _isBoostActive;

    public void SetExtendBoost(bool extend)
    {
        _shouldExtendBoost = extend;
    }

    [ContextMenu("Debug/Add 1 Point")]
    public void Debug_Add1Point() => AddBoostPoints(1f);

    [ContextMenu("Debug/Fill Boost")]
    public void Debug_FillBoost() => AddBoostPoints(Threshold);

    [ContextMenu("Debug/Reset Boost")]
    public void Debug_ResetBoost() => CancelBoost();

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
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (_cooldownRemaining > 0f)
            _cooldownRemaining -= Time.deltaTime;
    }

    public void AddBoostPoints(float points)
    {
        if (points <= 0f || _isBoostActive || _cooldownRemaining > 0f) return;

        _boostPoints = Mathf.Min(_boostPoints + points, threshold);
        BoostPointsChanged?.Invoke(_boostPoints, threshold);

        if (_boostPoints >= threshold) ApplyBoost();
    }

    public void ConsumeBoostPoints(float amount)
    {
        if (!_isBoostActive || amount <= 0f) return;

        _boostPoints = Mathf.Max(0f, _boostPoints - amount);
        BoostPointsChanged?.Invoke(_boostPoints, threshold);

        if (_boostPoints <= 0f) EndBoost();
    }

    public void CancelBoost()
    {
        if (!_isBoostActive) return;

        _boostPoints = 0f;
        BoostPointsChanged?.Invoke(_boostPoints, threshold);
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
        var remaining = boostDuration;
        while (remaining > 0f)
        {
            yield return null;
            remaining -= Time.deltaTime;
            _boostPoints = Mathf.Max(0f, (remaining / boostDuration) * threshold);
            BoostPointsChanged?.Invoke(_boostPoints, threshold);
        }

        _boostPoints = 0f;
        BoostPointsChanged?.Invoke(_boostPoints, threshold);
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
        _cooldownRemaining = postBoostCooldown;
        BoostEnded?.Invoke();
    }
}
