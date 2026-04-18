using System;
using System.Collections.Generic;
using UnityEngine;

namespace Minigame.ShakerMinigame
{
    public class ShakerNoteSpawner : MonoBehaviour
    {
        [Header("Spawn")]
        [SerializeField] private ShakerNoteController notePrefab;
        [SerializeField] private RectTransform spawnArea;
        [SerializeField] private float spawnInterval = 0.6f;
        [SerializeField] private int maxActiveNotes = 4;
        [SerializeField] private float noteLeadTime = 1.0f;

        private readonly List<ShakerNoteController> _activeNotes = new();
        private readonly HashSet<ArrowDirection> _activeDirections = new();

        private ArrowDirection? _previousDirection;
        private float _nextSpawnAt;
        private float _badWindow;
        private bool _isRunning;

        public IReadOnlyList<ShakerNoteController> ActiveNotes => _activeNotes;

        public event Action<ShakerNoteController> NoteSpawned;
        public event Action<ShakerNoteController> NoteExpired;
        public event Action<ShakerNoteController> NoteRemoved;

        public void Begin(float judgeBadWindow)
        {
            _badWindow = Mathf.Max(0.05f, judgeBadWindow);
            _nextSpawnAt = Time.time;
            _previousDirection = null;
            _isRunning = true;
        }

        public void Stop()
        {
            _isRunning = false;

            for (var i = _activeNotes.Count - 1; i >= 0; i--)
            {
                RemoveNote(_activeNotes[i]);
            }

            _activeDirections.Clear();
            _activeNotes.Clear();
        }

        private void Update()
        {
            if (!_isRunning || notePrefab == null || spawnArea == null)
            {
                return;
            }

            if (Time.time >= _nextSpawnAt && _activeNotes.Count < maxActiveNotes)
            {
                Spawn();
                _nextSpawnAt = Time.time + Mathf.Max(0.05f, spawnInterval);
            }
        }

        private void Spawn()
        {
            if (!TryGetNextDirection(out var direction))
            {
                return;
            }

            var note = Instantiate(notePrefab, spawnArea);
            var anchoredPosition = GetRandomAnchoredPosition(spawnArea);
            var noteRect = note.GetComponent<RectTransform>();
            if (noteRect != null)
            {
                noteRect.anchoredPosition = anchoredPosition;
            }

            var now = Time.time;
            var target = now + noteLeadTime;
            var expire = target + _badWindow;

            note.Initialize(direction, now, target, expire, OnNoteExpiredInternally);

            _activeNotes.Add(note);
            _activeDirections.Add(direction);
            _previousDirection = direction;
            NoteSpawned?.Invoke(note);
        }

        private void OnNoteExpiredInternally(ShakerNoteController note, float _)
        {
            NoteExpired?.Invoke(note);
            RemoveNote(note);
        }

        public bool TryGetNoteByDirection(ArrowDirection direction, out ShakerNoteController note)
        {
            note = null;

            for (var i = 0; i < _activeNotes.Count; i++)
            {
                if (_activeNotes[i] != null && _activeNotes[i].Matches(direction))
                {
                    note = _activeNotes[i];
                    return true;
                }
            }

            return false;
        }

        public bool TryGetEarliestNote(out ShakerNoteController note)
        {
            note = null;
            var bestTarget = float.MaxValue;

            for (var i = 0; i < _activeNotes.Count; i++)
            {
                var active = _activeNotes[i];
                if (active == null)
                {
                    continue;
                }

                if (active.TargetHitTime < bestTarget)
                {
                    bestTarget = active.TargetHitTime;
                    note = active;
                }
            }

            return note != null;
        }

        public void RemoveNote(ShakerNoteController note)
        {
            if (note == null)
            {
                return;
            }

            _activeNotes.Remove(note);
            _activeDirections.Remove(note.Direction);
            NoteRemoved?.Invoke(note);

            if (note != null)
            {
                Destroy(note.gameObject);
            }
        }

        private bool TryGetNextDirection(out ArrowDirection selected)
        {
            selected = ArrowDirection.Up;
            var candidates = new List<ArrowDirection>(4)
            {
                ArrowDirection.Up,
                ArrowDirection.Down,
                ArrowDirection.Left,
                ArrowDirection.Right
            };

            // Enforce no active-direction conflict and no direct consecutive repeat.
            candidates.RemoveAll(direction => _activeDirections.Contains(direction) || (_previousDirection.HasValue && _previousDirection.Value == direction));

            if (candidates.Count == 0)
            {
                return false;
            }

            selected = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            return true;
        }

        private static Vector2 GetRandomAnchoredPosition(RectTransform area)
        {
            var rect = area.rect;
            var x = UnityEngine.Random.Range(rect.xMin, rect.xMax);
            var y = UnityEngine.Random.Range(rect.yMin, rect.yMax);
            return new Vector2(x, y);
        }
    }
}


