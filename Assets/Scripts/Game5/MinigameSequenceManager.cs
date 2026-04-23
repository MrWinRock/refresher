using UnityEngine;
using UnityEngine.InputSystem;
using Minigame.ShakerMinigame;
using Refresh;

namespace Game5
{
    public class MinigameSequenceManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ShakerMinigameController shakerController;
        [SerializeField] private PouringMinigameTransitionController pouringTransitionController;
        [SerializeField] private ServingController servingController;
        [SerializeField] private FruitBeltMinigameManager fruitBeltManager;
        [SerializeField] private GameObject shakerVisualRoot;
        [SerializeField] private GameObject shakerUIRoot;
        [SerializeField] private GameObject ingredientsBeltRoot;
        [SerializeField] private GameObject textBubbleRoot;

        [Header("Settings")]
        [SerializeField] private Key startKey = Key.Space;
        [SerializeField] private float fruitBeltBoostMultiplier = 5.0f;

        [Header("FreshTime")]

        [SerializeField] private float freshTimeAutoStartDelay = 1.5f;

        private bool _isShaking;
        private bool _freshTimeActive;
        private float _autoStartTimer;
        private CustomerController _activeCustomer;

        public void SetFreshTimeActive(bool active)
        {
            _freshTimeActive = active;
            _autoStartTimer = 0f; // Reset timer when state changes

            if (fruitBeltManager != null)
            {
                fruitBeltManager.SetFreshTimeMode(active);
            }
        }

        private void Awake()
        {
            if (pouringTransitionController != null)
            {
                pouringTransitionController.SetStartInputLocked(true);
            }
        }

        private void OnEnable()
        {
            if (shakerController != null)
            {
                shakerController.MinigameFinished += OnShakerFinished;
            }

            if (fruitBeltManager != null)
            {
                fruitBeltManager.OnMinigameComplete += OnFruitBeltFinished;
            }

            if (pouringTransitionController != null)
            {
                pouringTransitionController.MinigameExitCompleted += OnPouringExitCompleted;
            }
        }

        private void OnDisable()
        {
            if (shakerController != null)
            {
                shakerController.MinigameFinished -= OnShakerFinished;
            }

            if (fruitBeltManager != null)
            {
                fruitBeltManager.OnMinigameComplete -= OnFruitBeltFinished;
            }

            if (pouringTransitionController != null)
            {
                pouringTransitionController.MinigameExitCompleted -= OnPouringExitCompleted;
            }

            if (_activeCustomer != null)
            {
                _activeCustomer.OnCustomerLeft -= HandleActiveCustomerLeft;
                _activeCustomer = null;
            }
        }

        private void Update()
        {
            if (_isShaking) return;
            if (fruitBeltManager != null && fruitBeltManager.IsPlaying) return;
            if (pouringTransitionController != null && pouringTransitionController.IsPlaying) return;
            if (servingController != null && servingController.IsWaitingForServe) return;

            if (_freshTimeActive)
            {
                // Only start if a customer is waiting
                if (FindWaitingCustomerDrinkData() != null)
                {
                    _autoStartTimer += Time.deltaTime;
                    if (_autoStartTimer >= freshTimeAutoStartDelay)
                    {
                        TryStartSequence();
                        _autoStartTimer = 0f;
                    }
                }
                else
                {
                    _autoStartTimer = 0f;
                }
                return;
            }

            if (Keyboard.current == null) return;

            if (Keyboard.current[startKey].wasPressedThisFrame)
            {
                TryStartSequence();
            }
        }

        private void TryStartSequence()
        {
            // Only start if a customer is waiting
            _activeCustomer = FindWaitingCustomer();
            if (_activeCustomer == null) return;

            var drinkData = _activeCustomer.CurrentOrder;

            // Subscribe to customer leaving (impatience)
            _activeCustomer.OnCustomerLeft += HandleActiveCustomerLeft;

            if (fruitBeltManager != null)
            {
                StartFruitBelt(drinkData);
            }
            else
            {
                StartShaker();
            }
        }

