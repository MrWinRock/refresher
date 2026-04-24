using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Refresh
{
    public class TextBubble : MonoBehaviour
    {
        [SerializeField] private GameObject bubbleRoot;
        [SerializeField] private Image bubbleBackground;
        [SerializeField] private CanvasGroup bubbleCanvasGroup;

        [Header("DOTween")]
        [SerializeField] private float showFromScale = 0.82f;
        [SerializeField] private float showScaleDuration = 0.18f;
        [SerializeField] private float showFadeDuration = 0.12f;
        [SerializeField] private Ease showEase = Ease.OutBack;
        [SerializeField] private float punchScale = 0.07f;
        [SerializeField] private float punchDuration = 0.2f;
        [SerializeField] private int punchVibrato = 8;

        [Header("Loop Animation")]
        [SerializeField] private bool useLoopAnimation = true;
        [SerializeField] private float loopMoveY = 6f;
        [SerializeField] private float loopDuration = 0.6f;
        [SerializeField] private Ease loopEase = Ease.InOutSine;

        private Tween _showTween;
        private Tween _fadeTween;
        private Tween _punchTween;
        private Tween _loopTween;
        private Vector3 _defaultLocalPosition;
        private bool _hasDefaultLocalPosition;

        private void Awake()
        {
            if (bubbleCanvasGroup == null)
            {
                var target = bubbleRoot != null ? bubbleRoot : gameObject;
                bubbleCanvasGroup = target.GetComponent<CanvasGroup>();
            }

            EnsureTextLabelExists();
            CacheDefaultLocalPosition();
            Hide();
        }

        private void OnDisable()
        {
            Hide();
        }


        public void ShowText(string content)
        {
            EnsureTextLabelExists();

            if (bubbleBackground != null)
            {
                bubbleBackground.enabled = true;
            }

            SetVisible(true);
            PlayShowTween();
            PlayLoopTween();
        }

        public void Hide()
        {
            KillTweens();

            if (bubbleBackground != null)
            {
                bubbleBackground.enabled = false;
            }

            ResetAnimatedTransform();

            SetVisible(false);
        }

        private void PlayShowTween()
        {
            var targetTransform = bubbleRoot != null ? bubbleRoot.transform : transform;
            if (targetTransform == null)
            {
                return;
            }

            KillTweens();

            targetTransform.localScale = Vector3.one * Mathf.Max(0.01f, showFromScale);
            _showTween = targetTransform.DOScale(Vector3.one, showScaleDuration).SetEase(showEase);

            if (bubbleCanvasGroup != null)
            {
                bubbleCanvasGroup.alpha = 0f;
                _fadeTween = DOTween.To(() => bubbleCanvasGroup.alpha, value => bubbleCanvasGroup.alpha = value, 1f, showFadeDuration)
                    .SetEase(Ease.OutSine);
            }

            if (punchScale > 0f && punchDuration > 0f)
            {
                _punchTween = targetTransform.DOPunchScale(Vector3.one * punchScale, punchDuration, punchVibrato, 0.8f)
                    .SetDelay(showScaleDuration * 0.65f);
            }
        }

        private void PlayLoopTween()
        {
            if (!useLoopAnimation || loopDuration <= 0f || Mathf.Approximately(loopMoveY, 0f))
            {
                return;
            }

            var targetTransform = bubbleRoot != null ? bubbleRoot.transform : transform;
            if (targetTransform == null)
            {
                return;
            }

            CacheDefaultLocalPosition();
            targetTransform.localPosition = _defaultLocalPosition;

            _loopTween = targetTransform.DOLocalMoveY(_defaultLocalPosition.y + loopMoveY, loopDuration)
                .SetEase(loopEase)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(showScaleDuration);
        }

        private void SetVisible(bool isVisible)
        {
            if (bubbleCanvasGroup != null)
            {
                bubbleCanvasGroup.alpha = isVisible ? 1f : 0f;
                bubbleCanvasGroup.interactable = isVisible;
                bubbleCanvasGroup.blocksRaycasts = isVisible;
                return;
            }

            if (bubbleRoot != null)
            {
                bubbleRoot.SetActive(isVisible);
                return;
            }

            gameObject.SetActive(isVisible);
        }

        private void EnsureTextLabelExists()
        {
            if (bubbleRoot == null)
            {
                bubbleRoot = gameObject;
            }
            
            var labelRoot = bubbleBackground != null
                ? bubbleBackground.GetComponent<RectTransform>()
                : bubbleRoot.GetComponent<RectTransform>();

            if (labelRoot == null)
            {
                return;
            }

            var textGo = new GameObject("DrinkLabel");
            textGo.transform.SetParent(labelRoot, false);

            var rect = textGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void CacheDefaultLocalPosition()
        {
            if (_hasDefaultLocalPosition)
            {
                return;
            }

            var targetTransform = bubbleRoot != null ? bubbleRoot.transform : transform;
            if (targetTransform == null)
            {
                return;
            }

            _defaultLocalPosition = targetTransform.localPosition;
            _hasDefaultLocalPosition = true;
        }

        private void ResetAnimatedTransform()
        {
            var targetTransform = bubbleRoot != null ? bubbleRoot.transform : transform;
            if (targetTransform == null)
            {
                return;
            }

            if (_hasDefaultLocalPosition)
            {
                targetTransform.localPosition = _defaultLocalPosition;
            }

            targetTransform.localScale = Vector3.one;
        }

        private void KillTweens()
        {
            if (_showTween != null && _showTween.IsActive())
            {
                _showTween.Kill();
            }

            if (_fadeTween != null && _fadeTween.IsActive())
            {
                _fadeTween.Kill();
            }

            if (_punchTween != null && _punchTween.IsActive())
            {
                _punchTween.Kill();
            }

            if (_loopTween != null && _loopTween.IsActive())
            {
                _loopTween.Kill();
            }

            _showTween = null;
            _fadeTween = null;
            _punchTween = null;
            _loopTween = null;
        }
    }
}

