using System.IO;
using Tidepool.Runtime;
using Tidepool.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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

            // Shell-shaped progress bar: a Stone Gray track with a Soft Kelp fill
            // (Image.Type.Filled) that JournalController animates on population.
            RectTransform progressBarRoot = CreateRect("ProgressBar", safeArea);
            progressBarRoot.anchoredPosition = new Vector2(0f, 270f);
            progressBarRoot.sizeDelta = new Vector2(420f, 36f);

            Image progressBarTrack = progressBarRoot.gameObject.AddComponent<Image>();
            progressBarTrack.color = new Color(0.435f, 0.459f, 0.431f, 0.30f);

            Image progressBarFill = CreateImage("ProgressBarFill", progressBarRoot, new Color(0.243f, 0.435f, 0.353f), Vector2.zero, new Vector2(420f, 36f));
            // Image.Type.Filled only clips a mesh when a sprite is assigned — without
            // one it silently falls back to drawing the full rect regardless of
            // fillAmount, so give it a plain solid-white sprite to fill against.
            progressBarFill.sprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
                new Vector2(0.5f, 0.5f));
            progressBarFill.type = Image.Type.Filled;
            progressBarFill.fillMethod = Image.FillMethod.Horizontal;
            progressBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            progressBarFill.fillAmount = 0f;

            Text progressText = CreateText("ProgressText", progressBarRoot, "0 of 13 found", 22, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(420f, 36f));
            progressText.color = new Color(0.08f, 0.18f, 0.22f);

            // Sort row (by Name/Zone/Current/Rarity) and filter row (by zone, plus
            // "All") sit above the grid. Space here is too tight for literal 88pt
            // square icon buttons alongside the title/progress bar/detail panel
            // without a broader layout pass (tracked separately as #125); these use
            // this codebase's existing ~40-56pt secondary-button sizing instead.
            Button sortByName = CreateSmallButton("SortByNameButton", safeArea, "Name", new Vector2(-494f, 210f), new Vector2(70f, 40f));
            Button sortByZone = CreateSmallButton("SortByZoneButton", safeArea, "Zone", new Vector2(-418f, 210f), new Vector2(70f, 40f));
            Button sortByCurrent = CreateSmallButton("SortByCurrentButton", safeArea, "Current", new Vector2(-342f, 210f), new Vector2(70f, 40f));
            Button sortByRarity = CreateSmallButton("SortByRarityButton", safeArea, "Rarity", new Vector2(-266f, 210f), new Vector2(70f, 40f));

            Button filterAll = CreateSmallButton("FilterAllButton", safeArea, "All", new Vector2(-500f, 160f), new Vector2(56f, 40f));
            Button filterShallows = CreateSmallButton("FilterShallowsButton", safeArea, "Shallow", new Vector2(-444f, 160f), new Vector2(56f, 40f));
            Button filterMeadow = CreateSmallButton("FilterMeadowButton", safeArea, "Meadow", new Vector2(-388f, 160f), new Vector2(56f, 40f));
            Button filterKelp = CreateSmallButton("FilterKelpButton", safeArea, "Kelp", new Vector2(-332f, 160f), new Vector2(56f, 40f));
            Button filterRocky = CreateSmallButton("FilterRockyButton", safeArea, "Rocky", new Vector2(-276f, 160f), new Vector2(56f, 40f));

            RectTransform gridPanel = CreateRect("GridPanel", safeArea);
            gridPanel.anchoredPosition = new Vector2(-380f, -40f);
            gridPanel.sizeDelta = new Vector2(300f, 320f);

            ScrollRect gridScroll = gridPanel.gameObject.AddComponent<ScrollRect>();
            gridScroll.horizontal = false;
            gridScroll.vertical = true;
            gridScroll.movementType = ScrollRect.MovementType.Clamped;

            RectTransform gridViewport = CreateRect("GridViewport", gridPanel);
            gridViewport.anchorMin = Vector2.zero;
            gridViewport.anchorMax = Vector2.one;
            gridViewport.offsetMin = Vector2.zero;
            gridViewport.offsetMax = Vector2.zero;
            gridViewport.gameObject.AddComponent<RectMask2D>();

            RectTransform gridRoot = CreateRect("GridRoot", gridViewport);
            gridRoot.anchorMin = new Vector2(0f, 1f);
            gridRoot.anchorMax = new Vector2(1f, 1f);
            gridRoot.pivot = new Vector2(0.5f, 1f);
            gridRoot.anchoredPosition = Vector2.zero;
            gridRoot.sizeDelta = new Vector2(0f, 1000f);
            gridScroll.viewport = gridViewport;
            gridScroll.content = gridRoot;

            GridLayoutGroup gridLayout = gridRoot.gameObject.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(88f, 88f);
            gridLayout.spacing = new Vector2(8f, 8f);
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperLeft;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3;

            RectTransform detailPanel = CreateRect("DetailPanel", safeArea);
            detailPanel.anchoredPosition = new Vector2(120f, 80f);
            detailPanel.sizeDelta = new Vector2(500f, 320f);
            Image detailPanelBg = detailPanel.gameObject.AddComponent<Image>();
            detailPanelBg.color = new Color(1f, 0.97f, 0.89f, 0.96f);

            ScrollRect detailScroll = detailPanel.gameObject.AddComponent<ScrollRect>();
            detailScroll.horizontal = false;
            detailScroll.vertical = true;
            detailScroll.movementType = ScrollRect.MovementType.Clamped;

            RectTransform detailViewport = CreateRect("DetailViewport", detailPanel);
            detailViewport.anchorMin = Vector2.zero;
            detailViewport.anchorMax = Vector2.one;
            detailViewport.offsetMin = new Vector2(20f, 20f);
            detailViewport.offsetMax = new Vector2(-20f, -20f);
            detailViewport.gameObject.AddComponent<RectMask2D>();

            RectTransform detailContent = CreateRect("DetailContent", detailViewport);
            detailContent.anchorMin = new Vector2(0f, 1f);
            detailContent.anchorMax = new Vector2(1f, 1f);
            detailContent.pivot = new Vector2(0.5f, 1f);
            detailContent.anchoredPosition = Vector2.zero;
            detailContent.sizeDelta = new Vector2(0f, 1040f);
            detailScroll.viewport = detailViewport;
            detailScroll.content = detailContent;

            CreateSectionHeader("IdentityHeader", detailContent, "Identity", new Vector2(0f, 484f));
            Image detailImage = CreateImage("DetailImage", detailContent, Color.white, new Vector2(0f, 388f), new Vector2(150f, 150f));
            detailImage.preserveAspect = true;

            Text detailName = CreateText("DetailName", detailContent, "?", 30, TextAnchor.MiddleCenter, new Vector2(0f, 300f), new Vector2(420f, 48f));
            detailName.color = new Color(0.18f, 0.23f, 0.21f);

            Text detailCurrent = CreateText("DetailCurrent", detailContent, "Unknown", 24, TextAnchor.MiddleLeft, new Vector2(46f, 256f), new Vector2(300f, 40f));
            detailCurrent.color = new Color(0.18f, 0.23f, 0.21f);

            Image detailCurrentIcon = CreateImage("DetailCurrentIcon", detailContent, Color.white, new Vector2(-140f, 256f), new Vector2(40f, 40f));
            detailCurrentIcon.preserveAspect = true;

            InputField nicknameInput = CreateInputField("NicknameInput", detailContent, "Nickname", new Vector2(-58f, 200f), new Vector2(292f, 56f));
            nicknameInput.characterLimit = 12;

            Button saveNicknameButton = CreateButton("SaveNicknameButton", detailContent, "Save", new Vector2(150f, 200f), new Vector2(112f, 56f));

            Dropdown growthFormDropdown = CreateDropdown("GrowthFormDropdown", detailContent, new Vector2(86f, 132f), new Vector2(260f, 56f));

            Button selectOriginalFormButton = CreateButton("SelectOriginalFormButton", detailContent, "Original Form", new Vector2(-142f, 132f), new Vector2(160f, 56f));
            selectOriginalFormButton.GetComponentInChildren<Text>().fontSize = 20;

            CreateDivider("IdentityDivider", detailContent, new Vector2(0f, 84f));

            CreateSectionHeader("HabitatHeader", detailContent, "Habitat", new Vector2(0f, 46f));

            Text detailHabitat = CreateText("DetailHabitat", detailContent, "Unknown", 22, TextAnchor.MiddleLeft, new Vector2(0f, 4f), new Vector2(420f, 36f));
            detailHabitat.color = new Color(0.18f, 0.23f, 0.21f);

            Text detailCaught = CreateText("DetailCaught", detailContent, "Not found yet", 22, TextAnchor.MiddleLeft, new Vector2(0f, -32f), new Vector2(420f, 36f));
            detailCaught.color = new Color(0.18f, 0.23f, 0.21f);

            CreateDivider("HabitatDivider", detailContent, new Vector2(0f, -70f));

            CreateSectionHeader("StatsHeader", detailContent, "Stats", new Vector2(0f, -108f));

            // Level/Growth/GrowthMemory/Moves rows use a 52px-tall box (not the usual 36px)
            // because their text can wrap to two lines (growth-memory and move-unlock
            // sentences run long) and Unity's Text truncates vertically instead of growing.
            Text detailLevel = CreateText("DetailLevel", detailContent, "Unknown", 22, TextAnchor.MiddleLeft, new Vector2(0f, -156f), new Vector2(420f, 52f));
            detailLevel.color = new Color(0.18f, 0.23f, 0.21f);

            Text detailGrowth = CreateText("DetailGrowth", detailContent, "Keep looking to learn more.", 22, TextAnchor.MiddleLeft, new Vector2(0f, -212f), new Vector2(420f, 52f));
            detailGrowth.color = new Color(0.18f, 0.23f, 0.21f);

            Text detailGrowthMemory = CreateText("DetailGrowthMemory", detailContent, "Unknown", 22, TextAnchor.MiddleLeft, new Vector2(0f, -268f), new Vector2(420f, 52f));
            detailGrowthMemory.color = new Color(0.18f, 0.23f, 0.21f);

            Text detailMoves = CreateText("DetailMoves", detailContent, "Unknown", 22, TextAnchor.MiddleLeft, new Vector2(0f, -324f), new Vector2(420f, 52f));
            detailMoves.color = new Color(0.18f, 0.23f, 0.21f);

            Text detailTimesSeen = CreateText("DetailTimesSeen", detailContent, "Not found yet", 22, TextAnchor.MiddleLeft, new Vector2(0f, -372f), new Vector2(420f, 36f));
            detailTimesSeen.color = new Color(0.18f, 0.23f, 0.21f);

            CreateDivider("StatsDivider", detailContent, new Vector2(0f, -410f));

            CreateSectionHeader("FieldNotesHeader", detailContent, "Field Notes", new Vector2(0f, -448f));

            Text detailFieldNote = CreateText("DetailFieldNote", detailContent, "Keep looking in the seagrass.", 22, TextAnchor.UpperLeft, new Vector2(0f, -516f), new Vector2(420f, 92f));
            detailFieldNote.color = new Color(0.18f, 0.23f, 0.21f);

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
            serializedController.FindProperty("progressBarFill").objectReferenceValue = progressBarFill;
            serializedController.FindProperty("growthFormDropdown").objectReferenceValue = growthFormDropdown;
            serializedController.FindProperty("selectOriginalGrowthFormButton").objectReferenceValue = selectOriginalFormButton;
            serializedController.ApplyModifiedProperties();

            UnityEditor.Events.UnityEventTools.AddPersistentListener(saveNicknameButton.onClick, controller.SaveNickname);
            JournalBackButton backHandler = backButton.gameObject.AddComponent<JournalBackButton>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(backButton.onClick, backHandler.BackToOverworld);

            UnityEditor.Events.UnityEventTools.AddPersistentListener(sortByName.onClick, controller.SortByName);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(sortByZone.onClick, controller.SortByZone);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(sortByCurrent.onClick, controller.SortByCurrent);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(sortByRarity.onClick, controller.SortByRarity);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(filterAll.onClick, controller.FilterAllZones);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(filterShallows.onClick, controller.FilterShallows);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(filterMeadow.onClick, controller.FilterMeadow);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(filterKelp.onClick, controller.FilterKelp);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(filterRocky.onClick, controller.FilterRocky);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static GameObject CreateSlotPrefab()
        {
            GameObject slotObj = new GameObject("JournalSlot");
            RectTransform slotRect = slotObj.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(88f, 88f);

            // slotBg is the 4px rarity-trim border (colored per-slot at runtime by
            // JournalSlotView.Bind); CardInterior is the fixed Shell Panel card face on
            // top of it. Root stays 88x88 to keep the 88pt touch target.
            Image slotBg = slotObj.AddComponent<Image>();
            slotBg.color = new Color(0.44f, 0.46f, 0.43f);

            Image cardInterior = CreateImage("CardInterior", slotObj.transform, new Color(1f, 0.969f, 0.894f, 0.96f), Vector2.zero, new Vector2(80f, 80f));

            Image creatureImage = CreateImage("CreatureImage", slotObj.transform, Color.black, new Vector2(0f, 8f), new Vector2(60f, 60f));
            creatureImage.preserveAspect = true;

            Text nameText = CreateText("NameText", slotObj.transform, "?", 16, TextAnchor.MiddleCenter, new Vector2(0f, -32f), new Vector2(84f, 20f));
            nameText.color = new Color(0.18f, 0.23f, 0.21f);
            nameText.horizontalOverflow = HorizontalWrapMode.Overflow;
            nameText.resizeTextForBestFit = true;
            nameText.resizeTextMinSize = 10;
            nameText.resizeTextMaxSize = 16;

            Button button = slotObj.AddComponent<Button>();
            button.targetGraphic = slotBg;

            JournalSlotView slotView = slotObj.AddComponent<JournalSlotView>();
            SerializedObject serializedSlot = new SerializedObject(slotView);
            serializedSlot.FindProperty("button").objectReferenceValue = button;
            serializedSlot.FindProperty("creatureImage").objectReferenceValue = creatureImage;
            serializedSlot.FindProperty("nameText").objectReferenceValue = nameText;
            serializedSlot.FindProperty("borderImage").objectReferenceValue = slotBg;
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

        private static Text CreateSectionHeader(string name, Transform parent, string label, Vector2 anchoredPosition)
        {
            Text header = CreateText(name, parent, label, 28, TextAnchor.MiddleLeft, anchoredPosition, new Vector2(420f, 40f));
            header.color = new Color(0.24f, 0.44f, 0.35f);
            header.fontStyle = FontStyle.Bold;
            return header;
        }

        private static Image CreateDivider(string name, Transform parent, Vector2 anchoredPosition)
        {
            return CreateImage(name, parent, new Color(0.85f, 0.68f, 0.38f, 0.55f), anchoredPosition, new Vector2(420f, 2f));
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

        private static Button CreateSmallButton(string name, Transform parent, string label, Vector2 anchoredPosition, Vector2 size)
        {
            Image image = CreateImage(name, parent, new Color(0.12f, 0.44f, 0.50f), anchoredPosition, size);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            Text text = CreateText("Label", image.transform, label, 14, TextAnchor.MiddleCenter, Vector2.zero, size);
            text.color = Color.white;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 9;
            text.resizeTextMaxSize = 14;
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
