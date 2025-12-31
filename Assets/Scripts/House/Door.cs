using System;
using System.Collections;
using UnityEngine;

namespace Streets.House
{
    /// <summary>
    /// A door that can be opened, closed, and locked.
    /// Can require a key item to unlock.
    /// </summary>
    public class Door : Interactable
    {
        public enum DoorState { Closed, Open, Locked }

        [Header("Door Settings")]
        [SerializeField] private DoorState initialState = DoorState.Closed;
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private bool stayOpen = false;

        [Header("Door Pivot")]
        [Tooltip("The transform to rotate. If null, uses this object.")]
        [SerializeField] private Transform doorPivot;

        [Header("Lock Settings")]
        [SerializeField] private string lockedMessage = "The door is locked.";
        [SerializeField] private bool unlockOnKeyUse = true;

        [Header("Door Audio")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] private AudioClip lockedSound;
        [SerializeField] private AudioClip unlockSound;

        // State
        private DoorState currentState;
        private Quaternion closedRotation;
        private Quaternion openRotation;
        private bool isAnimating;

        // Events
        public event Action OnDoorOpened;
        public event Action OnDoorClosed;
        public event Action OnDoorUnlocked;
        public event Action OnDoorLocked;

        // Properties
        public DoorState CurrentState => currentState;
        public bool IsOpen => currentState == DoorState.Open;
        public bool IsLocked => currentState == DoorState.Locked;
        public bool IsClosed => currentState == DoorState.Closed;

        protected override void Start()
        {
            base.Start();

            // Use this transform if no pivot specified
            if (doorPivot == null)
            {
                doorPivot = transform;
            }

            // Store initial rotation as closed
            closedRotation = doorPivot.localRotation;
            openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);

            // Set initial state
            currentState = initialState;

            // If starting open, set rotation
            if (initialState == DoorState.Open)
            {
                doorPivot.localRotation = openRotation;
            }

            // Update prompt based on state
            UpdatePromptText();
        }

        protected override void OnInteract()
        {
            switch (currentState)
            {
                case DoorState.Locked:
                    // If we have the key, unlock and open
                    if (requiresItem && HasRequiredItem())
                    {
                        Unlock();
                        if (unlockOnKeyUse)
                        {
                            Open();
                        }
                    }
                    break;

                case DoorState.Closed:
                    Open();
                    break;

                case DoorState.Open:
                    if (!stayOpen)
                    {
                        Close();
                    }
                    break;
            }
        }

        protected override bool CanInteract()
        {
            if (isAnimating) return false;

            // If locked and no key, still allow interaction (to show "locked" feedback)
            if (currentState == DoorState.Locked)
            {
                return true; // OnInteract will handle the locked case
            }

            // If open and stayOpen, can't interact
            if (currentState == DoorState.Open && stayOpen)
            {
                return false;
            }

            return base.CanInteract();
        }

        protected override void OnInteractFailed()
        {
            if (currentState == DoorState.Locked)
            {
                PlaySound(lockedSound ?? failSound);
                // Could show locked message via UI here
                Debug.Log($"[Door] {lockedMessage}");
            }
            else
            {
                base.OnInteractFailed();
            }
        }

        protected override string GetDynamicPrompt()
        {
            switch (currentState)
            {
                case DoorState.Locked:
                    if (requiresItem && HasRequiredItem())
                    {
                        return "Press E to unlock";
                    }
                    return lockedPromptText;

                case DoorState.Open:
                    return stayOpen ? "" : "Press E to close";

                case DoorState.Closed:
                default:
                    return promptText;
            }
        }

        private void UpdatePromptText()
        {
            promptText = "Press E to open";
            lockedPromptText = lockedMessage;
        }

        #region Door Actions

        public void Open()
        {
            if (currentState == DoorState.Open || isAnimating) return;
            if (currentState == DoorState.Locked)
            {
                Debug.LogWarning("[Door] Cannot open - door is locked!");
                return;
            }

            StartCoroutine(AnimateDoor(openRotation, DoorState.Open));
            PlaySound(openSound);
        }

        public void Close()
        {
            if (currentState == DoorState.Closed || isAnimating) return;

            StartCoroutine(AnimateDoor(closedRotation, DoorState.Closed));
            PlaySound(closeSound);
        }

        public void Toggle()
        {
            if (currentState == DoorState.Open)
            {
                Close();
            }
            else if (currentState == DoorState.Closed)
            {
                Open();
            }
        }

        public void Lock()
        {
            if (currentState == DoorState.Locked) return;

            // Close first if open
            if (currentState == DoorState.Open)
            {
                StartCoroutine(LockAfterClose());
                return;
            }

            currentState = DoorState.Locked;
            OnDoorLocked?.Invoke();
        }

        private IEnumerator LockAfterClose()
        {
            Close();
            while (isAnimating)
            {
                yield return null;
            }
            currentState = DoorState.Locked;
            OnDoorLocked?.Invoke();
        }

        public void Unlock()
        {
            if (currentState != DoorState.Locked) return;

            // Consume key if configured
            if (requiresItem && consumeItemOnUse && inventorySystem != null)
            {
                ConsumeRequiredItem();
            }

            currentState = DoorState.Closed;
            PlaySound(unlockSound);
            OnDoorUnlocked?.Invoke();

            Debug.Log("[Door] Unlocked!");
        }

        /// <summary>
        /// Force set door state without animation
        /// </summary>
        public void SetState(DoorState state)
        {
            currentState = state;
            doorPivot.localRotation = state == DoorState.Open ? openRotation : closedRotation;
        }

        #endregion

        private IEnumerator AnimateDoor(Quaternion targetRotation, DoorState endState)
        {
            isAnimating = true;

            Quaternion startRotation = doorPivot.localRotation;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                // Ease out quad for smooth door feel
                t = 1 - (1 - t) * (1 - t);

                doorPivot.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);
                yield return null;
            }

            doorPivot.localRotation = targetRotation;
            currentState = endState;
            isAnimating = false;

            if (endState == DoorState.Open)
            {
                OnDoorOpened?.Invoke();
            }
            else if (endState == DoorState.Closed)
            {
                OnDoorClosed?.Invoke();
            }
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Draw door swing arc
            if (doorPivot != null)
            {
                Gizmos.color = IsLocked ? Color.red : (IsOpen ? Color.green : Color.cyan);

                Vector3 pivot = doorPivot.position;
                Vector3 closedDir = doorPivot.forward;
                Vector3 openDir = Quaternion.Euler(0, openAngle, 0) * closedDir;

                Gizmos.DrawLine(pivot, pivot + closedDir);
                Gizmos.DrawLine(pivot, pivot + openDir);
            }
        }
    }
}
