using System;
using System.Linq;
using UnityEngine;

namespace Minigame.ShakerMinigame
{
    public class ShakerTimingJudge : MonoBehaviour
    {
        [Header("Timing Windows (seconds)")]
        [SerializeField] private ShakerJudgementWindow[] windows =
        {
            new() { threshold = 0.045f, tier = JudgementTier.Perfect, score = 300 },
            new() { threshold = 0.09f, tier = JudgementTier.Great, score = 200 },
            new() { threshold = 0.14f, tier = JudgementTier.Good, score = 100 },
            new() { threshold = 0.22f, tier = JudgementTier.Bad, score = 25 }
        };

        [Header("Fever")]
        [SerializeField] private int feverPerfectScore = 300;

        private ShakerJudgementWindow[] orderedWindows;

        public float MaxWindow => orderedWindows.Length == 0 ? 0.2f : orderedWindows[^1].threshold;

        private void Awake()
        {
            orderedWindows = windows
                .OrderBy(window => window.threshold)
                .ToArray();

            if (orderedWindows.Length == 0)
            {
                orderedWindows = new[] { new ShakerJudgementWindow { threshold = 0.2f, tier = JudgementTier.Bad, score = 0 } };
            }
        }

        public ShakerJudgementResult Evaluate(float hitTime, float targetTime, bool feverMode)
        {
            var deltaT = Mathf.Abs(hitTime - targetTime);

            if (feverMode)
            {
                return new ShakerJudgementResult(JudgementTier.Perfect, feverPerfectScore, deltaT);
            }

            foreach (var window in orderedWindows)
            {
                if (deltaT <= window.threshold)
                {
                    return new ShakerJudgementResult(window.tier, window.score, deltaT);
                }
            }

            return new ShakerJudgementResult(JudgementTier.Bad, 0, deltaT);
        }
    }
}


