using UnityEngine;

namespace Minigame.ShakerMinigame
{
    public class ShakerMinigameController : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private MinigameManager minigameManager;
        [SerializeField] private ShakerNoteSpawner noteSpawner;
        [SerializeField] private ShakerInputHandler inputHandler;
        [SerializeField] private ShakerTimingJudge timingJudge;
        [SerializeField] private ShakerScoreManager scoreManager;
        [SerializeField] private ShakerUIFeedback uiFeedback;
        [SerializeField] private BartenderShakeAnimator bartenderAnimator;

        [Header("Flow")]
        [SerializeField] private string minigameId = "ShakerMinigame";
        [SerializeField] private int targetNoteCount = 24;
        [SerializeField] private bool startOnEnable = true;

        [Header("Debug")]
        [SerializeField] private bool debugMode;

        private bool _feverMode;
        private bool _isRunning;
        private int _resolvedNotes;

        private void OnEnable()
        {
            if (inputHandler != null)
            {
                inputHandler.ArrowPressed += OnArrowPressed;
                inputHandler.AnyKeyPressed += OnAnyKeyPressed;
            }

            if (noteSpawner != null)
            {
                noteSpawner.NoteExpired += OnNoteExpired;
            }

            if (scoreManager != null)
            {
                scoreManager.ScoreChanged += OnScoreChanged;
            }

            if (minigameManager != null)
            {
                minigameManager.FeverModeChanged += OnFeverModeChanged;
                _feverMode = minigameManager.IsFeverMode;
            }

            if (startOnEnable)
            {
                BeginMinigame();
            }
        }

        private void OnDisable()
        {
            if (inputHandler != null)
            {
                inputHandler.ArrowPressed -= OnArrowPressed;
                inputHandler.AnyKeyPressed -= OnAnyKeyPressed;
            }

            if (noteSpawner != null)
            {
                noteSpawner.NoteExpired -= OnNoteExpired;
            }

            if (scoreManager != null)
            {
                scoreManager.ScoreChanged -= OnScoreChanged;
            }

            if (minigameManager != null)
            {
                minigameManager.FeverModeChanged -= OnFeverModeChanged;
            }
        }

        public void BeginMinigame()
        {
            if (_isRunning)
            {
                return;
            }

            _resolvedNotes = 0;
            _isRunning = true;

            scoreManager?.ResetScore();
            noteSpawner?.Begin(timingJudge != null ? timingJudge.MaxWindow : 0.2f);
            minigameManager?.RequestMinigame(minigameId);

            if (debugMode && timingJudge != null)
            {
                Debug.Log($"[QTE] Timing Ranges -> {timingJudge.BuildRangeSummary()}");
            }
        }

        public void EndMinigame()
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;
            noteSpawner?.Stop();
            minigameManager?.CompleteMinigame(minigameId, scoreManager != null ? scoreManager.CurrentScore : 0);
        }

        private void OnArrowPressed(ArrowDirection direction, float hitTime)
        {
            if (!_isRunning || noteSpawner == null)
            {
                return;
            }

            if (noteSpawner.TryGetNoteByDirection(direction, out var note))
            {
                ResolveNote(note, hitTime, _feverMode, direction);
            }
        }

        private void OnAnyKeyPressed(float hitTime)
        {
            if (!_isRunning || !_feverMode || noteSpawner == null)
            {
                return;
            }

            if (noteSpawner.TryGetEarliestNote(out var note))
            {
                ResolveNote(note, hitTime, true, note.Direction);
            }
        }

        private void OnNoteExpired(ShakerNoteController note)
        {
            if (!_isRunning || note == null)
            {
                return;
            }

            // Expired notes are still scored as part of no-fail flow.
            var now = Time.time;
            var result = timingJudge != null
                ? timingJudge.Evaluate(note.ExpireTime, note.TargetHitTime, false)
                : new ShakerJudgementResult(JudgementTier.Bad, 0, Mathf.Abs(now - note.TargetHitTime));

            ApplyResult(note, result, now, note.Direction);
        }

        private void ResolveNote(ShakerNoteController note, float hitTime, bool forceFever, ArrowDirection pressedDirection)
        {
            if (note == null || !note.TryResolve())
            {
                return;
            }

            var result = timingJudge != null
                ? timingJudge.Evaluate(hitTime, note.TargetHitTime, forceFever)
                : new ShakerJudgementResult(JudgementTier.Bad, 0, Mathf.Abs(hitTime - note.TargetHitTime));

            noteSpawner?.RemoveNote(note);
            ApplyResult(note, result, hitTime, pressedDirection);
        }

        private void ApplyResult(ShakerNoteController note, ShakerJudgementResult result, float hitTime, ArrowDirection pressedDirection)
        {
            scoreManager?.AddScore(result.AwardedScore);
            uiFeedback?.ShowJudgement(result.Tier, note.transform.position);

            if (result.Tier != JudgementTier.Bad)
            {
                bartenderAnimator?.PlayForJudgement(result.Tier);
            }

            _resolvedNotes++;
            if (_resolvedNotes >= targetNoteCount)
            {
                EndMinigame();
            }

            if (debugMode)
            {
                var score = scoreManager != null ? scoreManager.CurrentScore : 0;
                var rangeLabel = "N/A";
                if (timingJudge != null && timingJudge.TryGetTierRange(result.Tier, out var minRange, out var maxRange))
                {
                    rangeLabel = $"{minRange:F3}-{maxRange:F3}s";
                }

                Debug.Log($"[QTE] Key: {pressedDirection} | Thit: {hitTime:F3} | Tobj: {note.TargetHitTime:F3} | deltaT: {result.DeltaT:F3} | Range: {rangeLabel} | Result: {result.Tier} | Score: {score}");
            }
        }

        private void OnScoreChanged(int score)
        {
            uiFeedback?.SetScore(score);
        }

        private void OnFeverModeChanged(bool isEnabled)
        {
            _feverMode = isEnabled;
        }
    }
}


