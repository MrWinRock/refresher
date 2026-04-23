using System;
using UnityEngine;

namespace Minigame
{
    public class MinigameManager : MonoBehaviour
    {
        [SerializeField] private bool feverMode;

        public event Action<bool> FeverModeChanged;
        public event Action<string> MinigameRequested;
        public event Action<string, int> MinigameCompleted;

        public bool IsFeverMode => feverMode;

        public void RequestMinigame(string minigameId)
        {
            MinigameRequested?.Invoke(minigameId);
        }

        public void CompleteMinigame(string minigameId, int score)
        {
            MinigameCompleted?.Invoke(minigameId, score);
        }

        public void SetFeverMode(bool isEnabled)
        {
            if (feverMode == isEnabled)
            {
                return;
            }

            feverMode = isEnabled;
            FeverModeChanged?.Invoke(feverMode);
        }
    }
}


