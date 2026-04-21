using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace Minigame.ShakerMinigame
{
    public class ShakerNoteController : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private TMP_Text arrowLabel;
        [SerializeField] private Image approachCircle;
        [SerializeField] private float initialApproachScale = 2.5f;
        [SerializeField] private RectTransform baseCircle;
        [SerializeField] private bool autoMatchBaseCircleScale = true;
        [SerializeField] private float manualHitScale = 1f;
        [SerializeField] private float visualHitTimeOffsetSeconds;
        [SerializeField] private float latePhaseEndScale = 0.85f;

        [Header("Approach Feel")]
#if DOTWEEN_ENABLED
        [SerializeField] private Ease _approachEase = Ease.Linear;
        [SerializeField] private Ease _lateEase = Ease.InSine;
#endif

        [Header("Timeout Animation")]
        [SerializeField] private CanvasGroup noteCanvasGroup;
        [SerializeField] private float timeoutDropDistance = 180f;
        [SerializeField] private float timeoutDuration = 0.28f;
        [SerializeField] private float timeoutRotation = -28f;
        [SerializeField] private float timeoutAnticipationScale = 1.05f;

#if DOTWEEN_ENABLED
        [SerializeField] private Ease _timeoutDropEase = Ease.InCubic;
        [SerializeField] private Ease _timeoutFadeEase = Ease.OutQuad;
#endif

#if DOTWEEN_ENABLED
        private Sequence _approachSequence;
        private Sequence _timeoutSequence;
#endif

        private Action<ShakerNoteController, float> _expiredCallback;
        private bool _isResolved;
        private float _targetScaleAtHit;
        private float _visualHitTime;
        private RectTransform _rectTransform;
        private Vector3 _initialLocalScale;

        public ArrowDirection Direction { get; private set; }
        public float TargetHitTime { get; private set; }
        public float SpawnTime { get; private set; }
        public float ExpireTime { get; private set; }

        public void Initialize(ArrowDirection direction, float spawnTime, float targetHitTime, float expireTime, Action<ShakerNoteController, float> onExpired)
        {
            Direction = direction;
            SpawnTime = spawnTime;
            TargetHitTime = targetHitTime;
            ExpireTime = expireTime;
            _expiredCallback = onExpired;
            _isResolved = false;
            _targetScaleAtHit = ResolveTargetScaleAtHit();
            _visualHitTime = TargetHitTime + visualHitTimeOffsetSeconds;
            _rectTransform ??= transform as RectTransform;
            _initialLocalScale = transform.localScale;

            if (arrowLabel != null)
            {
                arrowLabel.text = DirectionToGlyph(direction);
            }

            if (noteCanvasGroup == null)
            {
                noteCanvasGroup = GetComponent<CanvasGroup>();
            }

            if (noteCanvasGroup == null)
            {
                noteCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            noteCanvasGroup.alpha = 1f;
            transform.localRotation = Quaternion.identity;
            transform.localScale = _initialLocalScale;

            if (approachCircle != null)
            {
                approachCircle.rectTransform.localScale = Vector3.one * initialApproachScale;
            }

#if DOTWEEN_ENABLED
            BuildApproachSequence();
#endif
        }

        private void Update()
        {
            if (_isResolved)
            {
                return;
            }

            var now = Time.time;

            if (approachCircle != null)
            {
#if DOTWEEN_ENABLED
                // DOTween drives approach-circle scale for smoother motion.
                return;
#else
                float scale;

                if (now > _visualHitTime)
                {
                    // Keep shrinking during late window so visual timing stays readable.
                    var lateProgress = Mathf.InverseLerp(_visualHitTime, ExpireTime, now);
                    var endScale = Mathf.Min(_targetScaleAtHit, latePhaseEndScale);
                    lateProgress = Mathf.SmoothStep(0f, 1f, lateProgress);
                    scale = Mathf.Lerp(_targetScaleAtHit, endScale, lateProgress);
                }
                else
                {
                    var progress = Mathf.InverseLerp(SpawnTime, _visualHitTime, now);
                    progress = Mathf.SmoothStep(0f, 1f, progress);
                    scale = Mathf.Lerp(initialApproachScale, _targetScaleAtHit, progress);
                }

                approachCircle.rectTransform.localScale = new Vector3(scale, scale, 1f);
#endif
            }

            if (now >= ExpireTime)
            {
                _isResolved = true;
                _expiredCallback?.Invoke(this, now);
            }
        }

        private float ResolveTargetScaleAtHit()
        {
            if (!autoMatchBaseCircleScale || approachCircle == null || baseCircle == null)
            {
                return Mathf.Max(0.01f, manualHitScale);
            }

            var approachRect = approachCircle.rectTransform.rect;
            var baseRect = baseCircle.rect;
            if (approachRect.width <= 0.001f || approachRect.height <= 0.001f)
            {
                return Mathf.Max(0.01f, manualHitScale);
            }

            var baseScaledWidth = baseRect.width * Mathf.Abs(baseCircle.localScale.x);
            var baseScaledHeight = baseRect.height * Mathf.Abs(baseCircle.localScale.y);

            var ratioX = baseScaledWidth / approachRect.width;
            var ratioY = baseScaledHeight / approachRect.height;
            var autoScale = (ratioX + ratioY) * 0.5f;

            return Mathf.Max(0.01f, autoScale);
        }

        public bool Matches(ArrowDirection direction)
        {
            return !_isResolved && Direction == direction;
        }

        public bool TryResolve()
        {
            if (_isResolved)
            {
                return false;
            }

            _isResolved = true;
            return true;
        }

        public void PlayTimeoutAnimation(Action onComplete)
        {
            var rectTransform = _rectTransform != null ? _rectTransform : transform as RectTransform;
            if (rectTransform == null)
            {
                onComplete?.Invoke();
                return;
            }

#if DOTWEEN_ENABLED
            _approachSequence?.Kill(false);
            _timeoutSequence?.Kill(false);

            var startPos = rectTransform.anchoredPosition;
            var targetPos = startPos + Vector2.down * timeoutDropDistance;
            var startRot = rectTransform.localEulerAngles;
            var targetRot = startRot + new Vector3(0f, 0f, timeoutRotation);
            var startScale = transform.localScale;

            _timeoutSequence = DOTween.Sequence();
            _timeoutSequence.Append(transform.DOScale(startScale * timeoutAnticipationScale, timeoutDuration * 0.2f).SetEase(Ease.OutQuad));
            _timeoutSequence.Append(rectTransform.DOAnchorPos(targetPos, timeoutDuration * 0.8f).SetEase(_timeoutDropEase));
            _timeoutSequence.Join(rectTransform.DOLocalRotate(targetRot, timeoutDuration * 0.8f).SetEase(_timeoutDropEase));
            _timeoutSequence.Join(transform.DOScale(startScale * 0.9f, timeoutDuration * 0.8f).SetEase(Ease.InQuad));

            if (noteCanvasGroup != null)
            {
                _timeoutSequence.Join(noteCanvasGroup.DOFade(0f, timeoutDuration).SetEase(_timeoutFadeEase));
            }

            _timeoutSequence.OnComplete(() => onComplete?.Invoke());
#else
            StartCoroutine(FallbackTimeoutAnimation(rectTransform, onComplete));
#endif
        }

#if DOTWEEN_ENABLED
        private void BuildApproachSequence()
        {
            if (approachCircle == null)
            {
                return;
            }

            _approachSequence?.Kill(false);
            var approachRect = approachCircle.rectTransform;
            approachRect.localScale = Vector3.one * initialApproachScale;

            var visualHitAt = Mathf.Max(SpawnTime + 0.01f, _visualHitTime);
            var phaseOneDuration = Mathf.Max(0.01f, visualHitAt - SpawnTime);
            var phaseTwoDuration = Mathf.Max(0.01f, ExpireTime - visualHitAt);
            var endScale = Mathf.Min(_targetScaleAtHit, latePhaseEndScale);

            _approachSequence = DOTween.Sequence();
            _approachSequence.Append(approachRect.DOScale(_targetScaleAtHit, phaseOneDuration).SetEase(_approachEase));
            _approachSequence.Append(approachRect.DOScale(endScale, phaseTwoDuration).SetEase(_lateEase));
        }

        private void OnDisable()
        {
            _approachSequence?.Kill(false);
            _timeoutSequence?.Kill(false);
        }
#endif

#if !DOTWEEN_ENABLED
        private IEnumerator FallbackTimeoutAnimation(RectTransform rectTransform, Action onComplete)
        {
            var elapsed = 0f;
            var startPos = rectTransform.anchoredPosition;
            var endPos = startPos + Vector2.down * timeoutDropDistance;
            var startRot = rectTransform.localEulerAngles;
            var endRot = startRot + new Vector3(0f, 0f, timeoutRotation);

            var baseAlpha = noteCanvasGroup != null ? noteCanvasGroup.alpha : 1f;

            while (elapsed < timeoutDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / timeoutDuration);

                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                rectTransform.localEulerAngles = Vector3.Lerp(startRot, endRot, t);

                if (noteCanvasGroup != null)
                {
                    noteCanvasGroup.alpha = Mathf.Lerp(baseAlpha, 0f, t);
                }

                yield return null;
            }

            onComplete?.Invoke();
        }
#endif

        private static string DirectionToGlyph(ArrowDirection direction)
        {
            return direction switch
            {
                ArrowDirection.Up => "↑",
                ArrowDirection.Down => "↓",
                ArrowDirection.Left => "←",
                ArrowDirection.Right => "→",
                _ => "?"
            };
        }
    }
}




