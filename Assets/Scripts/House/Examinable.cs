using System;
using UnityEngine;
using UnityEngine.Events;
using Streets.Inventory;
using Streets.House.UI;

namespace Streets.House
{
    /// <summary>
    /// An object that can be examined to reveal text, images, or clues.
    /// Great for notes, photos, documents, and environmental storytelling.
    /// </summary>
    public class Examinable : Interactable
    {
        [Header("Examine Content")]
        [SerializeField] private string examineTitle = "Note";
        [TextArea(3, 10)]
        [SerializeField] private string examineText = "A mysterious note...";
        [SerializeField] private Sprite examineImage;

        [Header("Code Hint")]
        [Tooltip("If true, this item reveals a code clue")]
        [SerializeField] private bool revealsCode = false;
        [SerializeField] private string codeHint;
        [SerializeField] private string codeHintLabel = "Code: ";

        [Header("Item Pickup")]
        [Tooltip("Pick up an item after examining")]
        [SerializeField] private bool pickupItemAfterExamine = false;
        [SerializeField] private ItemData itemToPickup;
        [SerializeField] private int pickupQuantity = 1;

        [Header("Examine Behavior")]
        [SerializeField] private bool destroyAfterExamine = false;
        [SerializeField] private bool canReExamine = true;

        [Header("Events")]
        [SerializeField] private UnityEvent OnExamined;
        [SerializeField] private UnityEvent OnExamineClosed;

        // State
        private bool hasBeenExamined = false;
        private bool isShowingUI = false;

        // UI Reference
        private ExamineUI examineUI;

        // Events
        public event Action OnExamineStarted;
        public event Action OnExamineEnded;

        // Properties
        public string Title => examineTitle;
        public string Text => examineText;
        public Sprite Image => examineImage;
        public bool RevealsCode => revealsCode;
        public string CodeHint => codeHint;
        public bool HasBeenExamined => hasBeenExamined;

        protected override void Start()
        {
            base.Start();

            // Find ExamineUI in scene
            examineUI = FindObjectOfType<ExamineUI>();

            promptText = "Press E to examine";
        }

        protected override void Update()
        {
            base.Update();

            // Handle closing examine UI
            if (isShowingUI)
            {
                if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame ||
                    UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame ||
                    UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
                {
                    CloseExamine();
                }
            }
        }

        protected override void OnInteract()
        {
            ShowExamine();
        }

        protected override bool CanInteract()
        {
            if (isShowingUI) return false;
            if (hasBeenExamined && !canReExamine) return false;

            return base.CanInteract();
        }

        private void ShowExamine()
        {
            if (examineUI == null)
            {
                Debug.LogWarning("[Examinable] No ExamineUI found in scene!");
                // Fallback: log the content
                Debug.Log($"[Examinable] {examineTitle}: {examineText}");
                OnExamineComplete();
                return;
            }

            isShowingUI = true;
            hasBeenExamined = true;

            // Build display text
            string displayText = examineText;
            if (revealsCode && !string.IsNullOrEmpty(codeHint))
            {
                displayText += $"\n\n<color=#FFD700>{codeHintLabel}{codeHint}</color>";
            }

            examineUI.Show(examineTitle, displayText, examineImage);

            // Unlock cursor for UI
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Pause time
            Time.timeScale = 0f;

            OnExamineStarted?.Invoke();
            OnExamined?.Invoke();
        }

        public void CloseExamine()
        {
            if (!isShowingUI) return;

            if (examineUI != null)
            {
                examineUI.Hide();
            }

            isShowingUI = false;

            // Re-lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Resume time
            Time.timeScale = 1f;

            OnExamineComplete();
        }

        private void OnExamineComplete()
        {
            // Pickup item if configured
            if (pickupItemAfterExamine && itemToPickup != null && inventorySystem != null)
            {
                bool added = inventorySystem.AddItem(itemToPickup, pickupQuantity);
                if (added)
                {
                    Debug.Log($"[Examinable] Picked up {itemToPickup.itemName}");
                }
            }

            OnExamineEnded?.Invoke();
            OnExamineClosed?.Invoke();

            // Destroy if configured
            if (destroyAfterExamine)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Set examine content at runtime
        /// </summary>
        public void SetContent(string title, string text, Sprite image = null)
        {
            examineTitle = title;
            examineText = text;
            examineImage = image;
        }

        /// <summary>
        /// Set code hint at runtime
        /// </summary>
        public void SetCodeHint(string hint)
        {
            revealsCode = true;
            codeHint = hint;
        }
    }
}
