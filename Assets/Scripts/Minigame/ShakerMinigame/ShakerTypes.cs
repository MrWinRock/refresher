using System;
using UnityEngine;

namespace Minigame.ShakerMinigame
{
    public enum ArrowDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum JudgementTier
    {
        Perfect,
        Great,
        Good,
        Bad
    }

    [Serializable]
    public struct ShakerJudgementWindow
    {
        [Min(0f)] public float threshold;
        public JudgementTier tier;
        public int score;
    }

    public readonly struct ShakerJudgementResult
    {
        public ShakerJudgementResult(JudgementTier tier, int awardedScore, float deltaT)
        {
            Tier = tier;
            AwardedScore = awardedScore;
            DeltaT = deltaT;
        }

        public JudgementTier Tier { get; }
        public int AwardedScore { get; }
        public float DeltaT { get; }
    }
}


