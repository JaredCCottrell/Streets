using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using Streets.Dialogue;

namespace Streets.Editor
{
    /// <summary>
    /// Editor tool to build the DialogueUI canvas structure.
    /// </summary>
    public class DialogueUIBuilder : EditorWindow
    {
        [MenuItem("Streets/Build Dialogue UI")]
        public static void BuildDialogueUI()
        {
            // Create Canvas
            GameObject canvasObj = new GameObject("DialogueCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // On top of other UI

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Add DialogueUI component
            DialogueUI dialogueUI = canvasObj.AddComponent<DialogueUI>();

            // Create main panel (anchored to bottom)
            GameObject panelObj = CreatePanel(canvasObj.transform, "DialoguePanel",
                new Color(0, 0, 0, 0.85f));
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0);
            panelRect.anchorMax = new Vector2(0.9f, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.anchoredPosition = new Vector2(0, 20);
            panelRect.sizeDelta = new Vector2(0, 350);

            // Add padding to panel
            VerticalLayoutGroup panelLayout = panelObj.AddComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(30, 30, 15, 15);
            panelLayout.spacing = 10;
            panelLayout.childAlignment = TextAnchor.UpperLeft;
            panelLayout.childControlWidth = true;
            panelLayout.childControlHeight = true;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;

            // Add content size fitter to auto-resize
            ContentSizeFitter panelFitter = panelObj.AddComponent<ContentSizeFitter>();
            panelFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Create speaker name panel
            GameObject speakerPanel = CreatePanel(panelObj.transform, "SpeakerNamePanel",
                new Color(0.2f, 0.2f, 0.2f, 0.9f));
            RectTransform speakerRect = speakerPanel.GetComponent<RectTransform>();
            LayoutElement speakerLayout = speakerPanel.AddComponent<LayoutElement>();
            speakerLayout.preferredHeight = 35;
            speakerLayout.preferredWidth = 200;

            HorizontalLayoutGroup speakerHLayout = speakerPanel.AddComponent<HorizontalLayoutGroup>();
            speakerHLayout.padding = new RectOffset(15, 15, 5, 5);
            speakerHLayout.childAlignment = TextAnchor.MiddleLeft;

            // Add mask so text doesn't bleed outside panel
            speakerPanel.AddComponent<RectMask2D>();

            // Speaker name text
            GameObject speakerTextObj = CreateTextMeshPro(speakerPanel.transform, "SpeakerNameText", "");
            TextMeshProUGUI speakerText = speakerTextObj.GetComponent<TextMeshProUGUI>();
            speakerText.fontSize = 20;
            speakerText.fontStyle = FontStyles.Bold;
            speakerText.color = Color.white;
            speakerText.overflowMode = TextOverflowModes.Ellipsis;

            // Create dialogue text area
            GameObject dialogueTextObj = CreateTextMeshPro(panelObj.transform, "DialogueText", "");
            TextMeshProUGUI dialogueText = dialogueTextObj.GetComponent<TextMeshProUGUI>();
            dialogueText.fontSize = 24;
            dialogueText.color = Color.white;
            dialogueText.alignment = TextAlignmentOptions.TopLeft;
            LayoutElement dialogueLayout = dialogueTextObj.AddComponent<LayoutElement>();
            dialogueLayout.preferredHeight = 60;
            dialogueLayout.flexibleWidth = 1;

            // Create continue indicator
            GameObject continueObj = CreateTextMeshPro(panelObj.transform, "ContinueIndicator",
                "[ Space / Click to continue ]");
            TextMeshProUGUI continueText = continueObj.GetComponent<TextMeshProUGUI>();
            continueText.fontSize = 16;
            continueText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            continueText.alignment = TextAlignmentOptions.Right;
            LayoutElement continueLayout = continueObj.AddComponent<LayoutElement>();
            continueLayout.preferredHeight = 25;

            // Create choices panel
            GameObject choicesPanel = new GameObject("ChoicesPanel");
            choicesPanel.transform.SetParent(panelObj.transform);
            RectTransform choicesRect = choicesPanel.AddComponent<RectTransform>();
            choicesRect.sizeDelta = Vector2.zero;

            VerticalLayoutGroup choicesLayout = choicesPanel.AddComponent<VerticalLayoutGroup>();
            choicesLayout.spacing = 5;
            choicesLayout.childAlignment = TextAnchor.UpperLeft;
            choicesLayout.childControlWidth = true;
            choicesLayout.childControlHeight = true;
            choicesLayout.childForceExpandWidth = true;
            choicesLayout.childForceExpandHeight = false;

            // Let content size fitter determine height based on children
            ContentSizeFitter choicesFitter = choicesPanel.AddComponent<ContentSizeFitter>();
            choicesFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            LayoutElement choicesLayoutElement = choicesPanel.AddComponent<LayoutElement>();
            choicesLayoutElement.flexibleWidth = 1;

            // Create choice buttons
            DialogueChoiceButton[] choiceButtons = new DialogueChoiceButton[4];
            for (int i = 0; i < 4; i++)
            {
                GameObject buttonObj = CreateChoiceButton(choicesPanel.transform, $"ChoiceButton{i + 1}",
                    $"{i + 1}. Choice option {i + 1}");
                choiceButtons[i] = buttonObj.GetComponent<DialogueChoiceButton>();
            }

            // Wire up DialogueUI references using SerializedObject
            SerializedObject serializedUI = new SerializedObject(dialogueUI);
            serializedUI.FindProperty("dialoguePanel").objectReferenceValue = panelObj;
            serializedUI.FindProperty("speakerNameText").objectReferenceValue = speakerText;
            serializedUI.FindProperty("speakerNamePanel").objectReferenceValue = speakerPanel;
            serializedUI.FindProperty("dialogueText").objectReferenceValue = dialogueText;
            serializedUI.FindProperty("continueIndicator").objectReferenceValue = continueObj;
            serializedUI.FindProperty("choicesPanel").objectReferenceValue = choicesPanel;

            SerializedProperty choiceButtonsArray = serializedUI.FindProperty("choiceButtons");
            choiceButtonsArray.arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                choiceButtonsArray.GetArrayElementAtIndex(i).objectReferenceValue = choiceButtons[i];
            }

            serializedUI.ApplyModifiedProperties();

            // Select the canvas
            Selection.activeGameObject = canvasObj;

            // Try to find and assign to DialogueManager
            DialogueManager manager = Object.FindObjectOfType<DialogueManager>();
            if (manager != null)
            {
                SerializedObject serializedManager = new SerializedObject(manager);
                serializedManager.FindProperty("dialogueUI").objectReferenceValue = dialogueUI;
                serializedManager.ApplyModifiedProperties();
                Debug.Log("[DialogueUIBuilder] Assigned DialogueUI to DialogueManager");
            }

            Debug.Log("[DialogueUIBuilder] DialogueUI canvas created successfully!");
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent);

            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            Image image = panel.AddComponent<Image>();
            image.color = color;

            return panel;
        }

        private static GameObject CreateTextMeshPro(Transform parent, string name, string text)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent);

            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.color = Color.white;

            return textObj;
        }

        private static GameObject CreateChoiceButton(Transform parent, string name, string text)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent);

            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 40);

            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
            image.raycastTarget = true;

            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);
            colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            colors.pressedColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            colors.selectedColor = new Color(0.35f, 0.35f, 0.35f, 1f);
            button.colors = colors;

            // Add layout element
            LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
            layout.preferredHeight = 40;
            layout.minHeight = 40;
            layout.flexibleWidth = 1;

            // Create text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = new Vector2(-20, 0);
            textRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 20;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
            tmp.raycastTarget = false; // Let clicks pass through to button

            // Add DialogueChoiceButton component
            DialogueChoiceButton choiceButton = buttonObj.AddComponent<DialogueChoiceButton>();

            // Wire up references
            SerializedObject serialized = new SerializedObject(choiceButton);
            serialized.FindProperty("button").objectReferenceValue = button;
            serialized.FindProperty("buttonText").objectReferenceValue = tmp;
            serialized.ApplyModifiedProperties();

            return buttonObj;
        }
    }
}
