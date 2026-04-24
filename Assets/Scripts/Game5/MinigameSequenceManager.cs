using UnityEngine;
using UnityEngine.InputSystem;
using Minigame.ShakerMinigame;
using Refresh;
using DG.Tweening;

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
        [SerializeField] private GameObject orderRoot; 
        [SerializeField] private GameObject textBubbleRoot;

        [Header("Animation Settings")]
        [SerializeField] private float animDuration = 0.5f;
        [SerializeField] private Vector3 orderHiddenOffset = new Vector3(0, -10f, 0); 
        [SerializeField] private Vector3 beltHiddenOffset = new Vector3(15f, 0, 0);   
        [SerializeField] private Vector3 shakerHiddenOffset = new Vector3(0, -5f, 0);
        [SerializeField] private Ease animEaseIn = Ease.OutBack;
        [SerializeField] private Ease animEaseOut = Ease.InBack;

        [Header("Settings")]
        [SerializeField] private Key startKey = Key.Space;
        [SerializeField] private float fruitBeltBoostMultiplier = 5.0f;
        [SerializeField] private float delayBetweenMinigames = 1.0f;

        [Header("FreshTime")]

        [SerializeField] private float freshTimeAutoStartDelay = 1.5f;

        private bool _isShaking;
        private bool _freshTimeActive;
        private float _autoStartTimer;
        private CustomerController _activeCustomer;

        private Vector3 _orderHomePos;
        private Vector3 _beltHomePos;
        private Vector3 _shakerHomePos;

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

            if (orderRoot != null) _orderHomePos = orderRoot.transform.localPosition;
            if (ingredientsBeltRoot != null) _beltHomePos = ingredientsBeltRoot.transform.localPosition;
            if (shakerVisualRoot != null) _shakerHomePos = shakerVisualRoot.transform.localPosition;
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

            if (servingController != null)
            {
                servingController.OnServingFinished += OnServingFinished;
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

            if (servingController != null)
            {
                servingController.OnServingFinished -= OnServingFinished;
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
                AnimateUIOut();
            }

            if (_isShaking)
            {
                _isShaking = false;
                shakerController?.EndMinigame();
                AnimateShakerOut();
                if (shakerUIRoot != null) shakerUIRoot.SetActive(false);
            }

            if (pouringTransitionController != null && pouringTransitionController.IsPlaying)
            {
                pouringTransitionController.EndMinigame();
            }

            if (servingController != null && servingController.IsWaitingForServe)
            {
                servingController.CancelServing();
            }

            ResolveTextBubbleRootIfNeeded();
            if (textBubbleRoot != null) textBubbleRoot.SetActive(true);
            }

        private void StartFruitBelt(DrinkData drinkData)
        {
            ResolveTextBubbleRootIfNeeded();
            if (textBubbleRoot != null) textBubbleRoot.SetActive(false);
            
            AnimateUIIn();

            if (fruitBeltManager != null)
            {
                fruitBeltManager.SetFreshTimeMode(_freshTimeActive);
                fruitBeltManager.StartMinigame(drinkData);
            }
        }

        private void AnimateUIIn()
        {
            if (orderRoot != null)
            {
                orderRoot.SetActive(true);
                orderRoot.transform.DOKill();
                orderRoot.transform.localPosition = _orderHomePos + orderHiddenOffset;
                orderRoot.transform.DOLocalMove(_orderHomePos, animDuration).SetEase(animEaseIn);
            }

            if (ingredientsBeltRoot != null)
            {
                ingredientsBeltRoot.SetActive(true);
                ingredientsBeltRoot.transform.DOKill();
                ingredientsBeltRoot.transform.localPosition = _beltHomePos + beltHiddenOffset;
                ingredientsBeltRoot.transform.DOLocalMove(_beltHomePos, animDuration).SetEase(animEaseIn);
            }
        }

        private void AnimateUIOut()
        {
            if (orderRoot != null)
            {
                orderRoot.transform.DOKill();
                orderRoot.transform.DOLocalMove(_orderHomePos + orderHiddenOffset, animDuration)
                    .SetEase(animEaseOut)
                    .OnComplete(() => {
                        orderRoot.SetActive(false);
                        orderRoot.transform.localPosition = _orderHomePos;
                    });
            }

            if (ingredientsBeltRoot != null)
            {
                ingredientsBeltRoot.transform.DOKill();
                ingredientsBeltRoot.transform.DOLocalMove(_beltHomePos + beltHiddenOffset, animDuration)
                    .SetEase(animEaseOut)
                    .OnComplete(() => {
                        ingredientsBeltRoot.SetActive(false);
                        ingredientsBeltRoot.transform.localPosition = _beltHomePos;
                    });
            }
            }

            private void AnimateShakerIn()
            {
            if (shakerVisualRoot != null)
            {
                shakerVisualRoot.SetActive(true);
                shakerVisualRoot.transform.DOKill();
                shakerVisualRoot.transform.localPosition = _shakerHomePos + shakerHiddenOffset;
                shakerVisualRoot.transform.DOLocalMove(_shakerHomePos, animDuration).SetEase(animEaseIn);
            }
            }

            private void AnimateShakerOut()
            {
            if (shakerVisualRoot != null)
            {
                shakerVisualRoot.transform.DOKill();
                shakerVisualRoot.transform.DOLocalMove(_shakerHomePos + shakerHiddenOffset, animDuration)
                    .SetEase(animEaseOut)
                    .OnComplete(() => {
                        shakerVisualRoot.SetActive(false);
                        shakerVisualRoot.transform.localPosition = _shakerHomePos;
                    });
            }
            }

        private void OnFruitBeltFinished(float score)
        {
            StartCoroutine(DelayedShakerStart(score));
        }

        private System.Collections.IEnumerator DelayedShakerStart(float score)
        {
            AnimateUIOut();

            if (BoostMode.Instance != null)
            {
                BoostMode.Instance.AddBoostPoints(score * fruitBeltBoostMultiplier);
            }

            if (delayBetweenMinigames > 0)
            {
                yield return new WaitForSeconds(delayBetweenMinigames);
            }

            // If customer left during delay, don't start shaker
            if (_activeCustomer == null) yield break;

            StartShaker();
        }

        private void StartShaker()
        {
            _isShaking = true;
            
            ResolveTextBubbleRootIfNeeded();
            if (textBubbleRoot != null) textBubbleRoot.SetActive(false);
            
            AnimateShakerIn();
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
            StartCoroutine(DelayedPouringStart());
        }

        private System.Collections.IEnumerator DelayedPouringStart()
        {
            _isShaking = false;

            AnimateShakerOut();
            if (shakerUIRoot != null) shakerUIRoot.SetActive(false);

            if (delayBetweenMinigames > 0)
            {
                yield return new WaitForSeconds(delayBetweenMinigames);
            }

            // If customer left during delay, don't start pouring
            if (_activeCustomer == null) yield break;

            // Now start the pouring minigame
            if (pouringTransitionController != null)
            {
                // We bypass the locked input by calling StartMinigame directly
                pouringTransitionController.StartMinigame();
            }
        }

        private void OnPouringExitCompleted()
        {
            // We no longer clear the active customer here, 
            // as they might still be waiting for the drink to be served.
            // Cleanup now happens in OnServingFinished or HandleActiveCustomerLeft.
        }

        private void OnServingFinished()
        {
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
