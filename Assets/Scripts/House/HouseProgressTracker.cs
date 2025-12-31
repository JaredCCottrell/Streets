using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Streets.House
{
    /// <summary>
    /// Tracks puzzle completion progress in the house.
    /// Can be used to gate the exit or provide hints.
    /// </summary>
    public class HouseProgressTracker : MonoBehaviour
    {
        public static HouseProgressTracker Instance { get; private set; }

        [Serializable]
        public class TrackedPuzzle
        {
            public string puzzleId;
            public string displayName;
            public bool isComplete;
            public bool isRequired; // Required for house completion
        }

        [Header("Puzzles")]
        [SerializeField] private List<TrackedPuzzle> puzzles = new List<TrackedPuzzle>();

        [Header("Auto-Register")]
        [Tooltip("Automatically find and register all PuzzleControllers and CodeLocks")]
        [SerializeField] private bool autoRegisterPuzzles = true;

        [Header("Events")]
        [SerializeField] private UnityEvent<string> OnPuzzleCompleted;
        [SerializeField] private UnityEvent OnAllRequiredComplete;
        [SerializeField] private UnityEvent OnAllComplete;

        // Events
        public event Action<string> OnPuzzleSolved;
        public event Action OnHouseComplete;

        // Properties
        public int TotalPuzzles => puzzles.Count;
        public int CompletedPuzzles => puzzles.FindAll(p => p.isComplete).Count;
        public int RequiredPuzzles => puzzles.FindAll(p => p.isRequired).Count;
        public int RequiredComplete => puzzles.FindAll(p => p.isRequired && p.isComplete).Count;
        public float CompletionPercent => TotalPuzzles > 0 ? (float)CompletedPuzzles / TotalPuzzles : 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (autoRegisterPuzzles)
            {
                AutoRegisterPuzzles();
            }
        }

        private void AutoRegisterPuzzles()
        {
            // Register PuzzleControllers
            var controllers = FindObjectsOfType<PuzzleController>();
            foreach (var controller in controllers)
            {
                string id = controller.gameObject.name;
                if (!HasPuzzle(id))
                {
                    RegisterPuzzle(id, controller.gameObject.name, true);
                }
                controller.OnSolved += () => MarkPuzzleComplete(id);
            }

            // Register CodeLocks
            var codeLocks = FindObjectsOfType<CodeLock>();
            foreach (var codeLock in codeLocks)
            {
                string id = codeLock.gameObject.name;
                if (!HasPuzzle(id))
                {
                    RegisterPuzzle(id, codeLock.gameObject.name, true);
                }
                codeLock.OnUnlocked += () => MarkPuzzleComplete(id);
            }

            Debug.Log($"[HouseProgressTracker] Auto-registered {puzzles.Count} puzzles");
        }

        /// <summary>
        /// Register a puzzle to track
        /// </summary>
        public void RegisterPuzzle(string puzzleId, string displayName = "", bool required = true)
        {
            if (HasPuzzle(puzzleId)) return;

            puzzles.Add(new TrackedPuzzle
            {
                puzzleId = puzzleId,
                displayName = string.IsNullOrEmpty(displayName) ? puzzleId : displayName,
                isComplete = false,
                isRequired = required
            });
        }

        /// <summary>
        /// Check if a puzzle is registered
        /// </summary>
        public bool HasPuzzle(string puzzleId)
        {
            return puzzles.Exists(p => p.puzzleId == puzzleId);
        }

        /// <summary>
        /// Mark a puzzle as complete
        /// </summary>
        public void MarkPuzzleComplete(string puzzleId)
        {
            var puzzle = puzzles.Find(p => p.puzzleId == puzzleId);
            if (puzzle == null)
            {
                Debug.LogWarning($"[HouseProgressTracker] Unknown puzzle: {puzzleId}");
                return;
            }

            if (puzzle.isComplete) return;

            puzzle.isComplete = true;
            Debug.Log($"[HouseProgressTracker] Puzzle complete: {puzzle.displayName} ({CompletedPuzzles}/{TotalPuzzles})");

            OnPuzzleCompleted?.Invoke(puzzleId);
            OnPuzzleSolved?.Invoke(puzzleId);

            // Check if all required puzzles are complete
            if (AreAllRequiredPuzzlesComplete())
            {
                OnAllRequiredComplete?.Invoke();
            }

            // Check if all puzzles are complete
            if (AreAllPuzzlesComplete())
            {
                OnAllComplete?.Invoke();
                OnHouseComplete?.Invoke();
            }
        }

        /// <summary>
        /// Check if a specific puzzle is complete
        /// </summary>
        public bool IsPuzzleComplete(string puzzleId)
        {
            var puzzle = puzzles.Find(p => p.puzzleId == puzzleId);
            return puzzle?.isComplete ?? false;
        }

        /// <summary>
        /// Check if all required puzzles are complete
        /// </summary>
        public bool AreAllRequiredPuzzlesComplete()
        {
            foreach (var puzzle in puzzles)
            {
                if (puzzle.isRequired && !puzzle.isComplete)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check if all puzzles (including optional) are complete
        /// </summary>
        public bool AreAllPuzzlesComplete()
        {
            foreach (var puzzle in puzzles)
            {
                if (!puzzle.isComplete)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get a list of incomplete required puzzles
        /// </summary>
        public List<TrackedPuzzle> GetIncompletePuzzles(bool requiredOnly = true)
        {
            return puzzles.FindAll(p => !p.isComplete && (!requiredOnly || p.isRequired));
        }

        /// <summary>
        /// Get progress as a formatted string
        /// </summary>
        public string GetProgressString()
        {
            return $"Progress: {CompletedPuzzles}/{TotalPuzzles} ({CompletionPercent:P0})";
        }

        /// <summary>
        /// Reset all puzzle progress
        /// </summary>
        public void ResetProgress()
        {
            foreach (var puzzle in puzzles)
            {
                puzzle.isComplete = false;
            }
        }

        /// <summary>
        /// Debug: Complete all puzzles
        /// </summary>
        public void CompleteAllPuzzles()
        {
            foreach (var puzzle in puzzles)
            {
                if (!puzzle.isComplete)
                {
                    MarkPuzzleComplete(puzzle.puzzleId);
                }
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Log Progress")]
        private void LogProgress()
        {
            Debug.Log(GetProgressString());
            foreach (var puzzle in puzzles)
            {
                string status = puzzle.isComplete ? "✓" : "✗";
                string required = puzzle.isRequired ? "(Required)" : "(Optional)";
                Debug.Log($"  {status} {puzzle.displayName} {required}");
            }
        }
#endif
    }
}
