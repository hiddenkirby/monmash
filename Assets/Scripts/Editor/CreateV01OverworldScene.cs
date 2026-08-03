using System.IO;
using Tidepool.Runtime;
using Tidepool.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace Tidepool.Editor
{
    public static class CreateV01OverworldScene
    {
        private const string ScenePath = "Assets/Scenes/Overworld.unity";
        private const string TileAssetFolder = "Assets/Data/Tiles";
        private const int MinX = -12;
        private const int MaxX = 15;
        private const int MinY = -8;
        private const int MaxY = 8;

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

            GameObject gridObject = new GameObject("Grid");
            Grid grid = gridObject.AddComponent<Grid>();
            grid.cellSize = Vector3.one;

            Tilemap ground = CreateTilemap(gridObject.transform, "Ground", 0);
            Tilemap obstacles = CreateTilemap(gridObject.transform, "Obstacles", 2);
            Tilemap seagrassMap = CreateTilemap(gridObject.transform, "Seagrass", 1);
            obstacles.gameObject.AddComponent<TilemapCollider2D>();

            PaintGround(ground, sand, water, grass);
            PaintObstacles(obstacles, shrub, crate);
            PaintSeagrass(seagrassMap, seagrass);

            GameObject playerSpawn = new GameObject("PlayerSpawn");
            playerSpawn.transform.position = grid.GetCellCenterWorld(new Vector3Int(-9, 0, 0));

            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = playerSpawn.transform.position;
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

            Canvas canvas = CreateCanvas();
            RectTransform safeArea = CreateRect("SafeArea", canvas.transform);
            safeArea.anchorMin = Vector2.zero;
            safeArea.anchorMax = Vector2.one;
            safeArea.offsetMin = Vector2.zero;
            safeArea.offsetMax = Vector2.zero;
            safeArea.gameObject.AddComponent<SafeAreaFitter>();
            CreateFirstRunGuidance(safeArea);

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

        private static void CreateFirstRunGuidance(RectTransform safeArea)
        {
            Image panel = CreateImage("FirstRunGuidancePanel", safeArea, new Color(0.10f, 0.26f, 0.28f, 0.92f), new Vector2(0f, 282f), new Vector2(720f, 104f));
            FirstRunGuidanceController guidance = panel.gameObject.AddComponent<FirstRunGuidanceController>();

            Text label = CreateText("GuidanceText", panel.transform, "Tap to walk. Look in the seagrass.", 28, TextAnchor.MiddleLeft, new Vector2(-48f, 0f), new Vector2(500f, 72f));
            label.color = Color.white;

            Button dismissButton = CreateButton("DismissButton", panel.transform, "OK", new Vector2(292f, 0f), new Vector2(112f, 88f));
            WireFirstRunGuidance(guidance, panel.gameObject, label, dismissButton);
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

        private static void PaintGround(Tilemap ground, TileBase sand, TileBase water, TileBase grass)
        {
            for (int x = MinX; x <= MaxX; x++)
            {
                for (int y = MinY; y <= MaxY; y++)
                {
                    bool isShallows = x < 1;
                    bool isPoolTile = isShallows && ((x + y) % 5 == 0 || y <= -6 || y >= 7);
                    TileBase tile = isShallows ? (isPoolTile ? water : sand) : grass;
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
            for (int x = 3; x <= 12; x++)
            {
                for (int y = -5; y <= 5; y++)
                {
                    if ((x + y) % 2 == 0)
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

            tile.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            tile.colliderType = Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);
            return tile;
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
