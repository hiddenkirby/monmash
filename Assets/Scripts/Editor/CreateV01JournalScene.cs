using System.IO;
using Tidepool.Runtime;
using Tidepool.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tidepool.Editor
{
    public static class CreateV01JournalScene
    {
        private const string ScenePath = "Assets/Scenes/Journal.unity";
        private const string SlotPrefabPath = "Assets/Prefabs/JournalSlot.prefab";
        private const string SpeciesDatabasePath = "Assets/Data/Databases/SpeciesDatabase.asset";

        [MenuItem("Tools/Tidepool/Create v0.1 Journal Scene")]
        public static void CreateJournalScene()
        {
            EnsureFolder("Assets/Scenes");
            EnsureFolder("Assets/Prefabs");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Canvas canvas = CreateCanvas();
            CreateEventSystem();

            RectTransform safeArea = CreateRect("SafeArea", canvas.transform);
            safeArea.anchorMin = Vector2.zero;
            safeArea.anchorMax = Vector2.one;
            safeArea.offsetMin = Vector2.zero;
            safeArea.offsetMax = Vector2.zero;
            safeArea.gameObject.AddComponent<SafeAreaFitter>();

            Image background = CreateImage("Background", safeArea, new Color(0.70f, 0.88f, 0.91f), Vector2.zero, new Vector2(1024f, 768f));
            background.rectTransform.anchorMin = Vector2.zero;
            background.rectTransform.anchorMax = Vector2.one;
            background.rectTransform.offsetMin = Vector2.zero;
            background.rectTransform.offsetMax = Vector2.zero;

            Text titleText = CreateText("JournalTitle", safeArea, "Journal", 40, TextAnchor.MiddleCenter, new Vector2(0f, 330f), new Vector2(600f, 72f));
            titleText.color = new Color(0.08f, 0.18f, 0.22f);

            Text progressText = CreateText("ProgressText", safeArea, "0 of 13 found", 28, TextAnchor.MiddleCenter, new Vector2(0f, 288f), new Vector2(600f, 56f));
            progressText.color = new Color(0.08f, 0.18f, 0.22f);

            RectTransform gridRoot = CreateRect("GridRoot", safeArea);
            gridRoot.anchoredPosition = new Vector2(-380f, 120f);
            gridRoot.sizeDelta = new Vector2(300f, 400f);
            GridLayoutGroup gridLayout = gridRoot.gameObject.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(88f, 88f);
            gridLayout.spacing = new Vector2(8f, 8f);
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperLeft;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3;

            RectTransform detailPanel = CreateRect("DetailPanel", safeArea);
            detailPanel.anchoredPosition = new Vector2(120f, 120f);
            detailPanel.sizeDelta = new Vector2(500f, 400f);
            Image detailPanelBg = detailPanel.gameObject.AddComponent<Image>();
            detailPanelBg.color = new Color(0.08f, 0.22f, 0.24f, 0.90f);

            Image detailImage = CreateImage("DetailImage", detailPanel, Color.white, new Vector2(0f, 130f), new Vector2(180f, 180f));
            detailImage.preserveAspect = true;

            Text detailName = CreateText("DetailName", detailPanel, "?", 30, TextAnchor.MiddleCenter, new Vector2(0f, 20f), new Vector2(480f, 48f));
            detailName.color = Color.white;

            Text detailCurrent = CreateText("DetailCurrent", detailPanel, "Unknown", 24, TextAnchor.MiddleCenter, new Vector2(-130f, -24f), new Vector2(220f, 40f));
            detailCurrent.color = Color.white;

            Image detailCurrentIcon = CreateImage("DetailCurrentIcon", detailPanel, Color.white, new Vector2(-250f, -24f), new Vector2(40f, 40f));
            detailCurrentIcon.preserveAspect = true;

            Text detailHabitat = CreateText("DetailHabitat", detailPanel, "Unknown", 22, TextAnchor.MiddleLeft, new Vector2(0f, -68f), new Vector2(480f, 36f));
            detailHabitat.color = Color.white;

            Text detailCaught = CreateText("DetailCaught", detailPanel, "Not found yet", 22, TextAnchor.MiddleLeft, new Vector2(0f, -104f), new Vector2(480f, 36f));
            detailCaught.color = Color.white;

            Text detailLevel = CreateText("DetailLevel", detailPanel, "Unknown", 22, TextAnchor.MiddleLeft, new Vector2(0f, -140f), new Vector2(480f, 36f));
            detailLevel.color = Color.white;

            Text detailGrowth = CreateText("DetailGrowth", detailPanel, "Keep looking to learn more.", 22, TextAnchor.MiddleLeft, new Vector2(0f, -176f), new Vector2(480f, 36f));
            detailGrowth.color = Color.white;

            Text detailGrowthMemory = CreateText("DetailGrowthMemory", detailPanel, "Unknown", 22, TextAnchor.MiddleLeft, new Vector2(0f, -212f), new Vector2(480f, 36f));
            detailGrowthMemory.color = Color.white;

            Text detailMoves = CreateText("DetailMoves", detailPanel, "Unknown", 22, TextAnchor.MiddleLeft, new Vector2(0f, -248f), new Vector2(480f, 36f));
            detailMoves.color = Color.white;

            Text detailFieldNote = CreateText("DetailFieldNote", detailPanel, "Keep looking in the seagrass.", 22, TextAnchor.MiddleLeft, new Vector2(0f, -284f), new Vector2(480f, 36f));
            detailFieldNote.color = Color.white;

            Text detailTimesSeen = CreateText("DetailTimesSeen", detailPanel, "Not found yet", 22, TextAnchor.MiddleLeft, new Vector2(0f, -320f), new Vector2(480f, 36f));
            detailTimesSeen.color = Color.white;

            InputField nicknameInput = CreateInputField("NicknameInput", detailPanel, "Nickname", new Vector2(-50f, -360f), new Vector2(300f, 56f));
            nicknameInput.characterLimit = 12;

            Button saveNicknameButton = CreateButton("SaveNicknameButton", detailPanel, "Save", new Vector2(160f, -360f), new Vector2(120f, 56f));

            Dropdown growthFormDropdown = CreateDropdown("GrowthFormDropdown", detailPanel, new Vector2(120f, -100f), new Vector2(260f, 56f));

            Button selectOriginalFormButton = CreateButton("SelectOriginalFormButton", detailPanel, "Original Form", new Vector2(-120f, -100f), new Vector2(160f, 56f));

            Button backButton = CreateButton("BackButton", safeArea, "Back", new Vector2(-400f, -320f), new Vector2(160f, 88f));

            JournalController controller = safeArea.gameObject.AddComponent<JournalController>();
            SpeciesDatabase database = AssetDatabase.LoadAssetAtPath<SpeciesDatabase>(SpeciesDatabasePath);

            GameObject slotPrefab = CreateSlotPrefab();

            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("speciesDatabase").objectReferenceValue = database;
            serializedController.FindProperty("slotPrefab").objectReferenceValue = slotPrefab.GetComponent<JournalSlotView>();
            serializedController.FindProperty("gridRoot").objectReferenceValue = gridRoot;
            serializedController.FindProperty("detailImage").objectReferenceValue = detailImage;
            serializedController.FindProperty("detailNameText").objectReferenceValue = detailName;
            serializedController.FindProperty("detailCurrentText").objectReferenceValue = detailCurrent;
            serializedController.FindProperty("detailCurrentIcon").objectReferenceValue = detailCurrentIcon;
            serializedController.FindProperty("detailHabitatText").objectReferenceValue = detailHabitat;
            serializedController.FindProperty("detailCaughtText").objectReferenceValue = detailCaught;
            serializedController.FindProperty("detailLevelText").objectReferenceValue = detailLevel;
            serializedController.FindProperty("detailGrowthText").objectReferenceValue = detailGrowth;
            serializedController.FindProperty("detailGrowthMemoryText").objectReferenceValue = detailGrowthMemory;
            serializedController.FindProperty("detailMovesText").objectReferenceValue = detailMoves;
            serializedController.FindProperty("detailFieldNoteText").objectReferenceValue = detailFieldNote;
            serializedController.FindProperty("detailTimesSeenText").objectReferenceValue = detailTimesSeen;
            serializedController.FindProperty("nicknameInput").objectReferenceValue = nicknameInput;
            serializedController.FindProperty("progressText").objectReferenceValue = progressText;
            serializedController.FindProperty("growthFormDropdown").objectReferenceValue = growthFormDropdown;
            serializedController.FindProperty("selectOriginalGrowthFormButton").objectReferenceValue = selectOriginalFormButton;
            serializedController.ApplyModifiedProperties();

            UnityEditor.Events.UnityEventTools.AddPersistentListener(saveNicknameButton.onClick, controller.SaveNickname);
            JournalBackButton backHandler = backButton.gameObject.AddComponent<JournalBackButton>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(backButton.onClick, backHandler.BackToOverworld);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static GameObject CreateSlotPrefab()
        {
            GameObject slotObj = new GameObject("JournalSlot");
            RectTransform slotRect = slotObj.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(88f, 88f);

            Image slotBg = slotObj.AddComponent<Image>();
            slotBg.color = new Color(0.08f, 0.22f, 0.24f, 0.80f);

            Image creatureImage = CreateImage("CreatureImage", slotObj.transform, Color.black, Vector2.zero, new Vector2(72f, 72f));
            creatureImage.preserveAspect = true;

            Text nameText = CreateText("NameText", slotObj.transform, "?", 16, TextAnchor.MiddleCenter, new Vector2(0f, -40f), new Vector2(88f, 24f));
            nameText.color = Color.white;

            Button button = slotObj.AddComponent<Button>();
            button.targetGraphic = slotBg;

            JournalSlotView slotView = slotObj.AddComponent<JournalSlotView>();
            SerializedObject serializedSlot = new SerializedObject(slotView);
            serializedSlot.FindProperty("button").objectReferenceValue = button;
            serializedSlot.FindProperty("creatureImage").objectReferenceValue = creatureImage;
            serializedSlot.FindProperty("nameText").objectReferenceValue = nameText;
            serializedSlot.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(slotObj, SlotPrefabPath);
            Object.DestroyImmediate(slotObj);
            return AssetDatabase.LoadAssetAtPath<GameObject>(SlotPrefabPath);
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1024f, 768f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPosition, Vector2 size)
        {
            Image image = CreateImage(name, parent, new Color(0.12f, 0.44f, 0.50f), anchoredPosition, size);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            Text text = CreateText("Label", image.transform, label, 28, TextAnchor.MiddleCenter, Vector2.zero, size);
            text.color = Color.white;
            return button;
        }

        private static Dropdown CreateDropdown(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
        {
            RectTransform root = CreateRect(name, parent);
            root.anchoredPosition = anchoredPosition;
            root.sizeDelta = size;

            Image bg = root.gameObject.AddComponent<Image>();
            bg.color = new Color(0.87f, 0.96f, 0.88f);

            Dropdown dropdown = root.gameObject.AddComponent<Dropdown>();
            dropdown.targetGraphic = bg;

            Text label = CreateText("Label", root, "Original form", 22, TextAnchor.MiddleLeft, new Vector2(10f, 0f), new Vector2(size.x - 40f, size.y));
            label.color = new Color(0.06f, 0.16f, 0.18f);
            label.raycastTarget = true;

            RectTransform template = CreateRect("Template", root);
            template.sizeDelta = new Vector2(size.x, 160f);
            template.anchoredPosition = new Vector2(0f, -size.y);
            Image templateBg = template.gameObject.AddComponent<Image>();
            templateBg.color = new Color(0.87f, 0.96f, 0.88f);
            template.gameObject.SetActive(false);

            return dropdown;
        }

        private static InputField CreateInputField(string name, Transform parent, string placeholder, Vector2 anchoredPosition, Vector2 size)
        {
            RectTransform root = CreateRect(name, parent);
            root.anchoredPosition = anchoredPosition;
            root.sizeDelta = size;

            Image bg = root.gameObject.AddComponent<Image>();
            bg.color = new Color(0.87f, 0.96f, 0.88f);

            Text inputText = CreateText("Text", root, string.Empty, 24, TextAnchor.MiddleLeft, new Vector2(10f, 0f), new Vector2(size.x - 20f, size.y));
            inputText.color = new Color(0.06f, 0.16f, 0.18f);
            inputText.raycastTarget = true;

            Text placeholderText = CreateText("Placeholder", root, placeholder, 24, TextAnchor.MiddleLeft, new Vector2(10f, 0f), new Vector2(size.x - 20f, size.y));
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

            InputField input = root.gameObject.AddComponent<InputField>();
            input.textComponent = inputText;
            input.placeholder = placeholderText;
            return input;
        }

        private static Image CreateImage(string name, Transform parent, Color color, Vector2 anchoredPosition, Vector2 size)
        {
            RectTransform rectTransform = CreateRect(name, parent);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;
            Image image = rectTransform.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(string name, Transform parent, string value, int fontSize, TextAnchor alignment, Vector2 anchoredPosition, Vector2 size)
        {
            RectTransform rectTransform = CreateRect(name, parent);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Text text = rectTransform.gameObject.AddComponent<Text>();
            text.text = value;
            text.font = GetBuiltinFont();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            return rectTransform;
        }

        private static Font GetBuiltinFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string name = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
