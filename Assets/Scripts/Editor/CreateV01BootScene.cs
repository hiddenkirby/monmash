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
    public static class CreateV01BootScene
    {
        private const string ScenePath = "Assets/Scenes/Boot.unity";

        [MenuItem("Tools/Tidepool/Create v0.8 Boot Scene")]
        public static void CreateBootScene()
        {
            EnsureFolder("Assets/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject saveServiceObject = new GameObject("GameSaveService");
            saveServiceObject.AddComponent<GameSaveService>();

            GameObject routerObject = new GameObject("BootRouter");
            BootRouter router = routerObject.AddComponent<BootRouter>();
            SerializedObject serializedRouter = new SerializedObject(router);
            serializedRouter.FindProperty("overworldSceneName").stringValue = "Overworld";
            serializedRouter.FindProperty("loadOverworldOnStart").boolValue = false;
            serializedRouter.ApplyModifiedProperties();

            Canvas canvas = CreateCanvas();
            RectTransform safeArea = CreateRect("SafeArea", canvas.transform);
            Stretch(safeArea);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();

            Image sky = CreateImage("Sky", safeArea, new Color(0.73f, 0.91f, 0.93f), Vector2.zero, Vector2.zero);
            Stretch(sky.rectTransform);
            Image water = CreateImage("Water", safeArea, new Color(0.32f, 0.70f, 0.73f), new Vector2(0f, -192f), new Vector2(0f, 384f));
            StretchWidth(water.rectTransform);

            RectTransform distantCoast = CreateLayer("DistantCoast", safeArea, new Color(0.28f, 0.54f, 0.50f), new Vector2(-12f, -92f), new Vector2(1100f, 170f));
            RectTransform midCoast = CreateLayer("MidCoast", safeArea, new Color(0.54f, 0.72f, 0.55f), new Vector2(16f, -188f), new Vector2(1160f, 150f));
            RectTransform foregroundPools = CreateLayer("ForegroundPools", safeArea, new Color(0.16f, 0.52f, 0.58f), new Vector2(-20f, -302f), new Vector2(1200f, 150f));

            GameObject openingRoot = CreateRoot("Opening", safeArea);
            Text title = CreateText("Title", openingRoot.transform, "TIDEPOOL", 64, TextAnchor.MiddleCenter, new Vector2(0f, 212f), new Vector2(760f, 92f));
            title.color = new Color(0.07f, 0.27f, 0.30f);
            Text subtitle = CreateText("Subtitle", openingRoot.transform, "THE GREAT LOW TIDE", 28, TextAnchor.MiddleCenter, new Vector2(0f, 148f), new Vector2(620f, 52f));
            subtitle.color = new Color(0.10f, 0.37f, 0.37f);

            Image openingCard = CreateImage("OpeningCard", openingRoot.transform, new Color(0.95f, 0.98f, 0.91f, 0.94f), new Vector2(0f, -2f), new Vector2(720f, 190f));
            Text openingCopy = CreateText(
                "OpeningCopy",
                openingCard.transform,
                "The tide went farther out than anyone expected.\nA whole coast is waiting to be noticed.",
                28,
                TextAnchor.MiddleCenter,
                Vector2.zero,
                new Vector2(650f, 140f));
            openingCopy.color = new Color(0.08f, 0.25f, 0.27f);

            Button openingContinue = CreateButton("ContinueButton", openingRoot.transform, "Explore the coast", new Vector2(0f, -176f), new Vector2(300f, 96f));
            Button skipButton = CreateButton("SkipButton", openingRoot.transform, "Skip", new Vector2(408f, 314f), new Vector2(160f, 88f));

            GameObject returningRoot = CreateRoot("Returning", safeArea);
            Text welcomeBack = CreateText("WelcomeBack", returningRoot.transform, "Welcome back to the coast", 34, TextAnchor.MiddleCenter, new Vector2(0f, 210f), new Vector2(760f, 60f));
            welcomeBack.color = new Color(0.07f, 0.27f, 0.30f);
            Image returningCard = CreateImage("ReturningCard", returningRoot.transform, new Color(0.95f, 0.98f, 0.91f, 0.94f), new Vector2(0f, 22f), new Vector2(720f, 230f));
            Text returningChapter = CreateText("ChapterTitle", returningCard.transform, "The Coast Opens", 36, TextAnchor.MiddleCenter, new Vector2(0f, 50f), new Vector2(650f, 64f));
            returningChapter.color = new Color(0.08f, 0.31f, 0.31f);
            Text returningSummary = CreateText("ChapterSummary", returningCard.transform, "Small ripples and bright shells are waiting in the shallows.", 25, TextAnchor.MiddleCenter, new Vector2(0f, -42f), new Vector2(640f, 96f));
            returningSummary.color = new Color(0.10f, 0.25f, 0.27f);
            Button returningContinue = CreateButton("ContinueButton", returningRoot.transform, "Continue", new Vector2(0f, -170f), new Vector2(260f, 96f));

            Toggle reducedMotionToggle = CreateToggle("ReducedMotionToggle", safeArea, "Reduce motion", new Vector2(-378f, -314f), new Vector2(260f, 88f));

            GameObject controllerObject = new GameObject("GreatLowTideOpeningController");
            controllerObject.transform.SetParent(safeArea, false);
            GreatLowTideOpeningController controller = controllerObject.AddComponent<GreatLowTideOpeningController>();
            AudioSource audioSource = controllerObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true;

            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("bootRouter").objectReferenceValue = router;
            serializedController.FindProperty("openingRoot").objectReferenceValue = openingRoot;
            serializedController.FindProperty("returningRoot").objectReferenceValue = returningRoot;
            serializedController.FindProperty("returningChapterText").objectReferenceValue = returningChapter;
            serializedController.FindProperty("returningSummaryText").objectReferenceValue = returningSummary;
            serializedController.FindProperty("openingContinueButton").objectReferenceValue = openingContinue;
            serializedController.FindProperty("skipButton").objectReferenceValue = skipButton;
            serializedController.FindProperty("returningContinueButton").objectReferenceValue = returningContinue;
            serializedController.FindProperty("reducedMotionToggle").objectReferenceValue = reducedMotionToggle;
            serializedController.FindProperty("openingAudioSource").objectReferenceValue = audioSource;
            SerializedProperty layers = serializedController.FindProperty("panoramaLayers");
            layers.arraySize = 3;
            layers.GetArrayElementAtIndex(0).objectReferenceValue = distantCoast;
            layers.GetArrayElementAtIndex(1).objectReferenceValue = midCoast;
            layers.GetArrayElementAtIndex(2).objectReferenceValue = foregroundPools;
            serializedController.ApplyModifiedProperties();

            returningRoot.SetActive(false);
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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

        private static GameObject CreateRoot(string name, Transform parent)
        {
            RectTransform root = CreateRect(name, parent);
            Stretch(root);
            return root.gameObject;
        }

        private static RectTransform CreateLayer(string name, Transform parent, Color color, Vector2 position, Vector2 size)
        {
            return CreateImage(name, parent, color, position, size).rectTransform;
        }

        private static Image CreateImage(string name, Transform parent, Color color, Vector2 position, Vector2 size)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text CreateText(string name, Transform parent, string value, int fontSize, TextAnchor alignment, Vector2 position, Vector2 size)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = LoadDefaultFont();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.text = value;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 position, Vector2 size)
        {
            Image image = CreateImage(name, parent, new Color(0.08f, 0.36f, 0.36f), position, size);
            image.raycastTarget = true;
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            Text text = CreateText("Label", image.transform, label, 25, TextAnchor.MiddleCenter, Vector2.zero, size - new Vector2(24f, 16f));
            text.color = Color.white;
            return button;
        }

        private static Toggle CreateToggle(string name, Transform parent, string label, Vector2 position, Vector2 size)
        {
            RectTransform root = CreateRect(name, parent);
            root.anchoredPosition = position;
            root.sizeDelta = size;
            Toggle toggle = root.gameObject.AddComponent<Toggle>();

            Image background = CreateImage("Background", root, new Color(0.95f, 0.98f, 0.91f, 0.95f), new Vector2(-84f, 0f), new Vector2(64f, 64f));
            background.raycastTarget = true;
            Image checkmark = CreateImage("Checkmark", background.transform, new Color(0.08f, 0.45f, 0.40f), Vector2.zero, new Vector2(38f, 38f));
            Text text = CreateText("Label", root, label, 22, TextAnchor.MiddleLeft, new Vector2(42f, 0f), new Vector2(160f, 64f));
            text.color = new Color(0.07f, 0.27f, 0.30f);

            toggle.targetGraphic = background;
            toggle.graphic = checkmark;
            return toggle;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void StretchWidth(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.offsetMin = new Vector2(0f, rect.offsetMin.y);
            rect.offsetMax = new Vector2(0f, rect.offsetMax.y);
        }

        private static Font LoadDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
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
