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

        [Header("Visual Random")]
        [SerializeField] private List<Sprite> customerVisualSprites = new List<Sprite>();

        [Header("Spawn")]
        [SerializeField] private bool autoStartOnEnable = true;
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
            var activeLimit = Mathf.Max(1, maxActiveCustomers);
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
            customer.SetVisualSprite(GetRandomCustomerVisualSprite());
            customer.Initialize(waitingPoint, exitPoint);
            _activeCustomers.Add(customer);
        }

        private IEnumerator SpawnLoop()
        {
            while (_isSpawning)
            {
                SpawnNow();
                var delay = Mathf.Max(0.1f, spawnInterval);
                yield return new WaitForSeconds(delay);
            }
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

        private Sprite GetRandomCustomerVisualSprite()
        {
            var validSprites = new List<Sprite>();
            for (var i = 0; i < customerVisualSprites.Count; i++)
            {
                if (customerVisualSprites[i] != null)
                {
                    validSprites.Add(customerVisualSprites[i]);
                }
            }

            if (validSprites.Count > 0)
            {
                var randomIndex = Random.Range(0, validSprites.Count);
                return validSprites[randomIndex];
            }

            return null;
        }
    }
}
