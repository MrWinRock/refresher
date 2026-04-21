using UnityEngine;
using UnityEngine.UI;

namespace Refresh
{
    public class OrderBubble : MonoBehaviour
    {
        [SerializeField] private GameObject bubbleRoot;
        [SerializeField] private Image bubbleBackground;
        [SerializeField] private Image drinkIcon;
        [SerializeField] private CanvasGroup bubbleCanvasGroup;

        private Sprite _defaultBackgroundSprite;

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
        }

        public void Hide()
        {

            if (drinkIcon != null)
            {
                drinkIcon.enabled = false;
            }

            if (bubbleBackground != null)
            {
                bubbleBackground.enabled = false;
            }

            SetVisible(false);
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

