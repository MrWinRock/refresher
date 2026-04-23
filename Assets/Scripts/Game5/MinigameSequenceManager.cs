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
        [SerializeField] private GameObject shakerVisualRoot;
[SerializeField] private GameObject shakerUIRoot;
        [SerializeField] private GameObject textBubbleRoot;

        [Header("Settings")]
        [SerializeField] private Key startKey = Key.Space;

        [Header("FreshTime")]
        [SerializeField] private float freshTimeAutoStartDelay = 1.5f;

        private bool _isShaking;
        private bool _freshTimeActive;
        private float _autoStartTimer;

        public void SetFreshTimeActive(bool active)
        {
            _freshTimeActive = active;
            _autoStartTimer = 0f; // Reset timer when state changes
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
        }

        private void OnDisable()
        {
            if (shakerController != null)
            {
                shakerController.MinigameFinished -= OnShakerFinished;
            }
        }

        private void Update()
        {
            if (_isShaking) return;
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
            var drinkData = FindWaitingCustomerDrinkData();
            if (drinkData == null) return;

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
            
            // Now start the pouring minigame
            if (pouringTransitionController != null)
            {
                // We bypass the locked input by calling StartMinigame directly
                pouringTransitionController.StartMinigame();
            }
        }

        private DrinkData FindWaitingCustomerDrinkData()
        {
            var customers = FindObjectsByType<CustomerController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (var i = 0; i < customers.Length; i++)
            {
                if (customers[i].State == CustomerController.CustomerState.Waiting)
                {
                    return customers[i].CurrentOrder;
                }
            }
            return null;
        }
    }
}
