using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Streets.Dialogue
{
    /// <summary>
    /// UI component for displaying dialogue subtitles and choices.
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Speaker")]
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private GameObject speakerNamePanel;

        [Header("Dialogue Text")]
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private GameObject continueIndicator;

        [Header("Choices")]
        [SerializeField] private GameObject choicesPanel;
        [SerializeField] private DialogueChoiceButton[] choiceButtons;

        [Header("Settings")]
        [SerializeField] private float typewriterSpeed = 0.03f;
        [SerializeField] private bool useTypewriter = true;

        // State
        private Action<int> currentChoiceCallback;
        private bool isTyping;
        private string targetText;
        private Coroutine typewriterCoroutine;

        private void Awake()
        {
            // Initialize choice buttons
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                int index = i; // Capture for closure
                if (choiceButtons[i] != null)
                {
                    choiceButtons[i].Initialize(index, OnChoiceClicked);
                }
            }

            Hide();
        }

        public void Show()
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);

            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
        }

        public void Hide()
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            HideChoices();
            StopTypewriter();
        }

        public void SetSpeaker(string name, Color color)
        {
            if (speakerNameText != null)
            {
                speakerNameText.text = name;
                speakerNameText.color = color;
            }

            if (speakerNamePanel != null)
            {
                speakerNamePanel.SetActive(!string.IsNullOrEmpty(name));
            }
        }

        public void DisplayLine(DialogueLine line)
        {
            DisplayText(line.Text);

            // Show/hide continue indicator
            if (continueIndicator != null)
            {
                continueIndicator.SetActive(!line.HasChoices);
            }
        }

        public void DisplayText(string text)
        {
            targetText = text;

            if (useTypewriter && typewriterSpeed > 0)
            {
                StartTypewriter(text);
            }
            else
            {
                if (dialogueText != null)
                    dialogueText.text = text;
            }
        }

        public void ShowChoices(DialogueChoice[] choices, Action<int> callback)
        {
            currentChoiceCallback = callback;

            if (choicesPanel != null)
                choicesPanel.SetActive(true);

            // Hide continue indicator when choices shown
            if (continueIndicator != null)
                continueIndicator.SetActive(false);

            // Setup buttons
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (choiceButtons[i] != null)
                {
                    if (i < choices.Length)
                    {
                        choiceButtons[i].SetChoice(choices[i].ChoiceText);
                        choiceButtons[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        choiceButtons[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        public void HideChoices()
        {
            if (choicesPanel != null)
                choicesPanel.SetActive(false);

            currentChoiceCallback = null;
        }

        private void OnChoiceClicked(int index)
        {
            currentChoiceCallback?.Invoke(index);
        }

        private void StartTypewriter(string text)
        {
            StopTypewriter();
            typewriterCoroutine = StartCoroutine(TypewriterRoutine(text));
        }

        private void StopTypewriter()
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }
            isTyping = false;
        }

        private System.Collections.IEnumerator TypewriterRoutine(string text)
        {
            isTyping = true;

            if (dialogueText != null)
            {
                dialogueText.text = "";

                foreach (char c in text)
                {
                    dialogueText.text += c;
                    yield return new WaitForSecondsRealtime(typewriterSpeed);
                }
            }

            isTyping = false;
        }

        /// <summary>
        /// Skip to end of typewriter effect
        /// </summary>
        public void CompleteTypewriter()
        {
            if (isTyping)
            {
                StopTypewriter();
                if (dialogueText != null)
                    dialogueText.text = targetText;
            }
        }
    }

    /// <summary>
    /// Individual choice button component
    /// </summary>
    [Serializable]
    public class DialogueChoiceButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI buttonText;

        private int choiceIndex;
        private Action<int> clickCallback;

        public void Initialize(int index, Action<int> callback)
        {
            choiceIndex = index;
            clickCallback = callback;

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnClick);
            }
        }

        public void SetChoice(string text)
        {
            if (buttonText != null)
                buttonText.text = text;
        }

        private void OnClick()
        {
            clickCallback?.Invoke(choiceIndex);
        }
    }
}
