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
        [Min(0f)] public float points;
    }

    public readonly struct ShakerJudgementResult
    {
        public ShakerJudgementResult(JudgementTier tier, float awardedPoints, float deltaT)
        {
            Tier = tier;
            AwardedPoints = awardedPoints;
            DeltaT = deltaT;
        }

        public JudgementTier Tier { get; }
        public float AwardedPoints { get; }
        public float DeltaT { get; }
    }
}


