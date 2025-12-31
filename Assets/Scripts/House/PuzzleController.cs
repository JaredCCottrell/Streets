using System;
using UnityEngine;
using UnityEngine.Events;

namespace Streets.House
{
    /// <summary>
    /// Controls a multi-switch puzzle.
    /// Triggers events when all switches are in the correct state.
    /// </summary>
    public class PuzzleController : MonoBehaviour
    {
        [Header("Puzzle Configuration")]
        [SerializeField] private PuzzleSwitch[] switches;
        [SerializeField] private bool[] requiredStates;

        [Header("Puzzle Settings")]
        [Tooltip("Reset all switches when puzzle is solved")]
        [SerializeField] private bool resetOnSolve = false;
        [Tooltip("Can the puzzle be solved multiple times")]
        [SerializeField] private bool allowReSolve = false;

        [Header("Connected Objects")]
        [SerializeField] private Door doorToUnlock;
        [SerializeField] private GameObject[] objectsToActivate;
        [SerializeField] private GameObject[] objectsToDeactivate;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip solvedSound;
        [SerializeField] private AudioClip resetSound;

        [Header("Events")]
        [SerializeField] private UnityEvent OnPuzzleSolved;
        [SerializeField] private UnityEvent OnPuzzleReset;

        // State
        private bool isSolved = false;

        // Events
        public event Action OnSolved;
        public event Action OnReset;

        // Properties
        public bool IsSolved => isSolved;
        public int SwitchCount => switches?.Length ?? 0;
        public int CorrectSwitchCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < switches.Length; i++)
                {
                    if (IsSwitchCorrect(i)) count++;
                }
                return count;
            }
        }

        private void Start()
        {
            // Validate configuration
            if (switches == null || switches.Length == 0)
            {
                Debug.LogWarning($"[PuzzleController] No switches configured for {gameObject.name}");
                return;
            }

            // Ensure requiredStates array matches switches
            if (requiredStates == null || requiredStates.Length != switches.Length)
            {
                Debug.LogWarning($"[PuzzleController] Required states array doesn't match switches. Defaulting all to ON.");
                requiredStates = new bool[switches.Length];
                for (int i = 0; i < requiredStates.Length; i++)
                {
                    requiredStates[i] = true;
                }
            }

            // Subscribe to switch events
            foreach (var sw in switches)
            {
                if (sw != null)
                {
                    sw.OnSwitchStateChanged += OnSwitchChanged;
                }
            }

            // Check initial state
            CheckPuzzleState();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (switches != null)
            {
                foreach (var sw in switches)
                {
                    if (sw != null)
                    {
                        sw.OnSwitchStateChanged -= OnSwitchChanged;
                    }
                }
            }
        }

        private void OnSwitchChanged(PuzzleSwitch sw, bool state)
        {
            CheckPuzzleState();
        }

        /// <summary>
        /// Check if all switches are in the correct state
        /// </summary>
        public void CheckPuzzleState()
        {
            if (isSolved && !allowReSolve) return;

            bool allCorrect = true;

            for (int i = 0; i < switches.Length; i++)
            {
                if (!IsSwitchCorrect(i))
                {
                    allCorrect = false;
                    break;
                }
            }

            if (allCorrect && !isSolved)
            {
                SolvePuzzle();
            }
            else if (!allCorrect && isSolved && allowReSolve)
            {
                ResetPuzzle();
            }
        }

        private bool IsSwitchCorrect(int index)
        {
            if (index < 0 || index >= switches.Length) return false;
            if (switches[index] == null) return false;

            return switches[index].IsActive == requiredStates[index];
        }

        private void SolvePuzzle()
        {
            isSolved = true;

            Debug.Log($"[PuzzleController] Puzzle solved: {gameObject.name}");

            // Play sound
            if (solvedSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(solvedSound);
            }

            // Unlock door
            if (doorToUnlock != null)
            {
                doorToUnlock.Unlock();
            }

            // Activate objects
            foreach (var obj in objectsToActivate)
            {
                if (obj != null) obj.SetActive(true);
            }

            // Deactivate objects
            foreach (var obj in objectsToDeactivate)
            {
                if (obj != null) obj.SetActive(false);
            }

            // Reset switches if configured
            if (resetOnSolve)
            {
                foreach (var sw in switches)
                {
                    if (sw != null)
                    {
                        sw.ForceState(false);
                    }
                }
            }

            OnPuzzleSolved?.Invoke();
            OnSolved?.Invoke();
        }

        private void ResetPuzzle()
        {
            isSolved = false;

            Debug.Log($"[PuzzleController] Puzzle reset: {gameObject.name}");

            if (resetSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(resetSound);
            }

            OnPuzzleReset?.Invoke();
            OnReset?.Invoke();
        }

        /// <summary>
        /// Force solve the puzzle (for debugging)
        /// </summary>
        public void ForceSolve()
        {
            // Set all switches to required states
            for (int i = 0; i < switches.Length; i++)
            {
                if (switches[i] != null)
                {
                    switches[i].SetState(requiredStates[i]);
                }
            }
        }

        /// <summary>
        /// Reset all switches to off
        /// </summary>
        public void ResetAllSwitches()
        {
            foreach (var sw in switches)
            {
                if (sw != null)
                {
                    sw.SetState(false);
                }
            }

            isSolved = false;
        }

        /// <summary>
        /// Get the hint for which switches need to change
        /// </summary>
        public string GetHint()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Switch states needed:");

            for (int i = 0; i < switches.Length; i++)
            {
                string switchName = switches[i] != null ? switches[i].gameObject.name : $"Switch {i}";
                string needed = requiredStates[i] ? "ON" : "OFF";
                string current = switches[i] != null ? (switches[i].IsActive ? "ON" : "OFF") : "?";
                string status = IsSwitchCorrect(i) ? "✓" : "✗";

                sb.AppendLine($"  {switchName}: {needed} (currently {current}) {status}");
            }

            return sb.ToString();
        }

        private void OnDrawGizmosSelected()
        {
            if (switches == null) return;

            // Draw lines to all switches
            Gizmos.color = isSolved ? Color.green : Color.yellow;

            foreach (var sw in switches)
            {
                if (sw != null)
                {
                    Gizmos.DrawLine(transform.position, sw.transform.position);
                }
            }
        }
    }
}
