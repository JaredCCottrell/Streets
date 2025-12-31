#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Streets.Inventory;
using Streets.House;
using Streets.Dialogue;

namespace Streets.Editor
{
    /// <summary>
    /// Editor utility to create key items for the house intro sequence.
    /// Use menu: Streets > Create House Key Items
    /// </summary>
    public static class HouseItemCreator
    {
        private const string ItemPath = "Assets/Resources/Items/";
        private const string DialoguePath = "Assets/Resources/Dialogue/";

        [MenuItem("Streets/Create House Items")]
        public static void CreateHouseKeyItems()
        {
            // Ensure folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Items"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Items");
            }

            // Create Beer
            CreateConsumableItem(
                "Beer",
                "Cold Beer",
                "Grandpa's favorite. He's been asking for one all day.",
                intoxication: 25f
            );

            // Create Garage Fridge Key
            CreateKeyItem(
                "Key_GarageFridge",
                "Fridge Key",
                "key_fridge",
                "A small key. Grandpa always locks the beer fridge for some reason."
            );

            // Create Bedroom Key
            CreateKeyItem(
                "Key_BedroomDoor",
                "Bedroom Key",
                "key_bedroom",
                "A small brass key. It looks like it fits the bedroom door."
            );

            // Create Basement Key
            CreateKeyItem(
                "Key_BasementDoor",
                "Basement Key",
                "key_basement",
                "A heavy iron key. The basement door, perhaps?"
            );

            // Create Front Door Key
            CreateKeyItem(
                "Key_FrontDoor",
                "Front Door Key",
                "key_front",
                "The house key. Time to get out of here."
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[HouseItemCreator] Created house items in Resources/Items/");
            EditorUtility.DisplayDialog("House Items Created",
                "Created items:\n- Beer (for grandpa)\n- Key_GarageFridge\n- Key_BedroomDoor\n- Key_BasementDoor\n- Key_FrontDoor\n\nLocated in Resources/Items/",
                "OK");
        }

        private static void CreateConsumableItem(string fileName, string displayName, string description,
            float healthRestore = 0, float intoxication = 0)
        {
            string fullPath = ItemPath + fileName + ".asset";

            if (AssetDatabase.LoadAssetAtPath<ConsumableData>(fullPath) != null)
            {
                Debug.Log($"[HouseItemCreator] {fileName} already exists, skipping.");
                return;
            }

            ConsumableData item = ScriptableObject.CreateInstance<ConsumableData>();
            item.itemName = displayName;
            item.description = description;
            item.healthRestore = healthRestore;
            item.intoxicationAmount = intoxication;

            AssetDatabase.CreateAsset(item, fullPath);
            Debug.Log($"[HouseItemCreator] Created: {fullPath}");
        }

        private static void CreateKeyItem(string fileName, string displayName, string keyId, string description)
        {
            string fullPath = ItemPath + fileName + ".asset";

            // Check if already exists
            if (AssetDatabase.LoadAssetAtPath<KeyItemData>(fullPath) != null)
            {
                Debug.Log($"[HouseItemCreator] {fileName} already exists, skipping.");
                return;
            }

            // Create the ScriptableObject
            KeyItemData keyItem = ScriptableObject.CreateInstance<KeyItemData>();
            keyItem.itemName = displayName;
            keyItem.description = description;
            keyItem.keyId = keyId;
            keyItem.canDrop = false;

            // Save as asset
            AssetDatabase.CreateAsset(keyItem, fullPath);
            Debug.Log($"[HouseItemCreator] Created: {fullPath}");
        }

        [MenuItem("Streets/Create House Scene")]
        public static void CreateHouseScene()
        {
            // Create a new scene
            var newScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
                UnityEditor.SceneManagement.NewSceneMode.Single
            );

            // Create hierarchy structure
            CreateHierarchyObject("--- ENVIRONMENT ---");
            var houseRoot = CreateHierarchyObject("House");
            CreateHierarchyObject("CulDeSac");

            CreateHierarchyObject("--- INTERACTABLES ---");
            CreateHierarchyObject("Doors");
            CreateHierarchyObject("Puzzles");
            CreateHierarchyObject("Examinables");

            CreateHierarchyObject("--- ITEMS ---");
            CreateHierarchyObject("Pickups");

            CreateHierarchyObject("--- MANAGERS ---");
            var managers = CreateHierarchyObject("Managers");

            // Add HouseProgressTracker
            managers.AddComponent<HouseProgressTracker>();

            CreateHierarchyObject("--- UI ---");

            // Add exit trigger placeholder
            var exitTrigger = CreateHierarchyObject("ExitTrigger");
            exitTrigger.AddComponent<BoxCollider>().isTrigger = true;
            exitTrigger.AddComponent<SceneExitTrigger>();
            exitTrigger.transform.position = new Vector3(0, 0, 50); // At end of cul-de-sac

            // Save the scene
            string scenePath = "Assets/Scenes/HouseScene.unity";
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(newScene, scenePath);

            Debug.Log($"[HouseItemCreator] Created HouseScene at {scenePath}");
            EditorUtility.DisplayDialog("House Scene Created",
                "Created HouseScene.unity with placeholder hierarchy.\n\nYou'll need to:\n1. Build out the house geometry\n2. Add door prefabs\n3. Configure puzzles\n4. Add the player prefab",
                "OK");
        }

        private static GameObject CreateHierarchyObject(string name)
        {
            var obj = new GameObject(name);
            return obj;
        }

