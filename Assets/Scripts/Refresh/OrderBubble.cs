using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Refresh
{
    public class OrderBubble : MonoBehaviour
    {
        [SerializeField] private GameObject bubbleRoot;
        [SerializeField] private Image bubbleBackground;
        [SerializeField] private Image drinkIcon;
        [SerializeField] private CanvasGroup bubbleCanvasGroup;

        [Header("DOTween")]
        [SerializeField] private float popupFromScale = 0.82f;
        [SerializeField] private float popupScaleDuration = 0.18f;
        [SerializeField] private float popupFadeDuration = 0.12f;
        [SerializeField] private Ease popupEase = Ease.OutBack;

        private Sprite _defaultBackgroundSprite;
        private Tween _popupTween;
        private Tween _fadeTween;

        private void Awake()
        {
            if (bubbleCanvasGroup == null)
            {
                var target = bubbleRoot != null ? bubbleRoot : gameObject;
                bubbleCanvasGroup = target.GetComponent<CanvasGroup>();
            }

            if (bubbleBackground != null)
            {
                _defaultBackgroundSprite = bubbleBackground.sprite;
            }

            Hide();
        }

        public void ShowOrder(DrinkData drinkData)
        {
            if (drinkData == null)
            {
                Hide();
                return;
            }

            if (drinkIcon != null)
            {
                drinkIcon.sprite = drinkData.drinkIcon;
                drinkIcon.enabled = drinkData.drinkIcon != null;
            }

            if (bubbleBackground != null)
            {
                if (bubbleBackground.sprite == null && _defaultBackgroundSprite != null)
                {
                    bubbleBackground.sprite = _defaultBackgroundSprite;
                }

                bubbleBackground.enabled = true;
            }

            SetVisible(true);
            PlayPopupTween();
        }

        public void Hide()
        {
            KillTweens();

            if (drinkIcon != null)
            {
                drinkIcon.enabled = false;
            }

            if (bubbleBackground != null)
            {
                bubbleBackground.enabled = false;
            }

            var targetTransform = bubbleRoot != null ? bubbleRoot.transform : transform;
            if (targetTransform != null)
            {
                targetTransform.localScale = Vector3.one;
            }

            SetVisible(false);
        }

        private void PlayPopupTween()
        {
            var targetTransform = bubbleRoot != null ? bubbleRoot.transform : transform;
            if (targetTransform == null)
            {
                return;
            }

            KillTweens();

            targetTransform.localScale = Vector3.one * Mathf.Max(0.01f, popupFromScale);
            _popupTween = targetTransform.DOScale(Vector3.one, popupScaleDuration).SetEase(popupEase);

            if (bubbleCanvasGroup != null)
            {
                bubbleCanvasGroup.alpha = 0f;
                _fadeTween = DOTween.To(() => bubbleCanvasGroup.alpha, value => bubbleCanvasGroup.alpha = value, 1f, popupFadeDuration)
                    .SetEase(Ease.OutSine);
            }
        }

        private void KillTweens()
        {
            if (_popupTween != null && _popupTween.IsActive())
            {
                _popupTween.Kill();
            }

            if (_fadeTween != null && _fadeTween.IsActive())
            {
                _fadeTween.Kill();
            }

            _popupTween = null;
            _fadeTween = null;
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
    }
}

