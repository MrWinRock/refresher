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
        [SerializeField] private ShakerUIFeedback uiFeedback;
        [SerializeField] private BartenderShakeAnimator bartenderAnimator;

        [Header("Flow")]
        [SerializeField] private string minigameId = "ShakerMinigame";
        [SerializeField] private int minNoteCount = 4;
        [SerializeField] private int maxNoteCount = 8;
        [SerializeField] private bool startOnEnable = true;

        [Header("Debug")]
        [SerializeField] private bool debugMode;

        private int _targetNoteCount;
        private PointManager _rhythm;
        private BoostMode _boostMode;
        private bool _feverMode;
        private bool _isRunning;
        private int _resolvedNotes;

        public event System.Action MinigameFinished;

        private void Awake()
        {
            _boostMode = FindFirstObjectByType<BoostMode>();
        }

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

            _targetNoteCount = Random.Range(minNoteCount, maxNoteCount + 1);
            _rhythm = new PointManager(Mathf.Max(1, _targetNoteCount));
            uiFeedback?.SetScore(0f);
            noteSpawner?.Begin(timingJudge ? timingJudge.MaxWindow : 0.2f);
            minigameManager?.RequestMinigame(minigameId);

            if (debugMode && timingJudge)
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
            minigameManager?.CompleteMinigame(minigameId, Mathf.RoundToInt(_rhythm.TotalPoints));

            if (_boostMode == null) _boostMode = FindFirstObjectByType<BoostMode>();
            _boostMode?.AddBoostPoints(_rhythm.CalculateBoostPoint());

            MinigameFinished?.Invoke();
            }

        // ReSharper disable Unity.PerformanceAnalysis
        private void OnArrowPressed(ArrowDirection direction, float hitTime)
        {
            if (!_isRunning || !noteSpawner)
            {
                return;
            }

            if (noteSpawner.TryGetNoteByDirection(direction, out var note))
            {
                ResolveNote(note, hitTime, _feverMode, direction);
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
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

        // ReSharper disable Unity.PerformanceAnalysis
        private void OnNoteExpired(ShakerNoteController note)
        {
            if (!_isRunning || !note)
            {
                return;
            }

            var now = Time.time;
            var result = timingJudge != null
                ? timingJudge.Evaluate(note.ExpireTime, note.TargetHitTime, false)
                : new ShakerJudgementResult(JudgementTier.Bad, 0f, Mathf.Abs(now - note.TargetHitTime));

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
                : new ShakerJudgementResult(JudgementTier.Bad, 0f, Mathf.Abs(hitTime - note.TargetHitTime));

            noteSpawner?.RemoveNote(note);
            ApplyResult(note, result, hitTime, pressedDirection);
        }

        private void ApplyResult(ShakerNoteController note, ShakerJudgementResult result, float hitTime, ArrowDirection pressedDirection)
        {
            _rhythm.AddPoints(result.AwardedPoints);
            uiFeedback?.SetScore(_rhythm.TotalPoints);
            uiFeedback?.ShowJudgement(result.Tier, note.transform.position);

            if (result.Tier != JudgementTier.Bad)
            {
                bartenderAnimator?.PlayForJudgement(result.Tier);
            }

            _resolvedNotes++;
            if (_resolvedNotes >= _targetNoteCount)
            {
                EndMinigame();
            }

            if (debugMode)
            {
                var rangeLabel = "N/A";
                if (timingJudge && timingJudge.TryGetTierRange(result.Tier, out var minRange, out var maxRange))
                {
                    rangeLabel = $"{minRange:F3}-{maxRange:F3}s";
                }

                Debug.Log($"[QTE] Key: {pressedDirection} | Thit: {hitTime:F3} | Tobj: {note.TargetHitTime:F3} | deltaT: {result.DeltaT:F3} | Range: {rangeLabel} | Result: {result.Tier}");
            }
        }

        private void OnFeverModeChanged(bool isEnabled)
        {
            _feverMode = isEnabled;
        }
    }
}


