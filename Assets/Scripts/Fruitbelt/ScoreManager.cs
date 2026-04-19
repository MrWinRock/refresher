using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int Point        { get; private set; }
    public int TotalAttempts { get; private set; }

    public void Reset()
    {
        Point         = 0;
        TotalAttempts = 0;
    }

    public void Register(bool isHit)
    {
        TotalAttempts++;
        if (isHit) Point++;
    }

    public float CalculateFever()
    {
        if (TotalAttempts == 0) return 0f;
        return (float)Point / TotalAttempts;
    }
}