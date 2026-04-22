using DG.Tweening;
using Refresh;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game5
{
    [DisallowMultipleComponent]
    public class ServingController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PouringMinigameTransitionController transitionController;
        [SerializeField] private GameObject preparedDrinkRoot;
        [SerializeField] private SpriteRenderer preparedDrinkRenderer;

        [Header("Input")]
        [SerializeField] private Key serveKey = Key.Space;

        [Header("Prepared Drink")]
        [SerializeField] private bool useCurrentPreparedShownPosition = true;
        [SerializeField] private Vector3 preparedShownLocalPosition = Vector3.zero;
        [SerializeField] private float preparedHiddenOffsetY = -8.81f;
        [SerializeField] private float preparedTransitionDuration = 0.35f;
        [SerializeField] private Ease preparedTransitionEase = Ease.OutCubic;

        [Header("Serve Animation")]
        [SerializeField] private Vector3 servedForwardLocalOffset = new Vector3(0f, 1.6f, 0f);
        [SerializeField] private float servedForwardDuration = 0.2f;
        [SerializeField] private Ease servedForwardEase = Ease.OutCubic;

        private Tween _preparedDrinkTween;
        private Vector3 _preparedShownLocalPosition;
        private Vector3 _preparedHiddenLocalPosition;
        private bool _hasCachedPreparedPosition;
        private bool _isWaitingForServe;
        private CustomerController _preparedDrinkCustomer;
        private DrinkData _preparedDrinkData;
        private Coroutine _startInputUnlockCoroutine;

        public bool IsWaitingForServe => _isWaitingForServe;

        private void Awake()
{
            ResolveRefsIfNeeded();
            CachePreparedDrinkPositions(true);
            HidePreparedDrinkInstant();
        }

        private void OnEnable()
        {
            ResolveRefsIfNeeded();
            if (transitionController != null)
            {
                transitionController.MinigameExitCompleted += HandleMinigameExitCompleted;
            }
        }

        private void OnDisable()
        {
            if (transitionController != null)
            {
                transitionController.MinigameExitCompleted -= HandleMinigameExitCompleted;
                transitionController.SetStartInputLocked(false);
            }

            KillPreparedDrinkTween();
        }

        private void Update()
        {
            if (!_isWaitingForServe || Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current[serveKey].wasPressedThisFrame)
            {
                TryServePreparedDrink();
            }
        }

        private void HandleMinigameExitCompleted()
        {
            PrepareDrinkForServe();
        }

        private void PrepareDrinkForServe()
        {
            _preparedDrinkCustomer = FindWaitingCustomer();
            _preparedDrinkData = _preparedDrinkCustomer != null ? _preparedDrinkCustomer.CurrentOrder : null;

            if (_preparedDrinkData == null)
            {
                _isWaitingForServe = false;
                transitionController?.SetStartInputLocked(false);
                HidePreparedDrinkInstant();
                return;
            }

            ShowPreparedDrink(_preparedDrinkData);
            _isWaitingForServe = true;
            transitionController?.SetStartInputLocked(true);
        }

        private void ShowPreparedDrink(DrinkData drinkData)
        {
            if (preparedDrinkRoot == null)
            {
                return;
            }

            ResolveRefsIfNeeded();
            CachePreparedDrinkPositions(false);

            if (preparedDrinkRenderer != null)
            {
                var sprite = drinkData.servedGlassSprite != null ? drinkData.servedGlassSprite : drinkData.drinkIcon;
                preparedDrinkRenderer.sprite = sprite;
                preparedDrinkRenderer.enabled = sprite != null;
            }

            preparedDrinkRoot.transform.localPosition = _preparedHiddenLocalPosition;
            preparedDrinkRoot.SetActive(true);

            KillPreparedDrinkTween();
            _preparedDrinkTween = preparedDrinkRoot.transform
                .DOLocalMove(_preparedShownLocalPosition, preparedTransitionDuration)
                .SetEase(preparedTransitionEase);
        }

        private void TryServePreparedDrink()
        {
            if (!_isWaitingForServe || _preparedDrinkData == null)
            {
                return;
            }

            if (_preparedDrinkCustomer == null || _preparedDrinkCustomer.State != CustomerController.CustomerState.Waiting)
            {
                _preparedDrinkCustomer = FindWaitingCustomer();
            }

            if (_preparedDrinkCustomer == null)
            {
                _isWaitingForServe = false;
                transitionController?.SetStartInputLocked(false);
                HidePreparedDrinkAnimated();
                return;
            }

            var wasServed = _preparedDrinkCustomer.TryServeDrink(_preparedDrinkData);
            if (!wasServed)
            {
                return;
            }

            _isWaitingForServe = false;
            // Unlock the start input on the next frame to avoid the same key press
            // being processed by the pouring minigame in the same Update cycle.
            // This prevents pressing Space to serve from immediately re-triggering
            // the pouring minigame that also listens for Space.
            StartUnlockStartInputNextFrame();
            PlayServeForwardAnimated();
            _preparedDrinkCustomer = null;
            _preparedDrinkData = null;
        }

        private void StartUnlockStartInputNextFrame()
        {
            if (transitionController == null)
            {
                return;
            }

            if (_startInputUnlockCoroutine != null)
            {
                StopCoroutine(_startInputUnlockCoroutine);
                _startInputUnlockCoroutine = null;
            }

            _startInputUnlockCoroutine = StartCoroutine(UnlockStartInputNextFrameCoroutine());
        }

        private System.Collections.IEnumerator UnlockStartInputNextFrameCoroutine()
        {
            transitionController.SetStartInputLocked(true);
            yield return null;
            yield return null;
            transitionController.SetStartInputLocked(false);
            _startInputUnlockCoroutine = null;
        }

        private CustomerController FindWaitingCustomer()
        {
            var customers = FindObjectsByType<CustomerController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (var i = 0; i < customers.Length; i++)
            {
                if (customers[i] != null && customers[i].State == CustomerController.CustomerState.Waiting)
                {
                    return customers[i];
                }
            }

            return null;
        }

        private void ResolveRefsIfNeeded()
        {
            if (transitionController == null)
            {
                transitionController = FindFirstObjectByType<PouringMinigameTransitionController>(FindObjectsInactive.Include);
            }

            if (preparedDrinkRoot != null && preparedDrinkRenderer == null)
            {
                preparedDrinkRenderer = preparedDrinkRoot.GetComponentInChildren<SpriteRenderer>(true);
            }
        }

        private void CachePreparedDrinkPositions(bool forceRecaptureShownPosition)
        {
            if (preparedDrinkRoot == null)
            {
                return;
            }

            if (useCurrentPreparedShownPosition && (forceRecaptureShownPosition || !_hasCachedPreparedPosition))
            {
                preparedShownLocalPosition = preparedDrinkRoot.transform.localPosition;
            }

            _preparedShownLocalPosition = preparedShownLocalPosition;
            _preparedHiddenLocalPosition = preparedShownLocalPosition + new Vector3(0f, preparedHiddenOffsetY, 0f);
            _hasCachedPreparedPosition = true;
        }

        private void HidePreparedDrinkAnimated()
        {
            if (preparedDrinkRoot == null)
            {
                return;
            }

            KillPreparedDrinkTween();
            _preparedDrinkTween = preparedDrinkRoot.transform
                .DOLocalMove(_preparedHiddenLocalPosition, preparedTransitionDuration)
                .SetEase(preparedTransitionEase)
                .OnComplete(() =>
                {
                    if (preparedDrinkRoot != null)
                    {
                        preparedDrinkRoot.SetActive(false);
                    }
                });
        }

        private void PlayServeForwardAnimated()
        {
            if (preparedDrinkRoot == null)
            {
                return;
            }

            var duration = servedForwardDuration > 0f ? servedForwardDuration : preparedTransitionDuration;
            var ease = servedForwardDuration > 0f ? servedForwardEase : preparedTransitionEase;
            var forwardTarget = _preparedShownLocalPosition + servedForwardLocalOffset;

            KillPreparedDrinkTween();
            _preparedDrinkTween = preparedDrinkRoot.transform
                .DOLocalMove(forwardTarget, duration)
                .SetEase(ease)
                .OnComplete(() =>
                {
                    if (preparedDrinkRoot != null)
                    {
                        preparedDrinkRoot.SetActive(false);
                        preparedDrinkRoot.transform.localPosition = _preparedHiddenLocalPosition;
                    }
                });
        }

        private void HidePreparedDrinkInstant()
        {
            _isWaitingForServe = false;
            _preparedDrinkCustomer = null;
            _preparedDrinkData = null;

            if (preparedDrinkRoot == null)
            {
                return;
            }

            KillPreparedDrinkTween();
            preparedDrinkRoot.transform.localPosition = _preparedHiddenLocalPosition;
            preparedDrinkRoot.SetActive(false);
        }

        private void KillPreparedDrinkTween()
        {
            if (_preparedDrinkTween != null && _preparedDrinkTween.IsActive())
            {
                _preparedDrinkTween.Kill();
            }

            _preparedDrinkTween = null;
        }
    }
}
