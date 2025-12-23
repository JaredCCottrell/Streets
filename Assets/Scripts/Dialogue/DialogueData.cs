using System;
using UnityEngine;

namespace Streets.Dialogue
{
    /// <summary>
    /// ScriptableObject containing a full dialogue sequence.
    /// </summary>
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Streets/Dialogue/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        [Header("Speaker Info")]
        [SerializeField] private string speakerName = "???";
        [SerializeField] private Color speakerColor = Color.white;

        [Header("Dialogue Lines")]
        [SerializeField] private DialogueLine[] lines;

        [Header("Completion Rewards")]
        [Tooltip("Sanity change when dialogue completes (can be negative)")]
        [SerializeField] private float completionSanityChange = 0f;

        [Tooltip("Item to drop when dialogue completes (optional)")]
        [SerializeField] private GameObject itemDropPrefab;

        // Properties
        public string SpeakerName => speakerName;
        public Color SpeakerColor => speakerColor;
        public DialogueLine[] Lines => lines;
        public float CompletionSanityChange => completionSanityChange;
        public GameObject ItemDropPrefab => itemDropPrefab;

        public int LineCount => lines != null ? lines.Length : 0;

        public DialogueLine GetLine(int index)
        {
            if (lines == null || index < 0 || index >= lines.Length)
                return null;
            return lines[index];
        }
    }

    /// <summary>
    /// A single line of dialogue, optionally with player choices.
    /// </summary>
    [Serializable]
    public class DialogueLine
    {
        [Header("Line Content")]
        [TextArea(2, 4)]
        [SerializeField] private string text;

        [Tooltip("Time to display before auto-advancing (0 = wait for input)")]
        [SerializeField] private float displayDuration = 0f;

        [Header("Player Choices (Optional)")]
        [Tooltip("If empty, player clicks to continue. If set, shows choice buttons.")]
        [SerializeField] private DialogueChoice[] choices;

        // Properties
        public string Text => text;
        public float DisplayDuration => displayDuration;
        public DialogueChoice[] Choices => choices;
        public bool HasChoices => choices != null && choices.Length > 0;
    }

    /// <summary>
    /// A player response choice with consequences.
    /// </summary>
    [Serializable]
    public class DialogueChoice
    {
        [Header("Choice")]
        [SerializeField] private string choiceText;

        [Header("Consequences")]
        [Tooltip("Sanity change when this choice is selected (can be negative)")]
        [SerializeField] private float sanityChange = 0f;

        [Tooltip("If set, jumps to this line index after choice. -1 = continue to next line.")]
        [SerializeField] private int jumpToLine = -1;

        [Tooltip("If true, ends the dialogue immediately after this choice")]
        [SerializeField] private bool endsDialogue = false;

        [Header("Response (Optional)")]
        [Tooltip("NPC response text shown after player picks this choice")]
        [TextArea(1, 2)]
        [SerializeField] private string responseText;

        // Properties
        public string ChoiceText => choiceText;
        public float SanityChange => sanityChange;
        public int JumpToLine => jumpToLine;
        public bool EndsDialogue => endsDialogue;
        public string ResponseText => responseText;
        public bool HasResponse => !string.IsNullOrEmpty(responseText);
    }
}
