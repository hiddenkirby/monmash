using System.IO;
using Tidepool.Domain;
using Tidepool.Runtime;
using Tidepool.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace Tidepool.Editor
{
    public static class CreateV01OverworldScene
    {
        private const string ScenePath = "Assets/Scenes/Overworld.unity";
        private const string TileAssetFolder = "Assets/Data/Tiles";
        private const string PlayerSpritePath = "Assets/Art/Creatures/blip.png";
        private const string SpeciesDatabasePath = "Assets/Data/Databases/SpeciesDatabase.asset";
        private const int MinX = -12;
        private const int MaxX = 26;
        private const int MinY = -8;
        private const int MaxY = 8;
        private const int MeadowEndX = 12;
        private const int KelpEndX = 19;

        [MenuItem("Tools/Tidepool/Create v0.1 Overworld Scene")]
        public static void CreateOverworldScene()
        {
            EnsureFolder("Assets/Scenes");
            EnsureFolder("Assets/Data");
            EnsureFolder(TileAssetFolder);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            TileBase sand = CreateTile("sand_plain", "Assets/Art/Tiles/KenneyRpgBase/sand_plain.png");
            TileBase water = CreateTile("water_plain", "Assets/Art/Tiles/KenneyRpgBase/water_plain.png");
            TileBase grass = CreateTile("grass_plain", "Assets/Art/Tiles/KenneyRpgBase/grass_plain.png");
            TileBase seagrass = CreateTile("grass_tufts", "Assets/Art/Tiles/KenneyRpgBase/grass_tufts.png");
            TileBase shrub = CreateTile("shrub_green", "Assets/Art/Tiles/KenneyRpgBase/shrub_green.png");
            TileBase crate = CreateTile("crate_large", "Assets/Art/Tiles/KenneyRpgBase/crate_large.png");
            TileBase kelp = CreateTile("kelp_tall", "Assets/Art/Tiles/KenneyRpgBase/kelp_tall.png");
            TileBase rock = CreateTile("rock_mossy", "Assets/Art/Tiles/KenneyRpgBase/rock_mossy.png");
            TileBase darkWater = CreateTile("water_dark", "Assets/Art/Tiles/KenneyRpgBase/water_dark.png");

            GameObject gridObject = new GameObject("Grid");
            Grid grid = gridObject.AddComponent<Grid>();
            grid.cellSize = Vector3.one;

            Tilemap ground = CreateTilemap(gridObject.transform, "Ground", 0);
            Tilemap obstacles = CreateTilemap(gridObject.transform, "Obstacles", 2);
            Tilemap seagrassMap = CreateTilemap(gridObject.transform, "Seagrass", 1);
            obstacles.gameObject.AddComponent<TilemapCollider2D>();

            PaintGround(ground, sand, water, grass, kelp, rock, darkWater);
            PaintObstacles(obstacles, shrub, crate);
            PaintSeagrass(seagrassMap, seagrass);

            GameObject playerSpawn = new GameObject("PlayerSpawn");
            playerSpawn.transform.position = grid.GetCellCenterWorld(new Vector3Int(-9, 0, 0));

            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = playerSpawn.transform.position;
            SpriteRenderer playerRenderer = playerObject.AddComponent<SpriteRenderer>();
            playerRenderer.sprite = LoadSprite(PlayerSpritePath);
            playerRenderer.sortingOrder = 3;
            playerObject.transform.localScale = new Vector3(0.32f, 0.32f, 1f);
            PlayerGridMover playerMover = playerObject.AddComponent<PlayerGridMover>();
            WirePlayerMover(playerMover, grid, playerObject.transform, ground, obstacles);

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.60f, 0.82f, 0.88f);
            cameraObject.transform.position = new Vector3(-1.5f, 0f, -10f);
            cameraObject.AddComponent<AudioListener>();
            CameraFollow2D cameraFollow = cameraObject.AddComponent<CameraFollow2D>();
            WireCameraFollow(cameraFollow, playerObject.transform);

            SpeciesDatabase database = AssetDatabase.LoadAssetAtPath<SpeciesDatabase>(SpeciesDatabasePath);
            CreateZoneEncounterDirectors(gridObject.transform, seagrassMap, playerMover, database);

            Canvas canvas = CreateCanvas();
            RectTransform safeArea = CreateRect("SafeArea", canvas.transform);
            safeArea.anchorMin = Vector2.zero;
            safeArea.anchorMax = Vector2.one;
            safeArea.offsetMin = Vector2.zero;
            safeArea.offsetMax = Vector2.zero;
            safeArea.gameObject.AddComponent<SafeAreaFitter>();
            ZoneWelcomeBanner zoneWelcomeBanner = CreateZoneWelcomeBanner(safeArea);
            CreateZoneTransitions(gridObject.transform, playerObject.transform, playerMover, grid, zoneWelcomeBanner);
            CreateFirstRunGuidance(safeArea);
            CreateContestButton(safeArea, playerMover, database);
            CreateJournalButton(safeArea, playerMover);
            CreateCharacterButton(safeArea, playerMover);
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Tilemap CreateTilemap(Transform parent, string name, int sortingOrder)
        {
            GameObject tilemapObject = new GameObject(name);
            tilemapObject.transform.SetParent(parent);
            Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
            TilemapRenderer renderer = tilemapObject.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = sortingOrder;
            return tilemap;
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

        private static void CreateFirstRunGuidance(RectTransform safeArea)
        {
            Image panel = CreateImage("FirstRunGuidancePanel", safeArea, new Color(0.10f, 0.26f, 0.28f, 0.92f), new Vector2(0f, 188f), new Vector2(720f, 104f));
            FirstRunGuidanceController guidance = panel.gameObject.AddComponent<FirstRunGuidanceController>();

            Text label = CreateText("GuidanceText", panel.transform, "Tap to walk. Look in the seagrass.", 28, TextAnchor.MiddleLeft, new Vector2(-48f, 0f), new Vector2(500f, 72f));
            label.color = Color.white;

            Button dismissButton = CreateButton("DismissButton", panel.transform, "OK", new Vector2(292f, 0f), new Vector2(112f, 88f));
            WireFirstRunGuidance(guidance, panel.gameObject, label, dismissButton);
        }

        private static ZoneWelcomeBanner CreateZoneWelcomeBanner(RectTransform safeArea)
        {
            Image panel = CreateImage("ZoneWelcomeBanner", safeArea, new Color(0.08f, 0.22f, 0.26f, 0.88f), new Vector2(0f, -24f), new Vector2(680f, 112f));
            RectTransform panelTransform = panel.rectTransform;
            panelTransform.anchorMin = new Vector2(0.5f, 1f);
            panelTransform.anchorMax = new Vector2(0.5f, 1f);
            panelTransform.pivot = new Vector2(0.5f, 1f);
            panel.raycastTarget = false;

            CanvasGroup canvasGroup = panel.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            Text zoneName = CreateText("ZoneName", panel.transform, "Tidepool Shallows", 30, TextAnchor.LowerCenter, new Vector2(0f, 16f), new Vector2(600f, 42f));
            zoneName.color = Color.white;

            Text subtitle = CreateText("ZoneSubtitle", panel.transform, "Where the water is warm and clear", 22, TextAnchor.UpperCenter, new Vector2(0f, -24f), new Vector2(600f, 40f));
            subtitle.color = new Color(0.86f, 0.96f, 0.92f);

            ZoneWelcomeBanner banner = panel.gameObject.AddComponent<ZoneWelcomeBanner>();
            WireZoneWelcomeBanner(banner, panel.gameObject, canvasGroup, zoneName, subtitle);
            return banner;
        }

        private static void CreateContestButton(RectTransform safeArea, PlayerGridMover playerMover, SpeciesDatabase database)
        {
            Button contestButton = CreateButton("ContestButton", safeArea, "Contest", new Vector2(-412f, -300f), new Vector2(180f, 96f));
            contestButton.transform.SetAsFirstSibling();

            ContestTrigger trigger = contestButton.gameObject.AddComponent<ContestTrigger>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(contestButton.onClick, trigger.StartContest);

            SerializedObject serializedTrigger = new SerializedObject(trigger);
            serializedTrigger.FindProperty("speciesDatabase").objectReferenceValue = database;
            serializedTrigger.FindProperty("playerMover").objectReferenceValue = playerMover;
            serializedTrigger.FindProperty("playerSpeciesId").stringValue = "blip";
            serializedTrigger.FindProperty("visitingSpeciesId").stringValue = "wobbet";
            serializedTrigger.FindProperty("partySelectSceneName").stringValue = "PartySelect";
            serializedTrigger.ApplyModifiedProperties();
        }

        private static void CreateJournalButton(RectTransform safeArea, PlayerGridMover playerMover)
        {
            Button journalButton = CreateButton("JournalButton", safeArea, "Journal", new Vector2(-200f, -300f), new Vector2(180f, 96f));
            journalButton.transform.SetAsFirstSibling();

            JournalTrigger trigger = journalButton.gameObject.AddComponent<JournalTrigger>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(journalButton.onClick, trigger.OpenJournal);

            SerializedObject serializedTrigger = new SerializedObject(trigger);
            serializedTrigger.FindProperty("playerMover").objectReferenceValue = playerMover;
            serializedTrigger.FindProperty("journalSceneName").stringValue = "Journal";
            serializedTrigger.ApplyModifiedProperties();
        }

        private static void CreateCharacterButton(RectTransform safeArea, PlayerGridMover playerMover)
        {
            Button characterButton = CreateButton("CharacterButton", safeArea, "Character", new Vector2(12f, -300f), new Vector2(180f, 96f));
            characterButton.transform.SetAsFirstSibling();

            CharacterSelectTrigger trigger = characterButton.gameObject.AddComponent<CharacterSelectTrigger>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(characterButton.onClick, trigger.OpenCharacterSelect);

            SerializedObject serializedTrigger = new SerializedObject(trigger);
            serializedTrigger.FindProperty("playerMover").objectReferenceValue = playerMover;
            serializedTrigger.FindProperty("characterSelectSceneName").stringValue = "CharacterSelect";
            serializedTrigger.ApplyModifiedProperties();
        }

        private static void CreateZoneTransitions(Transform gridTransform, Transform playerRoot, PlayerGridMover playerMover,
            Grid grid, ZoneWelcomeBanner zoneWelcomeBanner)
        {
            CreateZoneTransition("ShallowsToMeadowTransition", gridTransform, playerRoot, playerMover, grid,
                new Vector3(MeadowEndX - 1, 0, 0), ZoneId.SeagrassMeadow, zoneWelcomeBanner);
            CreateZoneTransition("MeadowToKelpTransition", gridTransform, playerRoot, playerMover, grid,
                new Vector3(KelpEndX - 1, 0, 0), ZoneId.KelpCurtain, zoneWelcomeBanner);
            CreateZoneTransition("KelpToRockyTransition", gridTransform, playerRoot, playerMover, grid,
                new Vector3(MaxX - 1, 0, 0), ZoneId.RockyShelf, zoneWelcomeBanner);
        }

        private static void CreateZoneTransition(string name, Transform gridTransform, Transform playerRoot,
            PlayerGridMover playerMover, Grid grid, Vector3 triggerPosition, ZoneId destinationZone,
            ZoneWelcomeBanner zoneWelcomeBanner)
        {
            GameObject triggerObj = new GameObject(name);
            triggerObj.transform.SetParent(gridTransform);
            triggerObj.transform.position = triggerPosition;

            BoxCollider2D collider = triggerObj.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(2f, 16f);

            ZoneTransitionTrigger trigger = triggerObj.AddComponent<ZoneTransitionTrigger>();
            SerializedObject serializedTrigger = new SerializedObject(trigger);
            serializedTrigger.FindProperty("destinationZone").enumValueIndex = (int)destinationZone;
            serializedTrigger.FindProperty("playerRoot").objectReferenceValue = playerRoot;
            serializedTrigger.FindProperty("playerMover").objectReferenceValue = playerMover;
            serializedTrigger.FindProperty("grid").objectReferenceValue = grid;
            serializedTrigger.ApplyModifiedProperties();

            WireZoneTransitionBanner(trigger, destinationZone, zoneWelcomeBanner);
        }

        private static void CreateZoneEncounterDirectors(Transform gridTransform, Tilemap seagrassMap,
            PlayerGridMover playerMover, SpeciesDatabase database)
        {
            if (database == null)
            {
                return;
            }

            CreateZoneEncounterDirector(gridTransform, "ShallowsEncounterDirector", seagrassMap,
                playerMover, database, ZoneId.TidepoolShallows);
            CreateZoneEncounterDirector(gridTransform, "MeadowEncounterDirector", seagrassMap,
                playerMover, database, ZoneId.SeagrassMeadow);
            CreateZoneEncounterDirector(gridTransform, "KelpEncounterDirector", seagrassMap,
                playerMover, database, ZoneId.KelpCurtain);
            CreateZoneEncounterDirector(gridTransform, "RockyEncounterDirector", seagrassMap,
                playerMover, database, ZoneId.RockyShelf);
        }

        private static void CreateZoneEncounterDirector(Transform gridTransform, string name, Tilemap seagrassMap,
            PlayerGridMover playerMover, SpeciesDatabase database, ZoneId zone)
        {
            GameObject directorObj = new GameObject(name);
            directorObj.transform.SetParent(gridTransform);

            EncounterDirector director = directorObj.AddComponent<EncounterDirector>();
            SerializedObject serializedDirector = new SerializedObject(director);
            serializedDirector.FindProperty("player").objectReferenceValue = playerMover;
            serializedDirector.FindProperty("seagrassTilemap").objectReferenceValue = seagrassMap;
            serializedDirector.FindProperty("speciesDatabase").objectReferenceValue = database;
            serializedDirector.FindProperty("currentZone").enumValueIndex = (int)zone;
            serializedDirector.FindProperty("catchSceneName").stringValue = "CatchEncounter";
            serializedDirector.ApplyModifiedProperties();
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPosition, Vector2 size)
        {
            Image image = CreateImage(name, parent, new Color(0.78f, 0.92f, 0.76f), anchoredPosition, size);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            Text text = CreateText("Label", image.transform, label, 28, TextAnchor.MiddleCenter, Vector2.zero, size);
            text.color = new Color(0.06f, 0.16f, 0.18f);
            return button;
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

        private static void WirePlayerMover(PlayerGridMover playerMover, Grid grid, Transform actor, Tilemap ground, Tilemap obstacles)
        {
            SerializedObject serializedMover = new SerializedObject(playerMover);
            serializedMover.FindProperty("grid").objectReferenceValue = grid;
            serializedMover.FindProperty("actor").objectReferenceValue = actor;
            serializedMover.FindProperty("groundTilemap").objectReferenceValue = ground;
            serializedMover.FindProperty("obstacleTilemap").objectReferenceValue = obstacles;
            serializedMover.ApplyModifiedProperties();
        }

        private static void WireCameraFollow(CameraFollow2D cameraFollow, Transform target)
        {
            SerializedObject serializedFollow = new SerializedObject(cameraFollow);
            serializedFollow.FindProperty("target").objectReferenceValue = target;
            serializedFollow.ApplyModifiedProperties();
        }

        private static void WireFirstRunGuidance(FirstRunGuidanceController guidance, GameObject root, Text label, Button dismissButton)
        {
            SerializedObject serializedGuidance = new SerializedObject(guidance);
            serializedGuidance.FindProperty("guidanceRoot").objectReferenceValue = root;
            serializedGuidance.FindProperty("guidanceText").objectReferenceValue = label;
            serializedGuidance.FindProperty("dismissButton").objectReferenceValue = dismissButton;
            serializedGuidance.ApplyModifiedProperties();
        }

        private static void WireZoneWelcomeBanner(ZoneWelcomeBanner banner, GameObject root, CanvasGroup canvasGroup, Text zoneName, Text subtitle)
        {
            SerializedObject serializedBanner = new SerializedObject(banner);
            serializedBanner.FindProperty("bannerRoot").objectReferenceValue = root;
            serializedBanner.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
            serializedBanner.FindProperty("zoneNameText").objectReferenceValue = zoneName;
            serializedBanner.FindProperty("subtitleText").objectReferenceValue = subtitle;
            serializedBanner.FindProperty("visibleSeconds").floatValue = 2f;
            serializedBanner.FindProperty("fadeSeconds").floatValue = 0.3f;
            serializedBanner.ApplyModifiedProperties();
        }

        private static void WireZoneTransitionBanner(ZoneTransitionTrigger trigger, ZoneId destinationZone, ZoneWelcomeBanner banner)
        {
            if (banner == null)
            {
                return;
            }

            switch (destinationZone)
            {
                case ZoneId.SeagrassMeadow:
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(trigger.EnteredZone, banner.ShowSeagrassMeadow);
                    break;
                case ZoneId.KelpCurtain:
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(trigger.EnteredZone, banner.ShowKelpCurtain);
                    break;
                case ZoneId.RockyShelf:
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(trigger.EnteredZone, banner.ShowRockyShelf);
                    break;
                default:
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(trigger.EnteredZone, banner.ShowTidepoolShallows);
                    break;
            }
        }

        private static void PaintGround(Tilemap ground, TileBase sand, TileBase water, TileBase grass, TileBase kelp, TileBase rock, TileBase darkWater)
        {
            for (int x = MinX; x <= MaxX; x++)
            {
                for (int y = MinY; y <= MaxY; y++)
                {
                    TileBase tile;
                    if (x < 1)
                    {
                        bool isPoolTile = (x + y) % 5 == 0 || y <= -6 || y >= 7;
                        tile = isPoolTile ? water : sand;
                    }
                    else if (x <= MeadowEndX)
                    {
                        tile = grass;
                    }
                    else if (x <= KelpEndX)
                    {
                        tile = (x + y) % 4 == 0 ? darkWater : kelp;
                    }
                    else
                    {
                        tile = (x + y) % 3 == 0 ? rock : sand;
                    }

                    ground.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }

        private static void PaintObstacles(Tilemap obstacles, TileBase shrub, TileBase crate)
        {
            for (int x = MinX; x <= MaxX; x++)
            {
                obstacles.SetTile(new Vector3Int(x, MinY, 0), shrub);
                obstacles.SetTile(new Vector3Int(x, MaxY, 0), shrub);
            }

            for (int y = MinY; y <= MaxY; y++)
            {
                obstacles.SetTile(new Vector3Int(MinX, y, 0), shrub);
                obstacles.SetTile(new Vector3Int(MaxX, y, 0), shrub);
            }

            Vector3Int[] props =
            {
                new Vector3Int(-7, -3, 0),
                new Vector3Int(-3, 4, 0),
                new Vector3Int(4, 6, 0),
                new Vector3Int(11, -5, 0),
                new Vector3Int(1, -1, 0)
            };

            for (int i = 0; i < props.Length; i++)
            {
                obstacles.SetTile(props[i], i == props.Length - 1 ? crate : shrub);
            }
        }

        private static void PaintSeagrass(Tilemap seagrassMap, TileBase seagrass)
        {
            for (int x = 3; x <= MeadowEndX; x++)
            {
                for (int y = -5; y <= 5; y++)
                {
                    if ((x + y) % 2 == 0)
                    {
                        seagrassMap.SetTile(new Vector3Int(x, y, 0), seagrass);
                    }
                }
            }

            for (int x = MeadowEndX + 1; x <= KelpEndX; x++)
            {
                for (int y = -5; y <= 5; y++)
                {
                    if ((x + y) % 3 == 0)
                    {
                        seagrassMap.SetTile(new Vector3Int(x, y, 0), seagrass);
                    }
                }
            }

            for (int x = KelpEndX + 1; x <= MaxX; x++)
            {
                for (int y = -5; y <= 5; y++)
                {
                    if ((x + y) % 4 == 0)
                    {
                        seagrassMap.SetTile(new Vector3Int(x, y, 0), seagrass);
                    }
                }
            }
        }

        private static TileBase CreateTile(string name, string spritePath)
        {
            string tilePath = $"{TileAssetFolder}/{name}.asset";
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, tilePath);
            }

            Sprite sprite = LoadSprite(spritePath, 64f);
            if (sprite != null)
            {
                tile.sprite = sprite;
            }
            else if (tile.sprite == null)
            {
                Debug.LogWarning($"Tile {name} has no sprite. Expected one at {spritePath}.");
            }

            tile.colliderType = Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);
            return tile;
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

            Debug.LogWarning($"Could not load tile sprite at {spritePath}.");
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
