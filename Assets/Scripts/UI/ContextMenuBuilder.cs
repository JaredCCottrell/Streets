using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Streets.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class ContextMenuBuilder : MonoBehaviour
    {
        [Header("Menu Settings")]
        [SerializeField] private float menuWidth = 120f;
        [SerializeField] private float buttonHeight = 30f;
        [SerializeField] private float buttonSpacing = 4f;
        [SerializeField] private float padding = 6f;
        [SerializeField] private float fontSize = 14f;

        [Header("Colors")]
        [SerializeField] private Color backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        [SerializeField] private Color buttonNormalColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        [SerializeField] private Color buttonHoverColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        [SerializeField] private Color buttonPressedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color buttonDisabledColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color textDisabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        [Header("Generated Buttons (Auto-populated)")]
        public Button useButton;
        public Button dropButton;
        public Button[] hotbarButtons;

        [Header("Hotbar Count")]
        [SerializeField] private int hotbarSlotCount = 4;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        [ContextMenu("Build Context Menu")]
        public void BuildContextMenu()
        {
            // Clear existing children
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            rectTransform = GetComponent<RectTransform>();

            // Setup the panel
            SetupPanel();

            // Create buttons
            useButton = CreateButton("Use");
            dropButton = CreateButton("Drop");

            // Create separator
            CreateSeparator();

            // Create hotbar buttons
            hotbarButtons = new Button[hotbarSlotCount];
            for (int i = 0; i < hotbarSlotCount; i++)
            {
                hotbarButtons[i] = CreateButton($"Hotbar {i + 1}");
            }

            // Force layout rebuild
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

        private void SetupPanel()
        {
            // Set anchors and pivot for top-left positioning
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);

            // Add background image
            Image bgImage = GetComponent<Image>();
            if (bgImage == null)
            {
                bgImage = gameObject.AddComponent<Image>();
            }
            bgImage.color = backgroundColor;

            // Add Content Size Fitter
            ContentSizeFitter csf = GetComponent<ContentSizeFitter>();
            if (csf == null)
            {
                csf = gameObject.AddComponent<ContentSizeFitter>();
            }
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Add Vertical Layout Group
            VerticalLayoutGroup vlg = GetComponent<VerticalLayoutGroup>();
            if (vlg == null)
            {
                vlg = gameObject.AddComponent<VerticalLayoutGroup>();
            }
            vlg.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
            vlg.spacing = buttonSpacing;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Add Layout Element for width
            LayoutElement le = GetComponent<LayoutElement>();
            if (le == null)
            {
                le = gameObject.AddComponent<LayoutElement>();
            }
            le.preferredWidth = menuWidth;
        }

        private Button CreateButton(string label)
        {
            // Create button GameObject
            GameObject buttonObj = new GameObject(label + "Button");
            buttonObj.transform.SetParent(transform, false);

            // Add RectTransform
            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();

            // Add Layout Element for height
            LayoutElement le = buttonObj.AddComponent<LayoutElement>();
            le.preferredHeight = buttonHeight;
            le.flexibleWidth = 1;

            // Add Image for button background
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = buttonNormalColor;

            // Add Button component
            Button button = buttonObj.AddComponent<Button>();

            // Setup button colors
            ColorBlock colors = button.colors;
            colors.normalColor = buttonNormalColor;
            colors.highlightedColor = buttonHoverColor;
            colors.pressedColor = buttonPressedColor;
            colors.disabledColor = buttonDisabledColor;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            // Create text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8, 0);
            textRect.offsetMax = new Vector2(-8, 0);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;

            return button;
        }

        private void CreateSeparator()
        {
            GameObject sepObj = new GameObject("Separator");
            sepObj.transform.SetParent(transform, false);

            RectTransform sepRect = sepObj.AddComponent<RectTransform>();

            LayoutElement le = sepObj.AddComponent<LayoutElement>();
            le.preferredHeight = 1;
            le.flexibleWidth = 1;

            Image sepImage = sepObj.AddComponent<Image>();
            sepImage.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-rebuild in editor when values change
            if (!Application.isPlaying && transform.childCount > 0)
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (this != null)
                    {
                        BuildContextMenu();
                    }
                };
            }
        }

        private void Reset()
        {
            BuildContextMenu();
        }
#endif
    }
}