        [MenuItem("Streets/Create Grandpa NPC Assets")]
        public static void CreateGrandpaAssets()
        {
            // Ensure dialogue folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Dialogue"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Dialogue");
            }

            // Create "wants beer" dialogue
            CreateDialogue(
                "Grandpa_WantsBeer",
                "Grandpa",
                new Color(0.8f, 0.7f, 0.5f),
                new string[]
                {
                    "Hey there, kiddo! Perfect timing.",
                    "Listen, could you do your old grandpa a favor?",
                    "Grab me a cold beer from the fridge in the garage.",
                    "I think I left the key somewhere in the bedroom... maybe the nightstand?",
                    "These old bones ain't what they used to be."
                }
            );

            // Create "receiving beer" dialogue
            CreateDialogue(
                "Grandpa_ReceivingBeer",
                "Grandpa",
                new Color(0.8f, 0.7f, 0.5f),
                new string[]
                {
                    "Oh! A cold one!",
                    "*takes the beer*",
                    "You're a lifesaver, kiddo. A real lifesaver.",
                    "*cracks open the beer*",
                    "Ahhhh... that hits the spot.",
                    "You know, you should get going. It's getting late.",
                    "Your parents will be wondering where you are."
                }
            );

            // Create "already has beer" dialogue
            CreateDialogue(
                "Grandpa_AlreadyHasBeer",
                "Grandpa",
                new Color(0.8f, 0.7f, 0.5f),
                new string[]
                {
                    "*sips beer*",
                    "Thanks again for the beer, kiddo.",
                    "Now get going! Don't keep your folks waiting."
                }
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[HouseItemCreator] Created grandpa dialogue assets in Resources/Dialogue/");
            EditorUtility.DisplayDialog("Grandpa Assets Created",
                "Created dialogue:\n- Grandpa_WantsBeer\n- Grandpa_ReceivingBeer\n- Grandpa_AlreadyHasBeer\n\nLocated in Resources/Dialogue/\n\nTo setup Grandpa:\n1. Create a GameObject with GrandpaNPC component\n2. Assign these dialogue assets\n3. Place in the house scene",
                "OK");
        }

        private static void CreateDialogue(string fileName, string speakerName, Color speakerColor, string[] lines)
        {
            string fullPath = DialoguePath + fileName + ".asset";

            if (AssetDatabase.LoadAssetAtPath<DialogueData>(fullPath) != null)
            {
                Debug.Log($"[HouseItemCreator] {fileName} already exists, skipping.");
                return;
            }

            DialogueData dialogue = ScriptableObject.CreateInstance<DialogueData>();

            // Use SerializedObject to set private fields
            SerializedObject so = new SerializedObject(dialogue);
            so.FindProperty("speakerName").stringValue = speakerName;
            so.FindProperty("speakerColor").colorValue = speakerColor;

            // Create lines array
            SerializedProperty linesArray = so.FindProperty("lines");
            linesArray.arraySize = lines.Length;

            for (int i = 0; i < lines.Length; i++)
            {
                SerializedProperty lineElement = linesArray.GetArrayElementAtIndex(i);
                lineElement.FindPropertyRelative("text").stringValue = lines[i];
                lineElement.FindPropertyRelative("displayDuration").floatValue = 0f;
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(dialogue, fullPath);
            Debug.Log($"[HouseItemCreator] Created: {fullPath}");
        }

        [MenuItem("Streets/Validate House Setup")]
        public static void ValidateHouseSetup()
        {
            bool allGood = true;
            string report = "House Setup Validation:\n\n";

            // Check for key items
            var bedroomKey = Resources.Load<KeyItemData>("Items/Key_BedroomDoor");
            var basementKey = Resources.Load<KeyItemData>("Items/Key_BasementDoor");
            var frontKey = Resources.Load<KeyItemData>("Items/Key_FrontDoor");

            report += bedroomKey != null ? "✓ Bedroom Key found\n" : "✗ Bedroom Key MISSING\n";
            report += basementKey != null ? "✓ Basement Key found\n" : "✗ Basement Key MISSING\n";
            report += frontKey != null ? "✓ Front Door Key found\n" : "✗ Front Door Key MISSING\n";

            if (bedroomKey == null || basementKey == null || frontKey == null)
            {
                allGood = false;
                report += "\nRun 'Streets > Create House Key Items' to create missing items.\n";
            }

            // Check for HouseScene
            var houseScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/HouseScene.unity");
            report += houseScene != null ? "✓ HouseScene.unity found\n" : "✗ HouseScene.unity MISSING\n";

            if (houseScene == null)
            {
                allGood = false;
                report += "\nRun 'Streets > Create House Scene' to create the scene.\n";
            }

            // Check build settings
            var scenes = EditorBuildSettings.scenes;
            bool houseInBuild = false;
            bool roadInBuild = false;

            foreach (var scene in scenes)
            {
                if (scene.path.Contains("HouseScene")) houseInBuild = true;
                if (scene.path.Contains("SampleScene") || scene.path.Contains("RoadScene")) roadInBuild = true;
            }

            report += houseInBuild ? "✓ HouseScene in Build Settings\n" : "✗ HouseScene NOT in Build Settings\n";
            report += roadInBuild ? "✓ Road Scene in Build Settings\n" : "✗ Road Scene NOT in Build Settings\n";

            report += "\n" + (allGood ? "All checks passed!" : "Some items need attention.");

            EditorUtility.DisplayDialog("House Setup Validation", report, "OK");
        }
    }
}
#endif
