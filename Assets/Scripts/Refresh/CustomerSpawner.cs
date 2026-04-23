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
        [SerializeField] private CharacterDatabase characterDatabase;

        [Header("Spawn Settings")]
        [SerializeField] private bool autoStartOnEnable = true;
        [SerializeField] private float spawnInterval = 4f;
        [SerializeField] private int maxActiveCustomers = 1;

        [Header("Randomization")]
        [SerializeField] private int historySize = 5;

        private readonly List<CustomerController> _activeCustomers = new List<CustomerController>();
        private readonly List<int> _spawnHistory = new List<int>();
        private Coroutine _spawnLoop;
        private bool _isSpawning;
        private bool _queueNextAsFever;

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

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.fKey.wasPressedThisFrame)
            {
                SetNextAsFever(true);
                Debug.Log("Next customer will be FEVER!");
            }
        }

        public void SetNextAsFever(bool isFever)
        {
            _queueNextAsFever = isFever;
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

            if (customerPrefab == null || waitingPoint == null || exitPoint == null || characterDatabase == null)
            {
                Debug.LogWarning("[CustomerSpawner] Missing required references.");
                return;
            }

            // Pick character
            int charIndex = PickSmartRandomCharacterIndex();
            var charData = characterDatabase.characters[charIndex];
            bool isFever = _queueNextAsFever;
            
            // If Fever requested but no fever version, skip or force? 
            // The user mapping implies specific characters have fever.
            if (isFever && !charData.hasFeverVersion)
            {
                // Find a fever character if requested
                int feverIndex = FindRandomFeverCharacterIndex();
                if (feverIndex != -1)
                {
                    charIndex = feverIndex;
                    charData = characterDatabase.characters[charIndex];
                }
                else
                {
                    isFever = false; // No fever characters available at all
                }
            }

            var startPosition = spawnPoint != null ? spawnPoint.position : waitingPoint.position;
            var customer = Instantiate(customerPrefab, startPosition, Quaternion.identity, transform);
            
            customer.Initialize(waitingPoint, exitPoint, charData, isFever);
            
            _activeCustomers.Add(customer);
            
            // Update history
            _spawnHistory.Add(charIndex);
            if (_spawnHistory.Count > historySize) _spawnHistory.RemoveAt(0);
            
            _queueNextAsFever = false;
        }

        private int PickSmartRandomCharacterIndex()
        {
            int count = characterDatabase.characters.Count;
            if (count == 0) return -1;
            if (count == 1) return 0;

            List<int> candidates = new List<int>();
            for (int i = 0; i < count; i++)
            {
                // Basic implementation: if in history, it's not a candidate unless we have no other choice
                if (!_spawnHistory.Contains(i))
                {
                    candidates.Add(i);
                }
            }

            if (candidates.Count > 0)
            {
                return candidates[Random.Range(0, candidates.Count)];
            }

            // Fallback to absolute random if history covers everyone
            return Random.Range(0, count);
        }

        private int FindRandomFeverCharacterIndex()
        {
            List<int> feverIndices = new List<int>();
            for (int i = 0; i < characterDatabase.characters.Count; i++)
            {
                if (characterDatabase.characters[i].hasFeverVersion)
                {
                    feverIndices.Add(i);
                }
            }

            if (feverIndices.Count > 0)
            {
                return feverIndices[Random.Range(0, feverIndices.Count)];
            }
            return -1;
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
        }
        }
