using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Streets.House.UI
{
    /// <summary>
    /// UI for the code lock keypad.
    /// Shows a numeric keypad for entering codes.
    /// </summary>
    public class CodeLockUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI displayText;
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("Buttons")]
        [SerializeField] private Button[] digitButtons; // 0-9
        [SerializeField] private Button clearButton;
        [SerializeField] private Button backspaceButton;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button closeButton;

        [Header("Display Settings")]
        [SerializeField] private char maskCharacter = '*';
        [SerializeField] private bool maskInput = false;
        [SerializeField] private string emptyDisplayText = "----";
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color errorColor = Color.red;
        [SerializeField] private Color successColor = Color.green;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip buttonClickSound;

        // Current lock reference
        private CodeLock currentLock;

        private void Awake()
        {
            // Setup button listeners
            SetupButtons();

            // Hide panel initially
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void SetupButtons()
        {
            // Digit buttons (0-9)
            for (int i = 0; i < digitButtons.Length && i <= 9; i++)
            {
                int digit = i;
                if (digitButtons[i] != null)
                {
                    digitButtons[i].onClick.AddListener(() => OnDigitPressed(digit));
                }
            }

            // Clear button
            if (clearButton != null)
            {
                clearButton.onClick.AddListener(OnClearPressed);
            }

            // Backspace button
            if (backspaceButton != null)
            {
                backspaceButton.onClick.AddListener(OnBackspacePressed);
            }

            // Submit button
            if (submitButton != null)
            {
                submitButton.onClick.AddListener(OnSubmitPressed);
            }

            // Close button
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnClosePressed);
            }
        }

        /// <summary>
        /// Show the keypad for a specific code lock
        /// </summary>
        public void Show(CodeLock codeLock)
        {
            currentLock = codeLock;

            if (panel != null)
            {
                panel.SetActive(true);
            }

            // Subscribe to lock events
            if (currentLock != null)
            {
                currentLock.OnDigitEntered += UpdateDisplay;
                currentLock.OnInputCleared += OnInputCleared;
                currentLock.OnUnlocked += OnUnlocked;
            }

            // Reset display
            UpdateDisplay("");
            SetDisplayColor(normalColor);

            // Set title if available
            if (titleText != null)
            {
                titleText.text = "Enter Code";
            }
        }

        /// <summary>
        /// Hide the keypad
        /// </summary>
        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }

            // Unsubscribe from lock events
            if (currentLock != null)
            {
                currentLock.OnDigitEntered -= UpdateDisplay;
                currentLock.OnInputCleared -= OnInputCleared;
                currentLock.OnUnlocked -= OnUnlocked;
            }

            currentLock = null;
        }

        private void OnDigitPressed(int digit)
        {
            PlayButtonSound();

            if (currentLock != null)
            {
                currentLock.EnterDigit(digit);
            }
        }

        private void OnClearPressed()
        {
            PlayButtonSound();

            if (currentLock != null)
            {
                currentLock.ClearInput();
            }
        }

        private void OnBackspacePressed()
        {
            PlayButtonSound();

            if (currentLock != null)
            {
                currentLock.Backspace();
            }
        }

        private void OnSubmitPressed()
        {
            PlayButtonSound();

            if (currentLock != null)
            {
                currentLock.SubmitCode();
            }
        }

        private void OnClosePressed()
        {
            PlayButtonSound();

            if (currentLock != null)
            {
                currentLock.HideCodeUI();
            }
        }

        private void UpdateDisplay(string input)
        {
            if (displayText == null) return;

            if (string.IsNullOrEmpty(input))
            {
                displayText.text = emptyDisplayText;
            }
            else if (maskInput)
            {
                displayText.text = new string(maskCharacter, input.Length);
            }
            else
            {
                displayText.text = input;
            }

            SetDisplayColor(normalColor);
        }

        private void OnInputCleared()
        {
            UpdateDisplay("");
            SetDisplayColor(normalColor);
        }

        private void OnUnlocked()
        {
            SetDisplayColor(successColor);

            if (titleText != null)
            {
                titleText.text = "Access Granted";
            }
        }

        public void ShowError()
        {
            SetDisplayColor(errorColor);

            if (titleText != null)
            {
                titleText.text = "Wrong Code";
            }

            // Reset after delay
            Invoke(nameof(ResetTitle), 1f);
        }

        private void ResetTitle()
        {
            if (titleText != null)
            {
                titleText.text = "Enter Code";
            }
            SetDisplayColor(normalColor);
        }

        private void SetDisplayColor(Color color)
        {
            if (displayText != null)
            {
                displayText.color = color;
            }
        }

        private void PlayButtonSound()
        {
            if (buttonClickSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(buttonClickSound);
            }
        }

        /// <summary>
        /// Trigger keyboard input (for physical keyboard support)
        /// </summary>
        private void Update()
        {
            if (!panel.activeInHierarchy || currentLock == null) return;

            // Number keys
            for (int i = 0; i <= 9; i++)
            {
                if (UnityEngine.InputSystem.Keyboard.current[KeyCodeForDigit(i)].wasPressedThisFrame)
                {
                    OnDigitPressed(i);
                }
            }

            // Backspace
            if (UnityEngine.InputSystem.Keyboard.current.backspaceKey.wasPressedThisFrame)
            {
                OnBackspacePressed();
            }

            // Enter to submit
            if (UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame ||
                UnityEngine.InputSystem.Keyboard.current.numpadEnterKey.wasPressedThisFrame)
            {
                OnSubmitPressed();
            }
        }

        private UnityEngine.InputSystem.Key KeyCodeForDigit(int digit)
        {
            return digit switch
            {
                0 => UnityEngine.InputSystem.Key.Digit0,
                1 => UnityEngine.InputSystem.Key.Digit1,
                2 => UnityEngine.InputSystem.Key.Digit2,
                3 => UnityEngine.InputSystem.Key.Digit3,
                4 => UnityEngine.InputSystem.Key.Digit4,
                5 => UnityEngine.InputSystem.Key.Digit5,
                6 => UnityEngine.InputSystem.Key.Digit6,
                7 => UnityEngine.InputSystem.Key.Digit7,
                8 => UnityEngine.InputSystem.Key.Digit8,
                9 => UnityEngine.InputSystem.Key.Digit9,
                _ => UnityEngine.InputSystem.Key.Digit0
            };
        }
    }
}
