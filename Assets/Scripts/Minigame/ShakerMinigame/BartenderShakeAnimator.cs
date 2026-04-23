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
        [SerializeField] private float duration = 0.26f;
        [SerializeField] private float positionStrength = 14f;
        [SerializeField] private float rotationStrength = 8f;
        [SerializeField] private int shakeSteps = 4;
        [SerializeField] private float settleDuration = 0.14f;

        private Vector3 _baseLocalPosition;
        private Vector3 _baseLocalEuler;

#if DOTWEEN_ENABLED
        private Sequence _shakeSequence;
#else
        private Coroutine _fallbackRoutine;
#endif

        private void Awake()
        {
            if (bartenderTransform != null)
            {
                _baseLocalPosition = bartenderTransform.localPosition;
                _baseLocalEuler = bartenderTransform.localEulerAngles;
            }
        }

        public void PlayForJudgement(JudgementTier tier)
        {
            if (tier == JudgementTier.Bad || bartenderTransform == null)
            {
                return;
            }

#if DOTWEEN_ENABLED
            PlayDotweenShake();
#else
            if (_fallbackRoutine != null)
            {
                StopCoroutine(_fallbackRoutine);
            }

            _fallbackRoutine = StartCoroutine(FallbackShake());
#endif
        }

#if DOTWEEN_ENABLED
        private void PlayDotweenShake()
        {
            _shakeSequence?.Kill(false);

            var steps = Mathf.Max(3, shakeSteps);
            var stepDuration = Mathf.Max(0.02f, duration / (steps + 1));
            var basePos = _baseLocalPosition;
            var baseRot = _baseLocalEuler;

            _shakeSequence = DOTween.Sequence();

            // Smoothly pull current transform back toward base before new shake pulse.
            _shakeSequence.Append(bartenderTransform.DOLocalMove(basePos, stepDuration * 0.7f).SetEase(Ease.OutSine));
            _shakeSequence.Join(bartenderTransform.DOLocalRotate(baseRot, stepDuration * 0.7f).SetEase(Ease.OutSine));

            for (var i = 0; i < steps; i++)
            {
                var sign = i % 2 == 0 ? 1f : -1f;
                var decay = 1f - (i / (float)steps) * 0.45f;
                var x = positionStrength * 0.01f * sign * decay;
                var y = positionStrength * 0.04f * sign * decay;
                var targetPos = basePos + new Vector3(x, y, 0f);
                var targetRot = baseRot + new Vector3(0f, 0f, rotationStrength * sign * decay);

                _shakeSequence.Append(bartenderTransform.DOLocalMove(targetPos, stepDuration).SetEase(Ease.InOutSine));
                _shakeSequence.Join(bartenderTransform.DOLocalRotate(targetRot, stepDuration).SetEase(Ease.InOutSine));
            }

            _shakeSequence.Append(bartenderTransform.DOLocalMove(basePos, settleDuration).SetEase(Ease.OutCubic));
            _shakeSequence.Join(bartenderTransform.DOLocalRotate(baseRot, settleDuration).SetEase(Ease.OutCubic));
        }
#endif

#if !DOTWEEN_ENABLED
        private IEnumerator FallbackShake()
        {
            var elapsed = 0f;
            var basePos = _baseLocalPosition;
            var baseRot = _baseLocalEuler;
            var steps = Mathf.Max(3, shakeSteps);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var phase = t * steps * Mathf.PI * 2f;
                var decay = 1f - t * 0.45f;
                var x = Mathf.Sin(phase) * positionStrength * 0.01f * decay;
                var y = Mathf.Sin(phase) * positionStrength * 0.04f * decay;
                var z = Mathf.Sin(phase) * rotationStrength * decay;

                bartenderTransform.localPosition = basePos + new Vector3(x, y, 0f);
                bartenderTransform.localEulerAngles = baseRot + new Vector3(0f, 0f, z);
                yield return null;
            }

            var settleElapsed = 0f;
            var settlePos = bartenderTransform.localPosition;
            var settleRot = bartenderTransform.localEulerAngles;

            while (settleElapsed < settleDuration)
            {
                settleElapsed += Time.deltaTime;
                var t = Mathf.Clamp01(settleElapsed / settleDuration);
                t = 1f - Mathf.Pow(1f - t, 3f);
                bartenderTransform.localPosition = Vector3.Lerp(settlePos, basePos, t);
                bartenderTransform.localEulerAngles = Vector3.Lerp(settleRot, baseRot, t);
                yield return null;
            }

            bartenderTransform.localPosition = basePos;
            bartenderTransform.localEulerAngles = baseRot;
        }
#endif

        private void OnDisable()
        {
            if (bartenderTransform == null)
            {
                return;
            }

#if DOTWEEN_ENABLED
            _shakeSequence?.Kill(false);
#else
            if (_fallbackRoutine != null)
            {
                StopCoroutine(_fallbackRoutine);
                _fallbackRoutine = null;
            }
#endif

            bartenderTransform.localPosition = _baseLocalPosition;
            bartenderTransform.localEulerAngles = _baseLocalEuler;
        }
    }
}


