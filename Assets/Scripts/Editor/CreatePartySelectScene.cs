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
    public static class CreatePartySelectScene
    {
        private const string ScenePath = "Assets/Scenes/PartySelect.unity";
        private const string SpeciesDatabasePath = "Assets/Data/Databases/SpeciesDatabase.asset";

        [MenuItem("Tools/Tidepool/Create Party Select Scene")]
        public static void CreateScene()
        {
            EnsureFolder("Assets/Scenes");

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

            Text titleText = CreateText("TitleText", safeArea, "Pick your Tideling", 36, TextAnchor.MiddleCenter, new Vector2(0f, 280f), new Vector2(600f, 72f));
            titleText.color = new Color(0.08f, 0.18f, 0.22f);

            RectTransform listRoot = CreateRect("ListRoot", safeArea);
            listRoot.anchoredPosition = new Vector2(0f, 60f);
            listRoot.sizeDelta = new Vector2(500f, 360f);
            VerticalLayoutGroup listLayout = listRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            listLayout.spacing = 8f;
            listLayout.childAlignment = TextAnchor.UpperCenter;
            listLayout.childControlWidth = true;
            listLayout.childControlHeight = false;
            listLayout.childForceExpandWidth = true;
            listLayout.childForceExpandHeight = false;

            Button backButton = CreateButton("BackButton", safeArea, "Back", new Vector2(-380f, -310f), new Vector2(160f, 88f));

            PartySelectController controller = safeArea.gameObject.AddComponent<PartySelectController>();
            SpeciesDatabase database = AssetDatabase.LoadAssetAtPath<SpeciesDatabase>(SpeciesDatabasePath);

            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("speciesDatabase").objectReferenceValue = database;
            serializedController.FindProperty("listRoot").objectReferenceValue = listRoot;
            serializedController.FindProperty("entryPrefab").objectReferenceValue = null;
            serializedController.FindProperty("titleText").objectReferenceValue = titleText;
            serializedController.FindProperty("contestSceneName").stringValue = "Contest";
            serializedController.FindProperty("backButton").objectReferenceValue = backButton;
            serializedController.FindProperty("playerMover").objectReferenceValue = null;
            serializedController.ApplyModifiedProperties();

            UnityEditor.Events.UnityEventTools.AddPersistentListener(backButton.onClick, controller.Close);

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

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPosition, Vector2 size)
        {
            Image image = CreateImage(name, parent, new Color(0.12f, 0.44f, 0.50f), anchoredPosition, size);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            Text text = CreateText("Label", image.transform, label, 28, TextAnchor.MiddleCenter, Vector2.zero, size);
            text.color = Color.white;
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
