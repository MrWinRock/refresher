using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refresh
{
    public class CustomerSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CustomerController customerPrefab;
        [SerializeField] private Transform waitingPoint;
        [SerializeField] private Transform exitPoint;
        [SerializeField] private Transform spawnPoint;

        [Header("Spawn")]
        [SerializeField] private bool autoStartOnEnable = true;
        [SerializeField] private bool useQueueMode = true;
        [SerializeField] private float queueSpawnDelay = 0.1f;
        [SerializeField] private float spawnInterval = 4f;
        [SerializeField] private int maxActiveCustomers = 1;

        private readonly List<CustomerController> _activeCustomers = new List<CustomerController>();
        private Coroutine _spawnLoop;
        private bool _isSpawning;

        private void OnEnable()
        {
            if (autoStartOnEnable)
            {
                StartSpawning();
            }
        }

        private void OnDisable()
        {
            StopSpawning();
        }

        public void StartSpawning()
        {
            if (_spawnLoop != null)
            {
                return;
            }

            _isSpawning = true;
            _spawnLoop = StartCoroutine(SpawnLoop());
        }

        public void StopSpawning()
        {
            _isSpawning = false;

            if (_spawnLoop == null)
            {
                return;
            }

            StopCoroutine(_spawnLoop);
            _spawnLoop = null;
        }

        public void SpawnNow()
        {
            CleanupInactiveCustomers();
            var activeLimit = useQueueMode ? 1 : Mathf.Max(1, maxActiveCustomers);
            if (_activeCustomers.Count >= activeLimit)
            {
                return;
            }

            if (customerPrefab == null || waitingPoint == null || exitPoint == null)
            {
                Debug.LogWarning("[CustomerSpawner] Missing required references.");
                return;
            }

            var startPosition = spawnPoint != null ? spawnPoint.position : waitingPoint.position;
            var customer = Instantiate(customerPrefab, startPosition, Quaternion.identity, transform);
            customer.Initialize(waitingPoint, exitPoint);
            _activeCustomers.Add(customer);
        }

        private IEnumerator SpawnLoop()
        {
            while (_isSpawning)
            {
                if (useQueueMode)
                {
                    yield return QueueSpawnStep();
                }
                else
                {
                    SpawnNow();
                    var delay = Mathf.Max(0.1f, spawnInterval);
                    yield return new WaitForSeconds(delay);
                }
            }
        }

        private IEnumerator QueueSpawnStep()
        {
            CleanupInactiveCustomers();

            if (_activeCustomers.Count > 0)
            {
                yield return null;
                yield break;
            }

            SpawnNow();
            if (_activeCustomers.Count == 0)
            {
                // Missing references or failed spawn: avoid tight loop.
                yield return new WaitForSeconds(0.25f);
                yield break;
            }

            // Wait until this customer has fully left (deactivated/destroyed).
            yield return new WaitUntil(() => !_isSpawning || !IsAnyCustomerActive());
            if (!_isSpawning)
            {
                yield break;
            }

            CleanupInactiveCustomers();
            var delay = Mathf.Max(0f, queueSpawnDelay);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }
        }

        private bool IsAnyCustomerActive()
        {
            for (var i = 0; i < _activeCustomers.Count; i++)
            {
                var customer = _activeCustomers[i];
                if (customer != null && customer.gameObject.activeInHierarchy)
                {
                    return true;
                }
            }

            return false;
        }

        private void CleanupInactiveCustomers()
        {
            for (var i = _activeCustomers.Count - 1; i >= 0; i--)
            {
                var customer = _activeCustomers[i];
                if (customer == null || !customer.gameObject.activeInHierarchy)
                {
                    _activeCustomers.RemoveAt(i);
                }
            }
        }
    }
}


