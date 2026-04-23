using Minigame;
using Refresh;
using UnityEngine;
using Game5;

public class FreshTimeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MinigameManager minigameManager;
    [SerializeField] private CustomerSpawner customerSpawner;
    [SerializeField] private MinigameSequenceManager sequenceManager;
    [SerializeField] private ServingController servingController;
    [SerializeField] private PouringMiniGameController pouringController;

    private bool _isFreshTimeActive;
    private bool _freshTimeEndPending;

    private void OnEnable()
    {
        BoostMode.BoostActivated += OnBoostActivated;
        BoostMode.BoostEnded += OnBoostEnded;
    }

    private void OnDisable()
    {
        BoostMode.BoostActivated -= OnBoostActivated;
        BoostMode.BoostEnded -= OnBoostEnded;
    }

    private void Update()
    {
        if (!_freshTimeEndPending) return;

        if (!HasActiveCustomers())
        {
            _freshTimeEndPending = false;
            EndFreshTime();
        }
    }

    private void OnBoostActivated()
    {
        _isFreshTimeActive = true;
        _freshTimeEndPending = false;

        customerSpawner?.SetNextAsFever(true);
        minigameManager?.SetFeverMode(true);
        sequenceManager?.SetFreshTimeActive(true);
        servingController?.SetFreshTimeActive(true);
        pouringController?.SetFeverMode(true);
    }

    private void OnBoostEnded()
    {
        if (HasActiveCustomers())
        {
            // Keep FreshTime behaviors alive until the current customer loop finishes.
            _freshTimeEndPending = true;
        }
        else
        {
            EndFreshTime();
        }
    }

    private void EndFreshTime()
    {
        _isFreshTimeActive = false;
        _freshTimeEndPending = false;

        customerSpawner?.SetNextAsFever(false);
        minigameManager?.SetFeverMode(false);
        sequenceManager?.SetFreshTimeActive(false);
        servingController?.SetFreshTimeActive(false);
        pouringController?.SetFeverMode(false);
    }

    private static bool HasActiveCustomers()
    {
        var customers = FindObjectsByType<CustomerController>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        return customers.Length > 0;
    }
}
