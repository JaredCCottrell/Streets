using UnityEngine;
using UnityEditor;
using Streets.Dialogue;

namespace Streets.Editor
{
    /// <summary>
    /// Editor tool to create a Shadow Figure NPC prefab.
    /// </summary>
    public class ShadowFigureBuilder : EditorWindow
    {
        [MenuItem("Streets/Create Shadow Figure NPC")]
        public static void CreateShadowFigure()
        {
            // Create root object
            GameObject figureObj = new GameObject("ShadowFigure_NPC");

            // Add ShadowFigure visual component
            ShadowFigure shadowFigure = figureObj.AddComponent<ShadowFigure>();

            // Add DialogueEntity for interaction
            DialogueEntity dialogueEntity = figureObj.AddComponent<DialogueEntity>();

            // Configure DialogueEntity defaults via SerializedObject
            SerializedObject serialized = new SerializedObject(dialogueEntity);
            serialized.FindProperty("interactionRange").floatValue = 4f;
            serialized.FindProperty("interactKey").enumValueIndex = (int)UnityEngine.InputSystem.Key.E;
            serialized.FindProperty("facePlayerDuringDialogue").boolValue = true;
            serialized.FindProperty("destroyAfterDialogue").boolValue = true;
            serialized.FindProperty("destroyDelay").floatValue = 0.5f;
            serialized.ApplyModifiedProperties();

            // Add a collider for potential physics interactions
            CapsuleCollider collider = figureObj.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0, 1f, 0);
            collider.radius = 0.4f;
            collider.height = 2f;
            collider.isTrigger = true; // So player doesn't bump into it

            // Create interact prompt as child
            GameObject promptObj = new GameObject("InteractPrompt");
            promptObj.transform.SetParent(figureObj.transform);
            promptObj.transform.localPosition = new Vector3(0, 2.5f, 0);

            // Add world-space canvas for prompt
            Canvas promptCanvas = promptObj.AddComponent<Canvas>();
            promptCanvas.renderMode = RenderMode.WorldSpace;

            RectTransform canvasRect = promptObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(150, 40);
            canvasRect.localScale = Vector3.one * 0.005f;

            // Add text to prompt
            GameObject textObj = new GameObject("PromptText");
            textObj.transform.SetParent(promptObj.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            TMPro.TextMeshProUGUI promptText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            promptText.text = "[E] Interact";
            promptText.fontSize = 36;
            promptText.color = Color.white;
            promptText.alignment = TMPro.TextAlignmentOptions.Center;
            promptText.fontStyle = TMPro.FontStyles.Bold;

            // Note: Add BillboardPrompt component manually if you want the prompt to face camera
            // promptObj.AddComponent<Streets.Dialogue.BillboardPrompt>();

            // Wire up the interact prompt reference
            serialized = new SerializedObject(dialogueEntity);
            serialized.FindProperty("interactPrompt").objectReferenceValue = promptObj;
            serialized.ApplyModifiedProperties();

            // Position in scene
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                figureObj.transform.position = sceneView.pivot;
            }

            // Select it
            Selection.activeGameObject = figureObj;

            Debug.Log("[ShadowFigureBuilder] Shadow Figure NPC created! Assign a DialogueData asset to test.");
            Debug.Log("[ShadowFigureBuilder] Drag to Assets/Prefabs/ to save as prefab.");
        }

        [MenuItem("Streets/Create Test Dialogue Data")]
        public static void CreateTestDialogue()
        {
            // Create the dialogue asset
            DialogueData dialogue = ScriptableObject.CreateInstance<DialogueData>();

            // Set up via SerializedObject
            SerializedObject serialized = new SerializedObject(dialogue);
            serialized.FindProperty("speakerName").stringValue = "???";
            serialized.FindProperty("speakerColor").colorValue = new Color(0.7f, 0.7f, 0.7f);

            // Create lines array
            SerializedProperty linesArray = serialized.FindProperty("lines");
            linesArray.arraySize = 4;

            // Line 0: Introduction
            SerializedProperty line0 = linesArray.GetArrayElementAtIndex(0);
            line0.FindPropertyRelative("text").stringValue = "You shouldn't be here, traveler...";
            line0.FindPropertyRelative("displayDuration").floatValue = 0;

            // Line 1: Question with choices
            SerializedProperty line1 = linesArray.GetArrayElementAtIndex(1);
            line1.FindPropertyRelative("text").stringValue = "Tell me... why do you walk this cursed road?";
            line1.FindPropertyRelative("displayDuration").floatValue = 0;

            SerializedProperty choices1 = line1.FindPropertyRelative("choices");
            choices1.arraySize = 3;

            // Choice 1: Positive
            SerializedProperty choice1 = choices1.GetArrayElementAtIndex(0);
            choice1.FindPropertyRelative("choiceText").stringValue = "I'm searching for the truth.";
            choice1.FindPropertyRelative("sanityChange").floatValue = 5f;
            choice1.FindPropertyRelative("responseText").stringValue = "Truth... yes. The road reveals all, in time.";
            choice1.FindPropertyRelative("jumpToLine").intValue = -1;
            choice1.FindPropertyRelative("endsDialogue").boolValue = false;

            // Choice 2: Neutral
            SerializedProperty choice2 = choices1.GetArrayElementAtIndex(1);
            choice2.FindPropertyRelative("choiceText").stringValue = "I don't know. I just... ended up here.";
            choice2.FindPropertyRelative("sanityChange").floatValue = 0f;
            choice2.FindPropertyRelative("responseText").stringValue = "None of us choose this path. It chooses us.";
            choice2.FindPropertyRelative("jumpToLine").intValue = -1;
            choice2.FindPropertyRelative("endsDialogue").boolValue = false;

            // Choice 3: Negative
            SerializedProperty choice3 = choices1.GetArrayElementAtIndex(2);
            choice3.FindPropertyRelative("choiceText").stringValue = "Get out of my way.";
            choice3.FindPropertyRelative("sanityChange").floatValue = -10f;
            choice3.FindPropertyRelative("responseText").stringValue = "Hostility... the road will remember this.";
            choice3.FindPropertyRelative("jumpToLine").intValue = 3; // Skip to end
            choice3.FindPropertyRelative("endsDialogue").boolValue = false;

            // Line 2: Follow-up
            SerializedProperty line2 = linesArray.GetArrayElementAtIndex(2);
            line2.FindPropertyRelative("text").stringValue = "Keep walking. But beware... not all shadows are as kind as I.";
            line2.FindPropertyRelative("displayDuration").floatValue = 0;

            // Line 3: Farewell
            SerializedProperty line3 = linesArray.GetArrayElementAtIndex(3);
            line3.FindPropertyRelative("text").stringValue = "We will meet again, traveler. The road ensures it.";
            line3.FindPropertyRelative("displayDuration").floatValue = 0;

            serialized.ApplyModifiedProperties();

            // Save asset
            string path = "Assets/Settings/Dialogue";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/Settings", "Dialogue");
            }

            AssetDatabase.CreateAsset(dialogue, path + "/Test_ShadowConversation.asset");
            AssetDatabase.SaveAssets();

            // Select it
            Selection.activeObject = dialogue;

            Debug.Log("[ShadowFigureBuilder] Test dialogue created at Assets/Settings/Dialogue/Test_ShadowConversation.asset");
        }
    }

}
