using UnityEngine;

public class PointManager
{
    private float _totalPoints;
    private float _boostPoints;
    private readonly float _maxTotalPoints;

    public float TotalPoints => _totalPoints;
    public float BoostPoints => _boostPoints;

    public PointManager(float maxTotalPoints)
    {
        _maxTotalPoints = maxTotalPoints;
    }

    public void AddPoints(float point)
    {
        _totalPoints += point;
    }

    public float CalculatePoints()
    {
        return _totalPoints / _maxTotalPoints;
    }

    public void ResetPoints()
    {
        _totalPoints = 0;
        _boostPoints = 0;
    }
}
