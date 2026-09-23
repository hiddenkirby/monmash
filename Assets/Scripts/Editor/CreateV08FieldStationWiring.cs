using System.IO;
using Tidepool.Domain;
using Tidepool.Runtime;
using Tidepool.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Tidepool.Editor
{
    // Wires the v0.8 field station into the existing Overworld scene:
    // a bounded world-side decoration/visitor area plus an always-available UI card whose
    // four route buttons hand off to the authoritative Journal, Coast Atlas, Goals, and
    // Settings systems. Idempotent: re-running removes and rebuilds only station objects.
    // See docs/V0_8_FIELD_STATION.md. UI helper methods mirror CreateV01OverworldScene
    // (its helpers are private; keep the copies visually consistent with it).
    public static class CreateV08FieldStationWiring
    {
        private const string ScenePath = "Assets/Scenes/Overworld.unity";
        private const string FieldStationDefinitionPath = "Assets/Data/FieldStation/GreatLowTideFieldStation.asset";
        private const string CoastAtlasDefinitionPath = "Assets/Data/CoastAtlas/GreatLowTideCoastAtlas.asset";
        private const int StationCellX = -8;
        private const int StationCellY = 7;

        [MenuItem("Tools/Tidepool/Wire v0.8 Field Station Into Overworld")]
        public static void WireFieldStationIntoOverworld()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            CreateFieldStationDefinition.CreateDefinition();
            FieldStationDefinition definition = AssetDatabase.LoadAssetAtPath<FieldStationDefinition>(FieldStationDefinitionPath);
            if (definition == null)
            {
                Debug.LogError("Field station definition missing after generation.");
                return;
            }

            CreateCoastAtlasDefinition.CreateDefinition();

            GameObject canvasObject = GameObject.Find("Canvas");
            Transform safeArea = canvasObject != null ? canvasObject.transform.Find("SafeArea") : null;
            if (safeArea == null)
            {
                Debug.LogError("Overworld Canvas/SafeArea not found. Run Tools/Tidepool/Create v0.1 Overworld Scene first.");
                return;
            }

            GameObject playerObject = GameObject.Find("Player");
            PlayerGridMover playerMover = playerObject != null ? playerObject.GetComponent<PlayerGridMover>() : null;
            if (playerMover == null)
            {
                Debug.LogError("Overworld Player with PlayerGridMover not found.");
                return;
            }

            RemoveExistingStation(safeArea);
            RemoveExistingPanels(safeArea);

            // UI first: the controller needs title/description references before it is wired.
            FieldStationCard card = CreateFieldStationCard(safeArea);
            (CoastAtlasController atlasController, Button atlasCloseButton) = CreateCoastAtlasPanel(safeArea);
            (SettingsController settingsController, Button settingsCloseButton) = CreateSettingsPanel(safeArea);

            Grid grid = Object.FindAnyObjectByType<Grid>();
            Vector3 stationOrigin = grid != null
                ? grid.GetCellCenterWorld(new Vector3Int(StationCellX, StationCellY, 0))
                : new Vector3(StationCellX, StationCellY + 0.5f, 0f);

            GameObject stationRoot = new GameObject("FieldStation");
            stationRoot.transform.position = stationOrigin;
            FieldStationController controller = stationRoot.AddComponent<FieldStationController>();

            FieldStationDecorationBinding[] decorationBindings = CreateDecorationRoots(stationRoot.transform);
            FieldStationVisitorBinding[] visitorBindings = CreateVisitorRoots(stationRoot.transform);

            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("definition").objectReferenceValue = definition;
            serializedController.FindProperty("titleText").objectReferenceValue = card.TitleText;
            serializedController.FindProperty("descriptionText").objectReferenceValue = card.DescriptionText;
            WireBindingArray(serializedController.FindProperty("decorations"), decorationBindings,
                (property, binding) =>
                {
                    property.FindPropertyRelative("upgradeId").stringValue = binding.UpgradeId;
                    property.FindPropertyRelative("standardRoot").objectReferenceValue = binding.StandardRoot;
                    property.FindPropertyRelative("reducedMotionRoot").objectReferenceValue = binding.ReducedMotionRoot;
                });
            WireBindingArray(serializedController.FindProperty("visitors"), visitorBindings,
                (property, binding) =>
                {
                    property.FindPropertyRelative("speciesId").stringValue = binding.SpeciesId;
                    property.FindPropertyRelative("visitorRoot").objectReferenceValue = binding.VisitorRoot;
                });
            serializedController.ApplyModifiedProperties();

            // The four route events hand off to the existing authoritative systems.
            JournalTrigger journalTrigger = playerObject.GetComponent<JournalTrigger>();
            if (journalTrigger == null)
            {
                journalTrigger = playerObject.AddComponent<JournalTrigger>();
                SerializedObject serializedTrigger = new SerializedObject(journalTrigger);
                serializedTrigger.FindProperty("playerMover").objectReferenceValue = playerMover;
                serializedTrigger.FindProperty("journalSceneName").stringValue = "Journal";
                serializedTrigger.ApplyModifiedProperties();
            }

            CharacterSelectTrigger characterTrigger = playerObject.GetComponent<CharacterSelectTrigger>();
            if (characterTrigger == null)
            {
                characterTrigger = playerObject.AddComponent<CharacterSelectTrigger>();
                SerializedObject serializedTrigger = new SerializedObject(characterTrigger);
                serializedTrigger.FindProperty("playerMover").objectReferenceValue = playerMover;
                serializedTrigger.FindProperty("characterSelectSceneName").stringValue = "CharacterSelect";
                serializedTrigger.ApplyModifiedProperties();
            }

            UnityEditor.Events.UnityEventTools.AddPersistentListener(controller.openJournal, journalTrigger.OpenJournal);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(controller.openAtlas, atlasController.OpenAtlas);
            if (card.GoalsPanel != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(controller.openGoals, card.GoalsPanel.OpenGoals);
            }

            UnityEditor.Events.UnityEventTools.AddPersistentListener(controller.openSettings, settingsController.OpenPanel);
            EditorUtility.SetDirty(controller);

            // Persistent listeners so the wiring survives scene save/reload.
            UnityEditor.Events.UnityEventTools.AddPersistentListener(card.JournalButton.onClick, controller.OpenJournal);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(card.AtlasButton.onClick, controller.OpenAtlas);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(card.GoalsButton.onClick, controller.OpenGoals);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(card.SettingsButton.onClick, controller.OpenSettings);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(atlasCloseButton.onClick, atlasController.CloseAtlas);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(settingsCloseButton.onClick, settingsController.ClosePanel);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Field station wired into Overworld: decorations, visitors, UI card, and four routes.");
        }

        private static void RemoveExistingStation(Transform safeArea)
        {
            GameObject previousRoot = GameObject.Find("FieldStation");
            if (previousRoot != null)
            {
                Object.DestroyImmediate(previousRoot);
            }

            Transform previousCard = safeArea.Find("FieldStationCard");
            if (previousCard != null)
            {
                Object.DestroyImmediate(previousCard.gameObject);
            }
        }

        private static void RemoveExistingPanels(Transform safeArea)
        {
            Transform previousAtlasPanel = safeArea.Find("CoastAtlasPanel");
            if (previousAtlasPanel != null)
            {
                Object.DestroyImmediate(previousAtlasPanel.gameObject);
            }

            Transform previousSettingsPanel = safeArea.Find("SettingsPanel");
            if (previousSettingsPanel != null)
            {
                Object.DestroyImmediate(previousSettingsPanel.gameObject);
            }

            CoastAtlasController previousAtlasController = safeArea.GetComponent<CoastAtlasController>();
            if (previousAtlasController != null)
            {
                Object.DestroyImmediate(previousAtlasController);
            }

            SettingsController previousSettingsController = safeArea.GetComponent<SettingsController>();
            if (previousSettingsController != null)
            {
                Object.DestroyImmediate(previousSettingsController);
            }
        }

        private static FieldStationCard CreateFieldStationCard(Transform safeArea)
        {
            // Sits between the story-beat dialogue strip and the bottom route-button row.
            Image panel = CreateImage("FieldStationCard", safeArea, new Color(0.92f, 0.97f, 0.91f, 0.96f), new Vector2(0f, -180f), new Vector2(1024f, 104f));
            ApplyRoundedPanelStyle(panel);

            Text titleText = CreateText("StationTitle", panel.transform, "Field Station", 28, TextAnchor.MiddleLeft, new Vector2(-380f, 14f), new Vector2(360f, 40f));
            titleText.color = new Color(0.06f, 0.16f, 0.18f);

            Text descriptionText = CreateText("StationDescription", panel.transform, "A quiet place for everything the coast remembers.", 20, TextAnchor.MiddleLeft, new Vector2(-330f, -22f), new Vector2(620f, 34f));
            descriptionText.color = new Color(0.24f, 0.34f, 0.32f);

            Button journalButton = CreateButton("StationJournalButton", panel.transform, "Journal", new Vector2(-168f, 0f), new Vector2(104f, 88f));
            Button atlasButton = CreateButton("StationAtlasButton", panel.transform, "Atlas", new Vector2(-56f, 0f), new Vector2(104f, 88f));
            Button goalsButton = CreateButton("StationGoalsButton", panel.transform, "Goals", new Vector2(56f, 0f), new Vector2(104f, 88f));
            Button settingsButton = CreateButton("StationSettingsButton", panel.transform, "Settings", new Vector2(168f, 0f), new Vector2(104f, 88f));

            GoalsPanelController goalsPanel = safeArea.GetComponent<GoalsPanelController>();
            if (goalsPanel == null)
            {
                Debug.LogError("GoalsPanelController missing from Canvas/SafeArea; the station Goals route will not work.");
            }

            return new FieldStationCard(titleText, descriptionText, journalButton, atlasButton, goalsButton, settingsButton, goalsPanel);
        }

        private static (CoastAtlasController controller, Button closeButton) CreateCoastAtlasPanel(Transform safeArea)
        {
            CoastAtlasDefinition atlasDefinition = AssetDatabase.LoadAssetAtPath<CoastAtlasDefinition>(CoastAtlasDefinitionPath);

            CoastAtlasController controller = safeArea.gameObject.AddComponent<CoastAtlasController>();
            Image panel = CreateImage("CoastAtlasPanel", safeArea, new Color(0.10f, 0.24f, 0.28f, 0.96f), new Vector2(0f, 148f), new Vector2(640f, 480f));
            ApplyRoundedPanelStyle(panel);

            Text title = CreateText("AtlasTitle", panel.transform, "The Great Low Tide", 32, TextAnchor.MiddleLeft, new Vector2(-240f, 196f), new Vector2(360f, 48f));
            title.color = Color.white;

            Text currentPlace = CreateText("AtlasCurrentPlace", panel.transform, string.Empty, 22, TextAnchor.MiddleLeft, new Vector2(30f, 196f), new Vector2(280f, 44f));
            currentPlace.color = new Color(0.86f, 0.96f, 0.92f);

            Text objective = CreateText("AtlasObjective", panel.transform, string.Empty, 22, TextAnchor.UpperLeft, new Vector2(0f, 140f), new Vector2(560f, 72f));
            objective.color = Color.white;

            Text progress = CreateText("AtlasProgress", panel.transform, string.Empty, 20, TextAnchor.MiddleLeft, new Vector2(0f, 92f), new Vector2(560f, 36f));
            progress.color = new Color(0.86f, 0.96f, 0.92f);

            Image mapImage = CreateImage("AtlasMap", panel.transform, new Color(0.94f, 0.96f, 0.92f, 0.35f), new Vector2(0f, -52f), new Vector2(576f, 224f));

            Text[] nodeLabels = new Text[4];
            Image[] nodeMarkers = new Image[4];
            for (int i = 0; i < 4; i++)
            {
                Text label = CreateText($"AtlasNodeLabel{i + 1}", panel.transform, string.Empty, 22, TextAnchor.MiddleLeft, new Vector2(-180f, -168f + i * 46f), new Vector2(420f, 40f));
                label.color = Color.white;
                nodeLabels[i] = label;

                Image marker = CreateImage($"AtlasNodeMarker{i + 1}", panel.transform, Color.white, Vector2.zero, new Vector2(28f, 28f));
                marker.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                marker.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                marker.gameObject.SetActive(false);
                nodeMarkers[i] = marker;
            }

            Button closeButton = CreateButton("CloseAtlasButton", panel.transform, "OK", new Vector2(244f, 196f), new Vector2(112f, 88f));

            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("panelRoot").objectReferenceValue = panel.gameObject;
            serializedController.FindProperty("atlasDefinition").objectReferenceValue = atlasDefinition;
            serializedController.FindProperty("mapImage").objectReferenceValue = mapImage;
            serializedController.FindProperty("titleText").objectReferenceValue = title;
            serializedController.FindProperty("currentPlaceText").objectReferenceValue = currentPlace;
            serializedController.FindProperty("objectiveText").objectReferenceValue = objective;
            serializedController.FindProperty("progressText").objectReferenceValue = progress;
            WireTextArray(serializedController.FindProperty("nodeLabels"), nodeLabels);
            WireImageArray(serializedController.FindProperty("nodeMarkers"), nodeMarkers);
            serializedController.FindProperty("closeButton").objectReferenceValue = closeButton;
            serializedController.ApplyModifiedProperties();

            panel.gameObject.SetActive(false);
            return (controller, closeButton);
        }

        private static (SettingsController controller, Button closeButton) CreateSettingsPanel(Transform safeArea)
        {
            SettingsController controller = safeArea.gameObject.AddComponent<SettingsController>();
            Image panel = CreateImage("SettingsPanel", safeArea, new Color(0.10f, 0.24f, 0.28f, 0.96f), new Vector2(0f, 148f), new Vector2(640f, 420f));
            ApplyRoundedPanelStyle(panel);

            Text title = CreateText("SettingsTitle", panel.transform, "Settings", 32, TextAnchor.MiddleLeft, new Vector2(-180f, 152f), new Vector2(300f, 48f));
            title.color = Color.white;

            Button closeButton = CreateButton("CloseSettingsButton", panel.transform, "OK", new Vector2(244f, 152f), new Vector2(112f, 88f));

            Toggle muteToggle = CreateToggle("MuteToggle", panel.transform, "Mute", new Vector2(0f, 60f), new Vector2(300f, 88f));
            Slider volumeSlider = CreateSlider("VolumeSlider", panel.transform, new Vector2(0f, -48f), new Vector2(440f, 40f));

            Text volumeValue = CreateText("VolumeValue", panel.transform, string.Empty, 24, TextAnchor.MiddleCenter, new Vector2(0f, -128f), new Vector2(240f, 44f));
            volumeValue.color = Color.white;

            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("panelRoot").objectReferenceValue = panel.gameObject;
            serializedController.FindProperty("muteToggle").objectReferenceValue = muteToggle;
            serializedController.FindProperty("volumeSlider").objectReferenceValue = volumeSlider;
            serializedController.FindProperty("volumeValueText").objectReferenceValue = volumeValue;
            serializedController.ApplyModifiedProperties();

            panel.gameObject.SetActive(false);
            return (controller, closeButton);
        }

        private static FieldStationDecorationBinding[] CreateDecorationRoots(Transform parent)
        {
            string[] upgradeIds =
            {
                "station.shallows",
                "station.species-board",
                "station.meadow",
                "station.kelp",
                "station.growth-memory",
                "station.festival-ribbon",
                "station.rocky",
                "station.finale"
            };

            string[] spritePaths =
            {
                "Assets/Art/Tiles/KenneyRpgBase/sand_plain.png",
                "Assets/Art/Tiles/KenneyRpgBase/grass_tufts.png",
                "Assets/Art/Tiles/KenneyRpgBase/shrub_green.png",
                "Assets/Art/Tiles/KenneyRpgBase/kelp_tall.png",
                "Assets/Art/Tiles/KenneyRpgBase/crate_large.png",
                "Assets/Art/Tiles/KenneyRpgBase/grass_tufts.png",
                "Assets/Art/Tiles/KenneyRpgBase/rock_mossy.png",
                "Assets/Art/Tiles/KenneyRpgBase/sand_plain.png"
            };

            Color[] standardColors =
            {
                new Color(0.98f, 0.92f, 0.74f, 1f),
                new Color(0.55f, 0.86f, 0.58f, 1f),
                new Color(0.42f, 0.78f, 0.50f, 1f),
                new Color(0.30f, 0.62f, 0.46f, 1f),
                new Color(0.76f, 0.64f, 0.44f, 1f),
                new Color(0.96f, 0.72f, 0.55f, 1f),
                new Color(0.62f, 0.64f, 0.62f, 1f),
                new Color(0.99f, 0.87f, 0.50f, 1f)
            };

            Color[] reducedMotionColors =
            {
                new Color(0.98f, 0.92f, 0.74f, 0.85f),
                new Color(0.55f, 0.86f, 0.58f, 0.85f),
                new Color(0.42f, 0.78f, 0.50f, 0.85f),
                new Color(0.30f, 0.62f, 0.46f, 0.85f),
                new Color(0.76f, 0.64f, 0.44f, 0.85f),
                new Color(0.96f, 0.72f, 0.55f, 0.85f),
                new Color(0.62f, 0.64f, 0.62f, 0.85f),
                new Color(0.99f, 0.87f, 0.50f, 0.85f)
            };

            // Two calm columns beside the walkable route; no colliders anywhere.
            Vector3[] offsets =
            {
                new Vector3(0.45f, 2.4f, 0f),
                new Vector3(0.45f, 1.6f, 0f),
                new Vector3(0.45f, 0.8f, 0f),
                new Vector3(0.45f, 0.0f, 0f),
                new Vector3(-0.45f, 2.0f, 0f),
                new Vector3(-0.45f, 1.2f, 0f),
                new Vector3(-0.45f, 0.4f, 0f),
                new Vector3(-0.45f, -0.4f, 0f)
            };

            FieldStationDecorationBinding[] bindings = new FieldStationDecorationBinding[upgradeIds.Length];
            for (int i = 0; i < upgradeIds.Length; i++)
            {
                GameObject standardRoot = new GameObject($"{upgradeIds[i]}_Standard");
                standardRoot.transform.SetParent(parent, false);
                standardRoot.transform.localPosition = offsets[i];
                CreateSpriteChild("StandardVisual", standardRoot.transform, spritePaths[i], standardColors[i], 0.62f);

                GameObject reducedMotionRoot = new GameObject($"{upgradeIds[i]}_ReducedMotion");
                reducedMotionRoot.transform.SetParent(parent, false);
                reducedMotionRoot.transform.localPosition = offsets[i];
                CreateSpriteChild("ReducedMotionVisual", reducedMotionRoot.transform, spritePaths[i], reducedMotionColors[i], 0.62f);

                standardRoot.SetActive(false);
                reducedMotionRoot.SetActive(false);

                bindings[i] = new FieldStationDecorationBinding(upgradeIds[i], standardRoot, reducedMotionRoot);
            }

            return bindings;
        }

        private static FieldStationVisitorBinding[] CreateVisitorRoots(Transform parent)
        {
            string[] speciesIds = { "blip", "nubbin", "frillick", "sputter" };
            string[] spritePaths =
            {
                "Assets/Art/Creatures/blip.png",
                "Assets/Art/Creatures/nubbin.png",
                "Assets/Art/Creatures/frillick.png",
                "Assets/Art/Creatures/sputter.png"
            };

            Vector3[] offsets =
            {
                new Vector3(1.6f, 0.4f, 0f),
                new Vector3(2.2f, -0.3f, 0f),
                new Vector3(1.4f, -1.0f, 0f),
                new Vector3(2.4f, 0.6f, 0f)
            };

            FieldStationVisitorBinding[] bindings = new FieldStationVisitorBinding[speciesIds.Length];
            for (int i = 0; i < speciesIds.Length; i++)
            {
                GameObject visitorRoot = new GameObject($"Visitor_{speciesIds[i]}");
                visitorRoot.transform.SetParent(parent, false);
                visitorRoot.transform.localPosition = offsets[i];
                // Creature art is ~512px (≈5.1 units at 100 PPU); scale it down to
                // roughly player size so visitors stay companion-scaled.
                CreateSpriteChild("VisitorVisual", visitorRoot.transform, spritePaths[i], Color.white, 0.22f);
                visitorRoot.SetActive(false);
                bindings[i] = new FieldStationVisitorBinding(speciesIds[i], visitorRoot);
            }

            return bindings;
        }

        // Loads the sprite without touching its importer settings: overriding
        // pixels-per-unit on shared creature art would rescale it everywhere.
        private static SpriteRenderer CreateSpriteChild(string name, Transform parent, string spritePath, Color color, float scale)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadSprite(spritePath, 0f);
            renderer.color = color;
            renderer.sortingOrder = 4;
            return renderer;
        }

        private static Toggle CreateToggle(string name, Transform parent, string label, Vector2 anchoredPosition, Vector2 size)
        {
            Image background = CreateImage(name, parent, new Color(0.92f, 0.97f, 0.91f), anchoredPosition, size);
            ApplyRoundedButtonStyle(background);
            RectTransform backgroundRect = background.rectTransform;
            backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
            backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);

            Image checkmark = CreateImage("Checkmark", background.transform, new Color(0.10f, 0.34f, 0.30f, 0.95f), Vector2.zero, new Vector2(44f, 44f));
            checkmark.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            checkmark.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            checkmark.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            checkmark.rectTransform.anchoredPosition = new Vector2(38f, 0f);

            Text text = CreateText("Label", background.transform, label, 26, TextAnchor.MiddleLeft, new Vector2(28f, 0f), new Vector2(220f, 48f));
            text.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            text.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            text.color = new Color(0.06f, 0.16f, 0.18f);

            Toggle toggle = background.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = background;
            toggle.graphic = checkmark;
            return toggle;
        }

        private static Slider CreateSlider(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
        {
            Image background = CreateImage(name, parent, new Color(0.92f, 0.97f, 0.91f, 0.35f), anchoredPosition, size);
            RectTransform backgroundRect = background.rectTransform;
            backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
            backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);

            Image fill = CreateImage("Fill", background.transform, new Color(0.12f, 0.36f, 0.42f), Vector2.zero, size);
            RectTransform fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image handle = CreateImage("Handle", background.transform, Color.white, Vector2.zero, new Vector2(36f, size.y + 16f));
            RectTransform handleRect = handle.rectTransform;
            handleRect.anchorMin = new Vector2(0f, 0.5f);
            handleRect.anchorMax = new Vector2(0f, 0.5f);

            Slider slider = background.gameObject.AddComponent<Slider>();
            slider.targetGraphic = handle;
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            return slider;
        }

        private static void WireTextArray(SerializedProperty property, Text[] texts)
        {
            property.arraySize = texts.Length;
            for (int i = 0; i < texts.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = texts[i];
            }
        }

        private static void WireImageArray(SerializedProperty property, Image[] images)
        {
            property.arraySize = images.Length;
            for (int i = 0; i < images.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = images[i];
            }
        }

        private static void WireBindingArray(SerializedProperty property, FieldStationDecorationBinding[] bindings,
            System.Action<SerializedProperty, FieldStationDecorationBinding> apply)
        {
            property.arraySize = bindings.Length;
            for (int i = 0; i < bindings.Length; i++)
            {
                apply(property.GetArrayElementAtIndex(i), bindings[i]);
            }
        }

        private static void WireBindingArray(SerializedProperty property, FieldStationVisitorBinding[] bindings,
            System.Action<SerializedProperty, FieldStationVisitorBinding> apply)
        {
            property.arraySize = bindings.Length;
            for (int i = 0; i < bindings.Length; i++)
            {
                apply(property.GetArrayElementAtIndex(i), bindings[i]);
            }
        }

        private readonly struct FieldStationCard
        {
            public FieldStationCard(Text titleText, Text descriptionText, Button journalButton, Button atlasButton,
                Button goalsButton, Button settingsButton, GoalsPanelController goalsPanel)
            {
                TitleText = titleText;
                DescriptionText = descriptionText;
                JournalButton = journalButton;
                AtlasButton = atlasButton;
                GoalsButton = goalsButton;
                SettingsButton = settingsButton;
                GoalsPanel = goalsPanel;
            }

            public Text TitleText { get; }
            public Text DescriptionText { get; }
            public Button JournalButton { get; }
            public Button AtlasButton { get; }
            public Button GoalsButton { get; }
            public Button SettingsButton { get; }
            public GoalsPanelController GoalsPanel { get; }
        }

        // The helpers below duplicate the private ones in CreateV01OverworldScene so both
        // generators produce identical visuals; keep them in sync if styles change.

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPosition, Vector2 size)
        {
            Image image = CreateImage(name, parent, new Color(0.78f, 0.92f, 0.76f), anchoredPosition, size);
            ApplyRoundedButtonStyle(image);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            Text text = CreateText("Label", image.transform, label, 28, TextAnchor.MiddleCenter, Vector2.zero, size);
            text.color = new Color(0.06f, 0.16f, 0.18f);
            return button;
        }

        private static void ApplyRoundedButtonStyle(Image image)
        {
            image.sprite = LoadRoundedPanelSprite();
            image.type = Image.Type.Sliced;
            Shadow shadow = image.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.28f);
            shadow.effectDistance = new Vector2(2f, -3f);
        }

        private static void ApplyRoundedPanelStyle(Image image)
        {
            image.sprite = LoadRoundedPanelSprite();
            image.type = Image.Type.Sliced;
        }

        private static Sprite LoadRoundedPanelSprite()
        {
            const string path = "Assets/Art/UI/rounded_panel.png";
            Sprite sprite = LoadSprite(path, 100f);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                Vector4 targetBorder = new Vector4(32f, 32f, 32f, 32f);
                if (importer.spriteBorder != targetBorder)
                {
                    importer.spriteBorder = targetBorder;
                    importer.SaveAndReimport();
                }
            }

            return sprite;
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

        private static Sprite LoadSprite(string spritePath, float pixelsPerUnit = 0f)
        {
            TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
            if (importer != null)
            {
                bool changed = importer.textureType != TextureImporterType.Sprite
                    || importer.spriteImportMode != SpriteImportMode.Single
                    || (pixelsPerUnit > 0f && importer.spritePixelsPerUnit != pixelsPerUnit);

                if (changed)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    if (pixelsPerUnit > 0f)
                    {
                        importer.spritePixelsPerUnit = pixelsPerUnit;
                    }

                    importer.mipmapEnabled = false;
                    importer.SaveAndReimport();
                }
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite != null)
            {
                return sprite;
            }

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(spritePath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite assetSprite)
                {
                    return assetSprite;
                }
            }

            Debug.LogWarning($"Could not load sprite at {spritePath}. Missing art degrades gracefully.");
            return null;
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
