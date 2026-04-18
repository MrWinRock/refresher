using System.Collections;
using UnityEngine;
#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace Minigame.ShakerMinigame
{
    public class BartenderShakeAnimator : MonoBehaviour
    {
        [SerializeField] private Transform bartenderTransform;
        [SerializeField] private float duration = 0.18f;
        [SerializeField] private float positionStrength = 14f;
        [SerializeField] private float rotationStrength = 8f;
        [SerializeField] private int vibrato = 18;

        public void PlayForJudgement(JudgementTier tier)
        {
            if (tier == JudgementTier.Bad || bartenderTransform == null)
            {
                return;
            }

#if DOTWEEN_ENABLED
            bartenderTransform.DOShakePosition(duration, positionStrength, vibrato, 90f, false, true);
            bartenderTransform.DOShakeRotation(duration, rotationStrength, vibrato, 90f, true);
#else
            StartCoroutine(FallbackShake());
#endif
        }

#if !DOTWEEN_ENABLED
        private IEnumerator FallbackShake()
        {
            var elapsed = 0f;
            var originalPos = bartenderTransform.localPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var noise = Random.insideUnitCircle * (positionStrength * 0.01f);
                bartenderTransform.localPosition = originalPos + new Vector3(noise.x, noise.y, 0f);
                yield return null;
            }

            bartenderTransform.localPosition = originalPos;
        }
#endif
    }
}


