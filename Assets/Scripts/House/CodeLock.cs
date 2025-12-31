using System;
using UnityEngine;
using UnityEngine.Events;
using Streets.House.UI;

namespace Streets.House
{
    /// <summary>
    /// A combination lock that requires a code to unlock.
    /// Can be connected to a door or trigger events on unlock.
    /// </summary>
    public class CodeLock : Interactable
    {
        [Header("Code Settings")]
        [SerializeField] private string correctCode = "1234";
        [SerializeField] private int maxCodeLength = 4;
        [SerializeField] private bool clearOnWrongCode = true;
        [SerializeField] private float wrongCodeDelay = 1f;

        [Header("Connected Objects")]
        [SerializeField] private Door connectedDoor;
        [SerializeField] private GameObject[] objectsToActivate;
        [SerializeField] private GameObject[] objectsToDeactivate;

        [Header("Code Lock Audio")]
        [SerializeField] private AudioClip buttonPressSound;
        [SerializeField] private AudioClip correctCodeSound;
        [SerializeField] private AudioClip wrongCodeSound;

        [Header("Events")]
        [SerializeField] private UnityEvent OnCodeCorrect;
        [SerializeField] private UnityEvent OnCodeIncorrect;

        // State
        private string currentInput = "";
        private bool isUnlocked = false;
        private bool isShowingUI = false;
        private bool isWaitingAfterWrong = false;

        // UI Reference
        private CodeLockUI codeLockUI;

        // Events
        public event Action<string> OnDigitEntered;
        public event Action OnInputCleared;
        public event Action OnUnlocked;

        // Properties
        public string CurrentInput => currentInput;
        public int MaxCodeLength => maxCodeLength;
        public bool IsUnlocked => isUnlocked;
        public bool IsShowingUI => isShowingUI;

        protected override void Start()
        {
            base.Start();

            // Find CodeLockUI in scene
            codeLockUI = FindObjectOfType<CodeLockUI>();

            // Update prompt
            promptText = "Press E to enter code";
            lockedPromptText = promptText;
        }

        protected override void Update()
        {
            base.Update();

            // Handle ESC to close UI
            if (isShowingUI && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                HideCodeUI();
            }
        }

        protected override void OnInteract()
        {
            if (isUnlocked)
            {
                // Already unlocked, maybe show a message
                return;
            }

            ShowCodeUI();
        }

        protected override bool CanInteract()
        {
            if (isUnlocked) return false;
            if (isShowingUI) return false;

            return true;
        }

        private void ShowCodeUI()
        {
            if (codeLockUI == null)
            {
                Debug.LogWarning("[CodeLock] No CodeLockUI found in scene!");
                return;
            }

            isShowingUI = true;
            currentInput = "";
            codeLockUI.Show(this);

            // Unlock cursor for UI
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Pause time (optional)
            Time.timeScale = 0f;
        }

        public void HideCodeUI()
        {
            if (codeLockUI != null)
            {
                codeLockUI.Hide();
            }

            isShowingUI = false;
            currentInput = "";

            // Re-lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Resume time
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Called by UI when a digit button is pressed
        /// </summary>
        public void EnterDigit(int digit)
        {
            if (isUnlocked || isWaitingAfterWrong) return;
            if (currentInput.Length >= maxCodeLength) return;

            currentInput += digit.ToString();
            PlaySound(buttonPressSound);
            OnDigitEntered?.Invoke(currentInput);

            // Auto-submit when max length reached
            if (currentInput.Length >= maxCodeLength)
            {
                CheckCode();
            }
        }

        /// <summary>
        /// Called by UI when a digit button is pressed (string version)
        /// </summary>
        public void EnterDigit(string digit)
        {
            if (int.TryParse(digit, out int d))
            {
                EnterDigit(d);
            }
        }

        /// <summary>
        /// Clear the current input
        /// </summary>
        public void ClearInput()
        {
            if (isWaitingAfterWrong) return;

            currentInput = "";
            OnInputCleared?.Invoke();
        }

        /// <summary>
        /// Remove the last digit
        /// </summary>
        public void Backspace()
        {
            if (isWaitingAfterWrong) return;
            if (currentInput.Length > 0)
            {
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
                OnDigitEntered?.Invoke(currentInput);
            }
        }

        /// <summary>
        /// Submit the current code
        /// </summary>
        public void SubmitCode()
        {
            CheckCode();
        }

        private void CheckCode()
        {
            if (currentInput == correctCode)
            {
                OnCodeSuccess();
            }
            else
            {
                OnCodeFailure();
            }
        }

        private void OnCodeSuccess()
        {
            isUnlocked = true;
            PlaySound(correctCodeSound ?? interactSound);

            // Unlock connected door
            if (connectedDoor != null)
            {
                connectedDoor.Unlock();
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

            OnCodeCorrect?.Invoke();
            OnUnlocked?.Invoke();

            Debug.Log("[CodeLock] Correct code entered!");

            // Close UI after short delay
            Invoke(nameof(HideCodeUI), 0.5f);
        }

        private void OnCodeFailure()
        {
            PlaySound(wrongCodeSound ?? failSound);
            OnCodeIncorrect?.Invoke();

            Debug.Log("[CodeLock] Wrong code!");

            if (clearOnWrongCode)
            {
                isWaitingAfterWrong = true;
                Invoke(nameof(ClearAfterWrongCode), wrongCodeDelay);
            }
        }

        private void ClearAfterWrongCode()
        {
            currentInput = "";
            isWaitingAfterWrong = false;
            OnInputCleared?.Invoke();
        }

        /// <summary>
        /// Set the correct code at runtime
        /// </summary>
        public void SetCorrectCode(string code)
        {
            correctCode = code;
        }

        /// <summary>
        /// Force unlock (for debugging or puzzle solutions)
        /// </summary>
        public void ForceUnlock()
        {
            currentInput = correctCode;
            OnCodeSuccess();
        }
    }
}
