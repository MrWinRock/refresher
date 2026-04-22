using System;
using UnityEngine;

public class PointManager
{
    private float _totalPoints;
    private readonly float _maxTotalPoints;

    public float TotalPoints => _totalPoints;
    public float MaxTotalPoints => _maxTotalPoints;

    public event Action<float, float> PointsChanged;
    public event Action PointsReset;

    public PointManager(float maxTotalPoints)
    {
        _maxTotalPoints = maxTotalPoints > 0f ? maxTotalPoints : 1f;
    }

    public void AddPoints(float point)
    {
        if (Mathf.Approximately(point, 0f)) return;

        _totalPoints += point;
        PointsChanged?.Invoke(_totalPoints, point);
    }

    public float CalculateBoostPoint()
    {
        return _totalPoints / _maxTotalPoints;
    }

    public void ResetPoints()
    {
        _totalPoints = 0f;
        PointsReset?.Invoke();
    }
}
