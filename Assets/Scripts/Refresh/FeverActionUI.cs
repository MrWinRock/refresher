using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

namespace Refresh
{
    public class FeverActionUI : MonoBehaviour
    {
        [SerializeField] private Image reactionImage;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private float punchScale = 1.2f;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (reactionImage != null) reactionImage.gameObject.SetActive(false);
        }

        public IEnumerator ShowReaction(Sprite reactionSprite, float duration)
        {
            if (reactionImage == null || canvasGroup == null) yield break;

            reactionImage.sprite = reactionSprite;
            reactionImage.gameObject.SetActive(true);
            
            // Fade in
            canvasGroup.DOFade(1f, fadeInDuration);
            reactionImage.transform.localScale = Vector3.one * 0.5f;
            reactionImage.transform.DOScale(Vector3.one, fadeInDuration).SetEase(Ease.OutBack);
            
            yield return new WaitForSeconds(fadeInDuration);
            
            // Stay and punch
            reactionImage.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f, 5, 0.5f);
            
            yield return new WaitForSeconds(duration);
            
            // Fade out
            canvasGroup.DOFade(0f, fadeOutDuration);
            yield return new WaitForSeconds(fadeOutDuration);
            
            reactionImage.gameObject.SetActive(false);
        }
    }
}