        private void HandleActiveCustomerLeft(CustomerController customer)
        {
            if (customer != _activeCustomer) return;

            // Unsubscribe immediately
            _activeCustomer.OnCustomerLeft -= HandleActiveCustomerLeft;

            Debug.Log($"[SequenceManager] Active customer left! Force stopping minigame.");
            ForceStopAllMinigames();
            _activeCustomer = null;
        }

        private void ForceStopAllMinigames()
        {
            if (fruitBeltManager != null && fruitBeltManager.IsPlaying)
            {
                fruitBeltManager.StopMinigame();
                if (ingredientsBeltRoot != null) ingredientsBeltRoot.SetActive(false);
            }

            if (_isShaking)
            {
                _isShaking = false;
                shakerController?.EndMinigame();
                if (shakerVisualRoot != null) shakerVisualRoot.SetActive(false);
                if (shakerUIRoot != null) shakerUIRoot.SetActive(false);
            }

            if (pouringTransitionController != null && pouringTransitionController.IsPlaying)
            {
                pouringTransitionController.EndMinigame();
            }

            ResolveTextBubbleRootIfNeeded();
            if (textBubbleRoot != null) textBubbleRoot.SetActive(true);
        }

        private void StartFruitBelt(DrinkData drinkData)
        {
            ResolveTextBubbleRootIfNeeded();
            if (textBubbleRoot != null) textBubbleRoot.SetActive(false);
            if (ingredientsBeltRoot != null) ingredientsBeltRoot.SetActive(true);

            if (fruitBeltManager != null)
            {
                fruitBeltManager.SetFreshTimeMode(_freshTimeActive);
                fruitBeltManager.StartMinigame(drinkData);
            }
        }

        private void OnFruitBeltFinished(float score)
        {
            if (ingredientsBeltRoot != null) ingredientsBeltRoot.SetActive(false);

            // If customer left during belt, don't start shaker
            if (_activeCustomer == null) return;

            if (BoostMode.Instance != null)
            {
                BoostMode.Instance.AddBoostPoints(score * fruitBeltBoostMultiplier);
            }

            StartShaker();
        }

        private void StartShaker()
        {
            _isShaking = true;
            
            ResolveTextBubbleRootIfNeeded();
            if (textBubbleRoot != null) textBubbleRoot.SetActive(false);
            if (shakerVisualRoot != null) shakerVisualRoot.SetActive(true);
            if (shakerUIRoot != null) shakerUIRoot.SetActive(true);

            shakerController?.BeginMinigame();
        }

        private void ResolveTextBubbleRootIfNeeded()
        {
            if (textBubbleRoot != null) return;

            var bubble = FindFirstObjectByType<Refresh.TextBubble>(FindObjectsInactive.Exclude);
            if (bubble == null)
            {
                bubble = FindFirstObjectByType<Refresh.TextBubble>(FindObjectsInactive.Include);
            }

            if (bubble != null)
            {
                textBubbleRoot = bubble.gameObject;
            }
        }

        private void OnShakerFinished()
        {
            _isShaking = false;

            if (shakerVisualRoot != null) shakerVisualRoot.SetActive(false);
            if (shakerUIRoot != null) shakerUIRoot.SetActive(false);
            
            // If customer left during shaker, don't start pouring
            if (_activeCustomer == null) return;

            // Now start the pouring minigame
            if (pouringTransitionController != null)
            {
                // We bypass the locked input by calling StartMinigame directly
                pouringTransitionController.StartMinigame();
            }
        }

        private void OnPouringExitCompleted()
        {
            // The whole sequence for this customer is finished normally
            if (_activeCustomer != null)
            {
                _activeCustomer.OnCustomerLeft -= HandleActiveCustomerLeft;
                _activeCustomer = null;
            }
        }

        private CustomerController FindWaitingCustomer()
        {
            var customers = FindObjectsByType<CustomerController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (var i = 0; i < customers.Length; i++)
            {
                if (customers[i].State == CustomerController.CustomerState.Waiting)
                {
                    return customers[i];
                }
            }
            return null;
        }

        private DrinkData FindWaitingCustomerDrinkData()
        {
            var customer = FindWaitingCustomer();
            return customer != null ? customer.CurrentOrder : null;
        }
        }
        }
