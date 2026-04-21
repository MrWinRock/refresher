using System;
using UnityEngine;

namespace Minigame.ShakerMinigame
{
    public class ShakerScoreManager : MonoBehaviour
    {
        public event Action<int> ScoreChanged;

        public int CurrentScore { get; private set; }

        public void ResetScore()
        {
            CurrentScore = 0;
            ScoreChanged?.Invoke(CurrentScore);
        }

        public void AddScore(int amount)
        {
            CurrentScore += Mathf.Max(0, amount);
            ScoreChanged?.Invoke(CurrentScore);
        }
    }
}


