using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game5
{
    [DisallowMultipleComponent]
    public class PouringMinigameTransitionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject textBubbleRoot;
        [SerializeField] private GameObject pouringMinigameRoot;
        [SerializeField] private PouringMiniGameController pouringMiniGameController;
        [SerializeField] private bool autoFindTextBubbleRoot = true;

        [Header("Input")]
        [SerializeField] private Key startKey = Key.Space;

        [Header("Transition")]
        [SerializeField] private bool useCurrentPositionAsShownPosition = true;
        [SerializeField] private Vector2 shownAnchoredPosition = Vector2.zero;
        [SerializeField] private Vector3 shownLocalPosition = Vector3.zero;
        [SerializeField] private float hiddenOffsetY = -1200f;
        [SerializeField] private float transitionDuration = 0.35f;
        [SerializeField] private Ease transitionEase = Ease.OutCubic;

        private RectTransform _minigameRectTransform;
        private Vector3 _shownLocalPosition;
        private Vector3 _hiddenLocalPosition;
        private Vector2 _hiddenAnchoredPosition;
        private bool _isPlaying;
        private bool _isTransitioning;
        private Tween _transitionTween;
        private bool _hasCachedShownPosition;
        private bool _isStartInputLocked;

        public event Action MinigameExitCompleted;

        private void Awake()
        {
            if (pouringMinigameRoot != null && pouringMiniGameController == null)
            {
                pouringMiniGameController = pouringMinigameRoot.GetComponentInChildren<PouringMiniGameController>(true);
            }

            if (pouringMinigameRoot != null)
            {
                _minigameRectTransform = pouringMinigameRoot.GetComponent<RectTransform>();
            }

            CachePositions(true);
            ResolveTextBubbleRootIfNeeded();
            SnapToHiddenState();
        }

        private void OnEnable()
        {
            if (pouringMiniGameController != null)
            {
                pouringMiniGameController.MinigameFinished += HandleMinigameFinished;
            }
        }

        private void OnDisable()
        {
            if (pouringMiniGameController != null)
            {
                pouringMiniGameController.MinigameFinished -= HandleMinigameFinished;
            }

            KillTransitionTween();
        }

        private void Update()
        {
            if (_isPlaying || _isTransitioning)
            {
                return;
            }

            if (_isStartInputLocked)
            {
                return;
            }

            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current[startKey].wasPressedThisFrame)
            {
                StartMinigame();
            }
        }

        public void StartMinigame()
        {
            if (_isPlaying || _isTransitioning || pouringMinigameRoot == null)
            {
                return;
            }

            CachePositions(false);
            ResolveTextBubbleRootIfNeeded();

            if (textBubbleRoot != null)
            {
                textBubbleRoot.SetActive(false);
            }

            pouringMinigameRoot.SetActive(true);
            pouringMiniGameController?.BeginMinigame();

            _isTransitioning = true;
            KillTransitionTween();
            _transitionTween = PlayMoveTween(true)
                .OnComplete(() =>
                {
                    _isTransitioning = false;
                    _isPlaying = true;
                });
        }

        public void EndMinigame()
        {
            if (!_isPlaying || _isTransitioning || pouringMinigameRoot == null)
            {
                return;
            }

            _isTransitioning = true;
            KillTransitionTween();
            _transitionTween = PlayMoveTween(false)
                .OnComplete(() =>
                {
                    _isTransitioning = false;
                    _isPlaying = false;
                    pouringMinigameRoot.SetActive(false);

                    if (textBubbleRoot != null)
                    {
                        textBubbleRoot.SetActive(true);
                    }

                    MinigameExitCompleted?.Invoke();
                });
        }

        private void HandleMinigameFinished()
        {
            EndMinigame();
        }

        private void ResolveTextBubbleRootIfNeeded()
        {
            if (!autoFindTextBubbleRoot || textBubbleRoot != null)
            {
                return;
            }

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

        private void CachePositions(bool forceRecaptureShownPosition)
        {
            if (pouringMinigameRoot == null)
            {
                return;
            }

            if (_minigameRectTransform != null)
            {
                if (useCurrentPositionAsShownPosition && (forceRecaptureShownPosition || !_hasCachedShownPosition))
                {
                    shownAnchoredPosition = _minigameRectTransform.anchoredPosition;
                }

                _hiddenAnchoredPosition = shownAnchoredPosition + new Vector2(0f, hiddenOffsetY);
                _hasCachedShownPosition = true;
                return;
            }

            if (useCurrentPositionAsShownPosition && (forceRecaptureShownPosition || !_hasCachedShownPosition))
            {
                shownLocalPosition = pouringMinigameRoot.transform.localPosition;
            }

            _shownLocalPosition = shownLocalPosition;
            _hiddenLocalPosition = shownLocalPosition + new Vector3(0f, hiddenOffsetY, 0f);
            _hasCachedShownPosition = true;
        }

        private void SnapToHiddenState()
        {
            if (pouringMinigameRoot == null)
            {
                return;
            }

            if (_minigameRectTransform != null)
            {
                _minigameRectTransform.anchoredPosition = _hiddenAnchoredPosition;
            }
            else
            {
                pouringMinigameRoot.transform.localPosition = _hiddenLocalPosition;
            }

            _isPlaying = false;
            _isTransitioning = false;
            pouringMinigameRoot.SetActive(false);
        }

        private Tween PlayMoveTween(bool show)
        {
            if (_minigameRectTransform != null)
            {
                var target = show ? shownAnchoredPosition : _hiddenAnchoredPosition;
                return _minigameRectTransform.DOAnchorPos(target, transitionDuration).SetEase(transitionEase);
            }

            var localTarget = show ? _shownLocalPosition : _hiddenLocalPosition;
            return pouringMinigameRoot.transform.DOLocalMove(localTarget, transitionDuration).SetEase(transitionEase);
        }

        private void KillTransitionTween()
        {
            if (_transitionTween != null && _transitionTween.IsActive())
            {
                _transitionTween.Kill();
            }

            _transitionTween = null;
        }

        public void SetStartInputLocked(bool isLocked)
        {
            _isStartInputLocked = isLocked;
        }
    }
}


