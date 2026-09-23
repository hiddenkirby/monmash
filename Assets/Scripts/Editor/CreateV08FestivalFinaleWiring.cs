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
    // Wires the v0.8 coast festival (docs/V0_8_COAST_FESTIVAL.md) and Old Barnaby finale
    // (docs/V0_8_BARNABY_FINALE.md) into the existing Overworld scene: two world-side venues
    // beside the walkable route plus UI cards whose buttons drive the authoritative controllers.
    // Both routes are started from UI buttons (not colliders) so no trigger colliders ever sit
    // on the walkable route. Idempotent: re-running removes and rebuilds only these objects.
    // Persistence is controller-owned: the festival remembers the ribbon from the contest win
    // and the finale reconciles from the catch; presentation events only refresh the station.
    // UI helper methods mirror CreateV01OverworldScene (its helpers are private; keep the
    // copies visually consistent with it and with CreateV08FieldStationWiring).
    public static class CreateV08FestivalFinaleWiring
    {
        private const string ScenePath = "Assets/Scenes/Overworld.unity";
        private const string FestivalDefinitionPath = "Assets/Data/Festival/GreatLowTideFestival.asset";
        private const string FinaleDefinitionPath = "Assets/Data/Finale/OldBarnabyFinale.asset";
        private const string SpeciesDatabasePath = "Assets/Data/Databases/SpeciesDatabase.asset";
        private const int FestivalCellX = 16;
        private const int FestivalCellY = 7;
        private const int FinaleCellX = 22;
        private const int FinaleCellY = 6;

        [MenuItem("Tools/Tidepool/Wire v0.8 Festival and Finale Into Overworld")]
        public static void WireFestivalAndFinaleIntoOverworld()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            CreateCoastFestivalDefinition.CreateDefinition();
            CoastFestivalDefinition festivalDefinition = AssetDatabase.LoadAssetAtPath<CoastFestivalDefinition>(FestivalDefinitionPath);
            CreateBarnabyFinaleDefinition.CreateDefinition();
            BarnabyFinaleDefinition finaleDefinition = AssetDatabase.LoadAssetAtPath<BarnabyFinaleDefinition>(FinaleDefinitionPath);
            if (festivalDefinition == null || finaleDefinition == null)
            {
                Debug.LogError("Festival or finale definition missing after generation.");
                return;
            }

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

            FieldStationController stationController = Object.FindAnyObjectByType<FieldStationController>();
            if (stationController == null)
            {
                Debug.LogError("FieldStationController not found. Run Tools/Tidepool/Wire v0.8 Field Station Into Overworld first.");
                return;
            }

            RemoveExistingWiring(safeArea);

            // World venues first; the cards hand off to controllers that live there.
            CoastFestivalController festivalController = CreateFestivalVenue(festivalDefinition, playerMover);
            BarnabyFinaleController finaleController = CreateFinaleSpot(finaleDefinition, playerMover);

            FestivalCard festivalCard = CreateFestivalCard(safeArea);
            FinaleCard finaleCard = CreateFinaleCard(safeArea);
            (GameObject festivalIntroPanel, Text festivalIntroText, Button festivalSkipButton) = CreateIntroPanel(
                safeArea, "FestivalIntroPanel", "FestivalIntroText", "FestivalSkipButton", "Skip");
            (GameObject finaleIntroPanel, Text finaleIntroText, Button finaleSkipButton) = CreateIntroPanel(
                safeArea, "FinaleIntroPanel", "FinaleIntroText", "FinaleSkipButton", "Continue");

            SerializedObject serializedFestival = new SerializedObject(festivalController);
            serializedFestival.FindProperty("definition").objectReferenceValue = festivalDefinition;
            serializedFestival.FindProperty("contestTrigger").objectReferenceValue = festivalController.GetComponent<ContestTrigger>();
            serializedFestival.FindProperty("invitationRoot").objectReferenceValue = festivalCard.InvitationGroup;
            serializedFestival.FindProperty("presentationRoot").objectReferenceValue = festivalIntroPanel;
            serializedFestival.FindProperty("celebrationRoot").objectReferenceValue = festivalController.transform.Find("Celebration").gameObject;
            serializedFestival.FindProperty("presentationText").objectReferenceValue = festivalIntroText;
            serializedFestival.FindProperty("skipButton").objectReferenceValue = festivalSkipButton;
            serializedFestival.FindProperty("introductionSeconds").floatValue = 1.5f;
            serializedFestival.ApplyModifiedProperties();

            SerializedObject serializedFinale = new SerializedObject(finaleController);
            serializedFinale.FindProperty("definition").objectReferenceValue = finaleDefinition;
            serializedFinale.FindProperty("playerMover").objectReferenceValue = playerMover;
            serializedFinale.FindProperty("invitationRoot").objectReferenceValue = finaleCard.BeginGroup;
            serializedFinale.FindProperty("presentationRoot").objectReferenceValue = finaleIntroPanel;
            serializedFinale.FindProperty("celebrationRoot").objectReferenceValue = finaleController.transform.Find("Celebration").gameObject;
            serializedFinale.FindProperty("presentationText").objectReferenceValue = finaleIntroText;
            serializedFinale.FindProperty("skipButton").objectReferenceValue = finaleSkipButton;
            serializedFinale.FindProperty("autoLaunchSeconds").floatValue = 2f;
            serializedFinale.ApplyModifiedProperties();

            // Card buttons drive the controllers through void wrappers so the listeners
            // persist across scene save/reload.
            UnityEditor.Events.UnityEventTools.AddPersistentListener(festivalCard.StartButton.onClick, festivalController.StartFestival);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(festivalSkipButton.onClick, festivalController.SkipIntroduction);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(finaleCard.BeginButton.onClick, finaleController.BeginFinale);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(finaleCard.MemoryButton.onClick, finaleController.ShowMemory);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(finaleSkipButton.onClick, finaleController.SkipPresentation);

            // Coast response: presentation events refresh the station so the festival-ribbon
            // and finale decorations light up; persistence itself is controller-owned.
            UnityEditor.Events.UnityEventTools.AddPersistentListener(festivalController.FestivalWon, stationController.RefreshStation);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(festivalController.FestivalRetryOffered, stationController.RefreshStation);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(finaleController.FinaleCompleted, stationController.RefreshStation);

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Festival venue and Barnaby finale wired into Overworld: venues, cards, intro panels, and station refresh events.");
        }

        private static void RemoveExistingWiring(Transform safeArea)
        {
            GameObject festivalVenue = GameObject.Find("CoastFestivalVenue");
            if (festivalVenue != null)
            {
                Object.DestroyImmediate(festivalVenue);
            }

            GameObject finaleSpot = GameObject.Find("BarnabyFinaleSpot");
            if (finaleSpot != null)
            {
                Object.DestroyImmediate(finaleSpot);
            }

            string[] cardNames = { "FestivalCard", "FinaleCard", "FestivalIntroPanel", "FinaleIntroPanel" };
            for (int i = 0; i < cardNames.Length; i++)
            {
                Transform previous = safeArea.Find(cardNames[i]);
                if (previous != null)
                {
                    Object.DestroyImmediate(previous.gameObject);
                }
            }
        }

        private static CoastFestivalController CreateFestivalVenue(CoastFestivalDefinition definition, PlayerGridMover playerMover)
        {
            Grid grid = Object.FindAnyObjectByType<Grid>();
            Vector3 origin = grid != null
                ? grid.GetCellCenterWorld(new Vector3Int(FestivalCellX, FestivalCellY, 0))
                : new Vector3(FestivalCellX, FestivalCellY + 0.5f, 0f);

            GameObject venue = new GameObject("CoastFestivalVenue");
            venue.transform.position = origin;

            // The festival ring itself is always-visible presentation with no colliders.
            CreateSpriteChild("Ring", venue.transform, "Assets/Art/Tiles/KenneyRpgBase/sand_plain.png", new Color(0.99f, 0.90f, 0.66f, 0.95f), 0.62f);
            CreateSpriteChild("Bunting", venue.transform, "Assets/Art/Tiles/KenneyRpgBase/grass_tufts.png", new Color(0.94f, 0.62f, 0.55f, 0.95f), 0.4f, new Vector3(0.9f, 0.7f, 0f));
            CreateSpriteChild("Torch", venue.transform, "Assets/Art/Tiles/KenneyRpgBase/shrub_green.png", new Color(0.99f, 0.80f, 0.45f, 0.95f), 0.4f, new Vector3(-1.1f, 0.5f, 0f));

            // Static spectators around the ring: no colliders, no contest logic.
            string[] spectatorSpecies = { "blip", "frillick", "nubbin", "sputter" };
            Vector3[] spectatorOffsets =
            {
                new Vector3(-1.7f, -0.5f, 0f),
                new Vector3(-2.1f, 0.5f, 0f),
                new Vector3(1.8f, -0.4f, 0f),
                new Vector3(2.2f, 0.7f, 0f)
            };
            for (int i = 0; i < spectatorSpecies.Length; i++)
            {
                CreateSpriteChild($"Spectator_{spectatorSpecies[i]}", venue.transform,
                    $"Assets/Art/Creatures/{spectatorSpecies[i]}.png", Color.white, 0.22f, spectatorOffsets[i]);
            }

            GameObject celebration = new GameObject("Celebration");
            celebration.transform.SetParent(venue.transform, false);
            celebration.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            CreateSpriteChild("Ribbon", celebration.transform, "Assets/Art/Tiles/KenneyRpgBase/crate_large.png", new Color(0.98f, 0.55f, 0.42f, 0.95f), 0.35f);
            celebration.SetActive(false);

            CoastFestivalController controller = venue.AddComponent<CoastFestivalController>();
            ContestTrigger trigger = venue.AddComponent<ContestTrigger>();
            SerializedObject serializedTrigger = new SerializedObject(trigger);
            serializedTrigger.FindProperty("speciesDatabase").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SpeciesDatabase>(SpeciesDatabasePath);
            serializedTrigger.FindProperty("playerSpeciesId").stringValue = "blip";
            serializedTrigger.FindProperty("visitingSpeciesId").stringValue = "wobbet";
            serializedTrigger.FindProperty("partySelectSceneName").stringValue = "PartySelect";
            serializedTrigger.FindProperty("playerMover").objectReferenceValue = playerMover;
            serializedTrigger.ApplyModifiedProperties();
            return controller;
        }

        private static BarnabyFinaleController CreateFinaleSpot(BarnabyFinaleDefinition definition, PlayerGridMover playerMover)
        {
            Grid grid = Object.FindAnyObjectByType<Grid>();
            Vector3 origin = grid != null
                ? grid.GetCellCenterWorld(new Vector3Int(FinaleCellX, FinaleCellY, 0))
                : new Vector3(FinaleCellX, FinaleCellY + 0.5f, 0f);

            GameObject spot = new GameObject("BarnabyFinaleSpot");
            spot.transform.position = origin;

            // The old stones and Old Barnaby himself: presentation only, no colliders.
            CreateSpriteChild("OldStones", spot.transform, "Assets/Art/Tiles/KenneyRpgBase/rock_mossy.png", new Color(0.72f, 0.74f, 0.72f, 0.98f), 0.62f);
            CreateSpriteChild("OldBarnaby", spot.transform, "Assets/Art/Creatures/old-barnaby.png", Color.white, 0.22f, new Vector3(0f, 0.8f, 0f));

            GameObject celebration = new GameObject("Celebration");
            celebration.transform.SetParent(spot.transform, false);
            celebration.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            CreateSpriteChild("ShoreLights", celebration.transform, "Assets/Art/Tiles/KenneyRpgBase/kelp_tall.png", new Color(0.99f, 0.87f, 0.50f, 0.95f), 0.35f);
            celebration.SetActive(false);

            BarnabyFinaleController controller = spot.AddComponent<BarnabyFinaleController>();
            return controller;
        }

        private static FestivalCard CreateFestivalCard(Transform safeArea)
        {
            // Left half of the strip above the field station card.
            Image panel = CreateImage("FestivalCard", safeArea, new Color(0.96f, 0.90f, 0.80f, 0.95f), new Vector2(-260f, -96f), new Vector2(512f, 88f));
            ApplyRoundedPanelStyle(panel);

            GameObject invitationGroup = panel.gameObject;

            Text titleText = CreateText("FestivalTitle", panel.transform, "Coast Festival", 26, TextAnchor.MiddleLeft, new Vector2(-140f, 14f), new Vector2(280f, 40f));
            titleText.color = new Color(0.06f, 0.16f, 0.18f);

            Text descriptionText = CreateText("FestivalDescription", panel.transform, "One friendly contest for the whole coast.", 18, TextAnchor.MiddleLeft, new Vector2(-120f, -18f), new Vector2(320f, 30f));
            descriptionText.color = new Color(0.24f, 0.34f, 0.32f);

            Button startButton = CreateButton("FestivalStartButton", panel.transform, "Contest", new Vector2(184f, 0f), new Vector2(128f, 88f));
            return new FestivalCard(invitationGroup, startButton);
        }

        private static FinaleCard CreateFinaleCard(Transform safeArea)
        {
            // Right half of the strip above the field station card.
            Image panel = CreateImage("FinaleCard", safeArea, new Color(0.88f, 0.90f, 0.94f, 0.95f), new Vector2(260f, -96f), new Vector2(512f, 88f));
            ApplyRoundedPanelStyle(panel);

            Text titleText = CreateText("FinaleTitle", panel.transform, "Rocky Shelf", 26, TextAnchor.MiddleLeft, new Vector2(-130f, 14f), new Vector2(280f, 40f));
            titleText.color = new Color(0.06f, 0.16f, 0.18f);

            Text descriptionText = CreateText("FinaleDescription", panel.transform, "The old stones ripple when you visit.", 18, TextAnchor.MiddleLeft, new Vector2(-110f, -18f), new Vector2(320f, 30f));
            descriptionText.color = new Color(0.24f, 0.34f, 0.32f);

            Button beginButton = CreateButton("FinaleBeginButton", panel.transform, "Visit", new Vector2(122f, 0f), new Vector2(112f, 88f));
            Button memoryButton = CreateButton("FinaleMemoryButton", panel.transform, "Memory", new Vector2(240f, 0f), new Vector2(112f, 88f));

            // The begin group is the controller's invitationRoot: it hides once the finale
            // is complete, while the memory button stays available on the always-visible card.
            GameObject beginGroup = beginButton.gameObject;
            return new FinaleCard(beginGroup, beginButton, memoryButton);
        }

        private static (GameObject panel, Text introText, Button skipButton) CreateIntroPanel(
            Transform safeArea, string panelName, string textName, string buttonName, string buttonLabel)
        {
            Image panel = CreateImage(panelName, safeArea, new Color(0.10f, 0.24f, 0.28f, 0.96f), new Vector2(0f, 148f), new Vector2(640f, 300f));
            ApplyRoundedPanelStyle(panel);

            Text introText = CreateText(textName, panel.transform, string.Empty, 24, TextAnchor.UpperLeft, new Vector2(0f, 20f), new Vector2(560f, 170f));
            introText.color = Color.white;

            Button skipButton = CreateButton(buttonName, panel.transform, buttonLabel, new Vector2(0f, -108f), new Vector2(200f, 88f));
            panel.gameObject.SetActive(false);
            return (panel.gameObject, introText, skipButton);
        }

        private readonly struct FestivalCard
        {
            public FestivalCard(GameObject invitationGroup, Button startButton)
            {
                InvitationGroup = invitationGroup;
                StartButton = startButton;
            }

            public GameObject InvitationGroup { get; }
            public Button StartButton { get; }
        }

        private readonly struct FinaleCard
        {
            public FinaleCard(GameObject beginGroup, Button beginButton, Button memoryButton)
            {
                BeginGroup = beginGroup;
                BeginButton = beginButton;
                MemoryButton = memoryButton;
            }

            public GameObject BeginGroup { get; }
            public Button BeginButton { get; }
            public Button MemoryButton { get; }
        }

        // Loads the sprite without touching its importer settings: overriding
        // pixels-per-unit on shared creature art would rescale it everywhere.
        private static SpriteRenderer CreateSpriteChild(string name, Transform parent, string spritePath, Color color, float scale)
        {
            return CreateSpriteChild(name, parent, spritePath, color, scale, Vector3.zero);
        }

        private static SpriteRenderer CreateSpriteChild(string name, Transform parent, string spritePath, Color color, float scale, Vector3 localPosition)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPosition;
            child.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadSprite(spritePath, 0f);
            renderer.color = color;
            renderer.sortingOrder = 4;
            return renderer;
        }

        // The helpers below duplicate the private ones in CreateV01OverworldScene so all
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
    }
}
