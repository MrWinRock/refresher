using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Refresh
{
    public class OrderBubble : MonoBehaviour
    {
        [SerializeField] private GameObject bubbleRoot;
        [SerializeField] private Image drinkIcon;
        [SerializeField] private TMP_Text drinkNameText;

        private void Awake()
        {
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

            if (drinkNameText != null)
            {
                drinkNameText.text = drinkData.drinkName;
            }

            SetVisible(true);
        }

        public void Hide()
        {
            if (drinkNameText != null)
            {
                drinkNameText.text = string.Empty;
            }

            if (drinkIcon != null)
            {
                drinkIcon.sprite = null;
                drinkIcon.enabled = false;
            }

            SetVisible(false);
        }

        private void SetVisible(bool isVisible)
        {
            if (bubbleRoot != null)
            {
                bubbleRoot.SetActive(isVisible);
                return;
            }

            gameObject.SetActive(isVisible);
        }
    }
}

