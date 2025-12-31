using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Streets.Dialogue
{
    /// <summary>
    /// Manages dialogue flow, pauses time, and communicates with UI.
    /// Singleton - one per scene.
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private DialogueUI dialogueUI;

        [Header("Settings")]
        [SerializeField] private bool pauseTimeOnDialogue = true;
        [SerializeField] private Key advanceKey = Key.Space;
        [SerializeField] private Key skipKey = Key.Escape;

        // State
        private DialogueData currentDialogue;
        private DialogueEntity currentEntity;
        private Action<bool> currentCallback;
        private int currentLineIndex;
        private bool isDialogueActive;
        private bool waitingForChoice;
        private bool waitingForResponse;
        private float lineStartTime;

        // Events
        public event Action OnDialogueStarted;
        public event Action OnDialogueEnded;
        public event Action<DialogueLine> OnLineDisplayed;
        public event Action<DialogueChoice> OnChoiceMade;

        // Properties
        public bool IsDialogueActive => isDialogueActive;
        public DialogueData CurrentDialogue => currentDialogue;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (!isDialogueActive) return;

            // Don't process input if waiting for choice buttons
            if (waitingForChoice) return;

            // Handle advance input
            bool advancePressed = Keyboard.current != null && Keyboard.current[advanceKey].wasPressedThisFrame;
            bool clickPressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

            if (advancePressed || clickPressed)
            {
                if (waitingForResponse)
                {
                    // Clear response and continue
                    waitingForResponse = false;
                    AdvanceDialogue();
                }
                else
                {
                    AdvanceDialogue();
                }
            }

            // Handle skip
            if (Keyboard.current != null && Keyboard.current[skipKey].wasPressedThisFrame)
            {
                EndDialogue(skipped: true);
            }

            // Auto-advance if duration set
            var currentLine = currentDialogue?.GetLine(currentLineIndex);
            if (currentLine != null && currentLine.DisplayDuration > 0 && !currentLine.HasChoices)
            {
                if (Time.unscaledTime - lineStartTime >= currentLine.DisplayDuration)
                {
                    AdvanceDialogue();
                }
            }
        }

        /// <summary>
        /// Start a dialogue with an entity
        /// </summary>
        public void StartDialogue(DialogueData dialogue, DialogueEntity entity = null)
        {
            StartDialogueInternal(dialogue, entity, null);
        }

        /// <summary>
        /// Start a dialogue with a completion callback
        /// </summary>
        public void StartDialogue(DialogueData dialogue, Action<bool> onComplete)
        {
            StartDialogueInternal(dialogue, null, onComplete);
        }

        private void StartDialogueInternal(DialogueData dialogue, DialogueEntity entity, Action<bool> onComplete)
        {
            if (dialogue == null || dialogue.LineCount == 0)
            {
                Debug.LogWarning("[DialogueManager] Cannot start empty dialogue");
                onComplete?.Invoke(false);
                return;
            }

            if (isDialogueActive)
            {
                Debug.LogWarning("[DialogueManager] Dialogue already active");
                return;
            }

            currentDialogue = dialogue;
            currentEntity = entity;
            currentCallback = onComplete;
            currentLineIndex = 0;
            isDialogueActive = true;
            waitingForChoice = false;
            waitingForResponse = false;

            // Pause time
            if (pauseTimeOnDialogue)
            {
                Time.timeScale = 0f;
            }

            // Show UI
            if (dialogueUI != null)
            {
                dialogueUI.Show();
                dialogueUI.SetSpeaker(dialogue.SpeakerName, dialogue.SpeakerColor);
            }

            // Unlock and show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            OnDialogueStarted?.Invoke();
            DisplayCurrentLine();

            Debug.Log($"[DialogueManager] Started dialogue: {dialogue.name}");
        }

        private void DisplayCurrentLine()
        {
            var line = currentDialogue.GetLine(currentLineIndex);
            if (line == null)
            {
                EndDialogue();
                return;
            }

            lineStartTime = Time.unscaledTime;

            if (dialogueUI != null)
            {
                dialogueUI.DisplayLine(line);

                if (line.HasChoices)
                {
                    waitingForChoice = true;
                    dialogueUI.ShowChoices(line.Choices, OnPlayerChoice);
                }
                else
                {
                    dialogueUI.HideChoices();
                }
            }

            OnLineDisplayed?.Invoke(line);
        }

        private void OnPlayerChoice(int choiceIndex)
        {
            var line = currentDialogue.GetLine(currentLineIndex);
            if (line == null || !line.HasChoices || choiceIndex >= line.Choices.Length)
                return;

            var choice = line.Choices[choiceIndex];
            waitingForChoice = false;

            OnChoiceMade?.Invoke(choice);

            // Show response if any
            if (choice.HasResponse)
            {
                waitingForResponse = true;
                if (dialogueUI != null)
                {
                    dialogueUI.DisplayText(choice.ResponseText);
                    dialogueUI.HideChoices();
                }

                // Store jump info for after response
                if (choice.EndsDialogue)
                {
                    // Will end after response is acknowledged
                    currentLineIndex = -999;
                }
                else if (choice.JumpToLine >= 0)
                {
                    currentLineIndex = choice.JumpToLine - 1; // -1 because AdvanceDialogue will +1
                }
            }
            else
            {
                // No response, process immediately
                if (choice.EndsDialogue)
                {
                    EndDialogue();
                    return;
                }

                if (choice.JumpToLine >= 0)
                {
                    currentLineIndex = choice.JumpToLine;
                    DisplayCurrentLine();
                }
                else
                {
                    AdvanceDialogue();
                }
            }

            Debug.Log($"[DialogueManager] Choice made: {choice.ChoiceText} (Sanity: {choice.SanityChange:+0;-0;0})");
        }

        private void AdvanceDialogue()
        {
            // Check if we should end (from choice)
            if (currentLineIndex == -999)
            {
                EndDialogue();
                return;
            }

            currentLineIndex++;

            if (currentLineIndex >= currentDialogue.LineCount)
            {
                EndDialogue();
            }
            else
            {
                DisplayCurrentLine();
            }
        }

        private void EndDialogue(bool skipped = false)
        {
            if (!isDialogueActive) return;

            isDialogueActive = false;

            // Apply completion rewards (only if not skipped)
            if (!skipped && currentDialogue != null)
            {
                // Item drop
                if (currentDialogue.ItemDropPrefab != null && currentEntity != null)
                {
                    Vector3 dropPos = currentEntity.transform.position + Vector3.up * 0.5f;
                    Instantiate(currentDialogue.ItemDropPrefab, dropPos, Quaternion.identity);
                    Debug.Log("[DialogueManager] Item dropped");
                }
            }

            // Hide UI
            if (dialogueUI != null)
            {
                dialogueUI.Hide();
            }

            // Restore time
            if (pauseTimeOnDialogue)
            {
                Time.timeScale = 1f;
            }

            // Lock cursor back
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Notify entity
            if (currentEntity != null)
            {
                currentEntity.OnDialogueComplete(skipped);
            }

            // Call callback
            currentCallback?.Invoke(skipped);

            OnDialogueEnded?.Invoke();

            Debug.Log($"[DialogueManager] Dialogue ended (skipped: {skipped})");

            currentDialogue = null;
            currentEntity = null;
            currentCallback = null;
        }

        /// <summary>
        /// Force end the current dialogue
        /// </summary>
        public void ForceEndDialogue()
        {
            EndDialogue(skipped: true);
        }
    }
}
