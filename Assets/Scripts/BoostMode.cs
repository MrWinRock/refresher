using System.Collections;
using Interfaces;
using UnityEngine;

public class BoostMode : MonoBehaviour
{
    private static BoostMode Instance { get; set; }

    [SerializeField] private float boostPoints;
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

    public float BoostPoints => boostPoints;

    public void AddBoostPoints(float points)
    {
        boostPoints += points;
        if (boostPoints >= DefaultThreshold) ApplyBoost();
    }

    private void ApplyBoost()
    {
        // Apply boost effect

        StartCoroutine(BoostCooldown());
    }

    private IEnumerator BoostCooldown()
    {
        yield return new WaitForSeconds(BoostDuration);

        boostPoints = 0;
    }
}
