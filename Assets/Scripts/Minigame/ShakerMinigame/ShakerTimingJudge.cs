using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Minigame.ShakerMinigame
{
    public class ShakerTimingJudge : MonoBehaviour
    {
        [Header("Timing Windows (seconds)")]
        [SerializeField] private ShakerJudgementWindow[] windows =
        {
            new() { threshold = 0.06f, tier = JudgementTier.Perfect, points = 1f },
            new() { threshold = 0.11f, tier = JudgementTier.Great, points = 0.7f },
            new() { threshold = 0.17f, tier = JudgementTier.Good, points = 0.5f },
            new() { threshold = 0.26f, tier = JudgementTier.Bad, points = 0f }
        };

        [Header("Range Adjust")]
        [Min(0.1f)]
        [SerializeField] private float timingWindowScale = 1.1f;
        [Tooltip("Adds extra seconds only to Perfect threshold (applies both early and late because deltaT is absolute).")]
        [Min(0f)]
        [SerializeField] private float perfectBonusSeconds = 0.02f;

        [Header("Fever")]
        [Min(0f)]
        [SerializeField] private float feverPerfectPoints = 1f;

        private ShakerJudgementWindow[] _orderedWindows;

        public float MaxWindow => _orderedWindows == null || _orderedWindows.Length == 0 ? 0.2f : GetEffectiveThreshold(_orderedWindows[^1]);

        private void Awake()
        {
            RebuildWindows();
        }

        private void OnValidate()
        {
            RebuildWindows();
        }

        private void RebuildWindows()
        {
            timingWindowScale = Mathf.Max(0.1f, timingWindowScale);

            _orderedWindows = windows
                .Where(window => window.threshold >= 0f)
                .OrderBy(window => window.threshold)
                .ToArray();

            if (_orderedWindows.Length == 0)
            {
                _orderedWindows = new[] { new ShakerJudgementWindow { threshold = 0.2f, tier = JudgementTier.Bad, points = 0f } };
            }
        }

        public ShakerJudgementResult Evaluate(float hitTime, float targetTime, bool feverMode)
        {
            var deltaT = Mathf.Abs(hitTime - targetTime);

            if (feverMode)
            {
                return new ShakerJudgementResult(JudgementTier.Perfect, feverPerfectPoints, deltaT);
            }

            foreach (var window in _orderedWindows)
            {
                var effectiveThreshold = GetEffectiveThreshold(window);
                if (deltaT <= effectiveThreshold)
                {
                    return new ShakerJudgementResult(window.tier, window.points, deltaT);
                }
            }

            return new ShakerJudgementResult(JudgementTier.Bad, 0f, deltaT);
        }

        public bool TryGetTierRange(JudgementTier tier, out float minInclusive, out float maxInclusive)
        {
            minInclusive = 0f;
            maxInclusive = 0f;
            var previousThreshold = 0f;

            for (var i = 0; i < _orderedWindows.Length; i++)
            {
                var window = _orderedWindows[i];
                var currentThreshold = GetEffectiveThreshold(window);

                if (window.tier == tier)
                {
                    minInclusive = i == 0 ? 0f : previousThreshold;
                    maxInclusive = currentThreshold;
                    return true;
                }

                previousThreshold = currentThreshold;
            }

            return false;
        }

        private float GetEffectiveThreshold(ShakerJudgementWindow window)
        {
            var threshold = window.threshold * timingWindowScale;

            if (window.tier == JudgementTier.Perfect)
            {
                threshold += perfectBonusSeconds;
            }

            return threshold;
        }

        public string BuildRangeSummary()
        {
            var builder = new StringBuilder();

            foreach (JudgementTier tier in Enum.GetValues(typeof(JudgementTier)))
            {
                if (!TryGetTierRange(tier, out var min, out var max))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(" | ");
                }

                builder.Append(tier);
                builder.Append(": ");
                builder.Append(min.ToString("F3"));
                builder.Append("-");
                builder.Append(max.ToString("F3"));
                builder.Append("s");
            }

            return builder.ToString();
        }
    }
}


