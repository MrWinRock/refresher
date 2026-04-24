using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

namespace Refresh
{
    public class FeverActionUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform container;
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.5f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip reactionSfx; // FeverSplash

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        public IEnumerator ShowReactionByName(string objectName, Sprite optionalSprite, float duration)
        {
            if (canvasGroup == null) yield break;

            // Find the object by name in children (even inactive)
            Transform reactionTransform = null;
            Transform panelTransform = null;
            
            foreach (Transform child in transform)
            {
                if (child.name == objectName) reactionTransform = child;
                if (child.name == "Panel") panelTransform = child;
            }

            if (reactionTransform == null)
            {
                Debug.LogWarning($"FeverActionUI: Could not find reaction object named '{objectName}' under {name}");
                yield break;
            }

            // Deactivate all reaction objects first except Panel and Container
            foreach (Transform child in transform)
            {
                if (child != container && child != panelTransform)
                {
                    child.gameObject.SetActive(false);
                }
            }

            if (panelTransform != null) panelTransform.gameObject.SetActive(true);
            
            GameObject instance = reactionTransform.gameObject;
            instance.SetActive(true);

            PlaySFX();

            if (optionalSprite != null)
            {
                var img = instance.GetComponent<UnityEngine.UI.Image>();
                if (img == null) img = instance.GetComponentInChildren<UnityEngine.UI.Image>();
                if (img != null) img.sprite = optionalSprite;
            }
            
            // Fade in master UI
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, fadeInDuration).SetUpdate(true);
            
            // Scale animation
            Vector3 originalScale = reactionTransform.localScale;
            reactionTransform.localScale = originalScale * 0.5f;
            reactionTransform.DOScale(originalScale, fadeInDuration).SetEase(Ease.OutBack).SetUpdate(true);
            
            yield return new WaitForSeconds(fadeInDuration + duration);
            
            // Fade out
            canvasGroup.DOFade(0f, fadeOutDuration).SetUpdate(true);
            
            yield return new WaitForSeconds(fadeOutDuration);
            
            instance.SetActive(false);
            if (panelTransform != null) panelTransform.gameObject.SetActive(false);
            reactionTransform.localScale = originalScale;
        }

        public IEnumerator ShowReaction(GameObject prefab, float duration)
        {
            if (canvasGroup == null || prefab == null) yield break;

            // Clear previous
            foreach (Transform child in container) Destroy(child.gameObject);

            // Instantiate
            GameObject instance = Instantiate(prefab, container);
            instance.SetActive(true);

            PlaySFX();
            
            // Fade in master
            canvasGroup.DOFade(1f, fadeInDuration);
            
            container.localScale = Vector3.one * 0.5f;
            container.DOScale(Vector3.one, fadeInDuration).SetEase(Ease.OutBack);
            
            yield return new WaitForSeconds(fadeInDuration + duration);
            
            canvasGroup.DOFade(0f, fadeOutDuration);
            yield return new WaitForSeconds(fadeOutDuration);
            
            Destroy(instance);
            }

            private void PlaySFX()
            {
            if (reactionSfx != null && audioSource != null)
            {
                audioSource.PlayOneShot(reactionSfx);
            }
            }

            // Legacy support for sprite-only
        public IEnumerator ShowReaction(Sprite reactionSprite, float duration)
        {
            // We'll create a simple dummy prefab or just handle it if needed
            // But with the new requirement, we'll mostly use the prefab version.
            yield break;
        }
    }
}
