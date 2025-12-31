using System;
using UnityEngine;
using UnityEngine.Events;

namespace Streets.House
{
    /// <summary>
    /// A switch/lever/button that can be toggled or pressed.
    /// Can trigger events or be part of a multi-switch puzzle.
    /// </summary>
    public class PuzzleSwitch : Interactable
    {
        [Header("Switch Settings")]
        [SerializeField] private bool isToggle = true;
        [SerializeField] private bool initialState = false;
        [SerializeField] private bool canInteractWhenOn = true;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject onIndicator;
        [SerializeField] private GameObject offIndicator;
        [SerializeField] private Transform switchTransform;
        [SerializeField] private Vector3 onRotation = new Vector3(0, 0, 45);
        [SerializeField] private Vector3 offRotation = new Vector3(0, 0, -45);
        [SerializeField] private float animationDuration = 0.2f;

        [Header("Switch Audio")]
        [SerializeField] private AudioClip switchOnSound;
        [SerializeField] private AudioClip switchOffSound;

        [Header("Events")]
        [SerializeField] private UnityEvent OnActivated;
        [SerializeField] private UnityEvent OnDeactivated;
        [SerializeField] private UnityEvent<bool> OnStateChanged;

        // State
        private bool isActive;
        private bool isAnimating;

        // Events
        public event Action<PuzzleSwitch, bool> OnSwitchStateChanged;

        // Properties
        public bool IsActive => isActive;
        public bool IsAnimating => isAnimating;

        protected override void Start()
        {
            base.Start();

            // Set initial state
            isActive = initialState;
            UpdateVisuals(false);
            UpdatePromptText();
        }

        protected override void OnInteract()
        {
            if (isToggle)
            {
                Toggle();
            }
            else
            {
                // Momentary switch - activate then deactivate
                if (!isActive)
                {
                    SetState(true);
                    Invoke(nameof(Deactivate), 0.5f);
                }
            }
        }

        protected override bool CanInteract()
        {
            if (isAnimating) return false;
            if (isActive && !canInteractWhenOn && isToggle) return false;

            return base.CanInteract();
        }

        private void UpdatePromptText()
        {
            if (isToggle)
            {
                promptText = isActive ? "Press E to turn off" : "Press E to turn on";
            }
            else
            {
                promptText = "Press E to activate";
            }
        }

        #region Switch Actions

        public void Toggle()
        {
            SetState(!isActive);
        }

        public void Activate()
        {
            SetState(true);
        }

        public void Deactivate()
        {
            SetState(false);
        }

        public void SetState(bool active)
        {
            if (isActive == active) return;

            isActive = active;
            UpdateVisuals(true);
            UpdatePromptText();

            // Play sound
            PlaySound(active ? switchOnSound : switchOffSound);

            // Fire events
            if (active)
            {
                OnActivated?.Invoke();
            }
            else
            {
                OnDeactivated?.Invoke();
            }

            OnStateChanged?.Invoke(active);
            OnSwitchStateChanged?.Invoke(this, active);

            Debug.Log($"[PuzzleSwitch] {gameObject.name} is now {(active ? "ON" : "OFF")}");
        }

        /// <summary>
        /// Force set state without events (for initialization)
        /// </summary>
        public void ForceState(bool active)
        {
            isActive = active;
            UpdateVisuals(false);
            UpdatePromptText();
        }

        #endregion

        private void UpdateVisuals(bool animate)
        {
            // Update indicators
            if (onIndicator != null)
            {
                onIndicator.SetActive(isActive);
            }
            if (offIndicator != null)
            {
                offIndicator.SetActive(!isActive);
            }

            // Animate switch transform
            if (switchTransform != null)
            {
                Vector3 targetRotation = isActive ? onRotation : offRotation;

                if (animate && animationDuration > 0)
                {
                    StartCoroutine(AnimateSwitch(targetRotation));
                }
                else
                {
                    switchTransform.localEulerAngles = targetRotation;
                }
            }
        }

        private System.Collections.IEnumerator AnimateSwitch(Vector3 targetRotation)
        {
            isAnimating = true;

            Vector3 startRotation = switchTransform.localEulerAngles;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                t = t * t * (3f - 2f * t); // Smoothstep

                switchTransform.localEulerAngles = Vector3.Lerp(startRotation, targetRotation, t);
                yield return null;
            }

            switchTransform.localEulerAngles = targetRotation;
            isAnimating = false;
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Draw switch state
            Gizmos.color = isActive ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, Vector3.one * 0.3f);
        }
    }
}
