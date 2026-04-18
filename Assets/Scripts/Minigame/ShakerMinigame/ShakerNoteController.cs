using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private float visualHitTimeOffsetSeconds = 0f;
        [SerializeField] private float latePhaseEndScale = 0.85f;

        private Action<ShakerNoteController, float> _expiredCallback;
        private bool _isResolved;
        private float _targetScaleAtHit;
        private float _visualHitTime;

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

            if (arrowLabel != null)
            {
                arrowLabel.text = DirectionToGlyph(direction);
            }

            if (approachCircle != null)
            {
                approachCircle.rectTransform.localScale = Vector3.one * initialApproachScale;
            }
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
                float scale;

                if (now > _visualHitTime)
                {
                    // Keep shrinking during late window so visual timing stays readable.
                    var lateProgress = Mathf.InverseLerp(_visualHitTime, ExpireTime, now);
                    var endScale = Mathf.Min(_targetScaleAtHit, latePhaseEndScale);
                    scale = Mathf.Lerp(_targetScaleAtHit, endScale, lateProgress);
                }
                else
                {
                    var progress = Mathf.InverseLerp(SpawnTime, _visualHitTime, now);
                    scale = Mathf.Lerp(initialApproachScale, _targetScaleAtHit, progress);
                }

                approachCircle.rectTransform.localScale = new Vector3(scale, scale, 1f);
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




