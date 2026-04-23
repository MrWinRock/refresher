using System.Collections;
using TMPro;
using UnityEngine;
#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace Minigame.ShakerMinigame
{
    public class ShakerUIFeedback : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text floatingTextPrefab;
        [SerializeField] private RectTransform textCanvasRoot;
        [SerializeField] private TMP_Text scoreLabel;

        [Header("Feedback Animation")]
        [SerializeField] private float floatDistance = 90f;
        [SerializeField] private float floatDuration = 0.45f;

        [Header("SFX")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip perfectSfx;
        [SerializeField] private AudioClip greatSfx;
        [SerializeField] private AudioClip goodSfx;
        [SerializeField] private AudioClip badSfx;

        [Header("VFX")]
        [SerializeField] private ParticleSystem perfectVfx;
        [SerializeField] private ParticleSystem greatVfx;
        [SerializeField] private ParticleSystem goodVfx;
        [SerializeField] private ParticleSystem badVfx;

        public void SetScore(float score)
        {
            if (scoreLabel)
            {
                scoreLabel.text = $"Score: {score:F1}";
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void ShowJudgement(JudgementTier tier, Vector3 worldPosition)
        {
            if (!floatingTextPrefab || !textCanvasRoot)
            {
                return;
            }

            var text = Instantiate(floatingTextPrefab, textCanvasRoot);
            var rect = text.rectTransform;

            var canvas = textCanvasRoot.GetComponentInParent<Canvas>();
            bool isOverlay = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay;

            Camera cam = isOverlay ? null : (canvas != null ? canvas.worldCamera : Camera.main);
            if (!isOverlay && cam == null)
            {
                Destroy(text.gameObject);
                return;
            }

            var screenPoint = isOverlay
                ? (Vector2)worldPosition
                : RectTransformUtility.WorldToScreenPoint(cam, worldPosition);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(textCanvasRoot, screenPoint, cam, out var localPoint);
            rect.anchoredPosition = localPoint;

            text.text = tier.ToString();
            text.color = GetColor(tier);

            PlayAudio(tier);
            PlayVfx(tier, worldPosition);

#if DOTWEEN_ENABLED
            var canvasGroup = text.gameObject.AddComponent<CanvasGroup>();
            var targetY = rect.anchoredPosition.y + floatDistance;

            var sequence = DOTween.Sequence();
            sequence.Join(rect.DOAnchorPosY(targetY, floatDuration).SetEase(Ease.OutCubic));
            sequence.Join(canvasGroup.DOFade(0f, floatDuration).SetEase(Ease.Linear));
            sequence.OnComplete(() => Destroy(text.gameObject));
#else
            StartCoroutine(FallbackFloatAndFade(text));
#endif
        }

        private static Color GetColor(JudgementTier tier)
        {
            return tier switch
            {
                JudgementTier.Perfect => new Color(1f, 0.88f, 0.2f),
                JudgementTier.Great => new Color(0.3f, 0.95f, 1f),
                JudgementTier.Good => new Color(0.55f, 1f, 0.55f),
                _ => new Color(1f, 0.45f, 0.45f)
            };
        }

        private void PlayAudio(JudgementTier tier)
        {
            if (audioSource == null)
            {
                return;
            }

            var clip = tier switch
            {
                JudgementTier.Perfect => perfectSfx,
                JudgementTier.Great => greatSfx,
                JudgementTier.Good => goodSfx,
                _ => badSfx
            };

            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        private void PlayVfx(JudgementTier tier, Vector3 worldPosition)
        {
            var vfx = tier switch
            {
                JudgementTier.Perfect => perfectVfx,
                JudgementTier.Great => greatVfx,
                JudgementTier.Good => goodVfx,
                _ => badVfx
            };

            if (vfx == null)
            {
                return;
            }

            vfx.transform.position = worldPosition;
            vfx.Play();
        }

        private IEnumerator FallbackFloatAndFade(TMP_Text text)
        {
            var elapsed = 0f;
            var startPos = text.rectTransform.anchoredPosition;
            var startColor = text.color;

            while (elapsed < floatDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / floatDuration);

                text.rectTransform.anchoredPosition = startPos + Vector2.up * (floatDistance * t);
                text.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);

                yield return null;
            }

            Destroy(text.gameObject);
        }
    }
}


