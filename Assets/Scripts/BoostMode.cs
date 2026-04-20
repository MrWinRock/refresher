using System.Collections;
using Interfaces;
using UnityEngine;

public class BoostMode : MonoBehaviour
{
    private static BoostMode Instance { get; set; }

    private float _boostPoints;
    private const int DefaultThreshold = 5;
    private const float BoostDuration = 10f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public float BoostPoints => _boostPoints;

    public void AddBoostPoints(float points)
    {
        _boostPoints += points;
        if (_boostPoints >= DefaultThreshold) ApplyBoost();
    }

    private void ApplyBoost()
    {
        // Apply boost effect

        StartCoroutine(BoostCooldown());
    }

    private IEnumerator BoostCooldown()
    {
        yield return new WaitForSeconds(BoostDuration);

        _boostPoints = 0;
    }
}
