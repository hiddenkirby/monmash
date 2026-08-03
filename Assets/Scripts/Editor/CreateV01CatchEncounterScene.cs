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
    public static class CreateV01CatchEncounterScene
    {
        private const string ScenePath = "Assets/Scenes/CatchEncounter.unity";

        [MenuItem("Tools/Tidepool/Create v0.1 CatchEncounter Scene")]
        public static void CreateCatchEncounterScene()
        {
            EnsureFolder("Assets/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject controllerObject = new GameObject("CatchEncounterController");
            CatchEncounterController controller = controllerObject.AddComponent<CatchEncounterController>();

            Canvas canvas = CreateCanvas();
            RectTransform safeArea = CreateRect("SafeArea", canvas.transform);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();

            Image background = CreateImage("Background", safeArea, new Color(0.70f, 0.88f, 0.91f), Vector2.zero, new Vector2(1024f, 768f));
            background.rectTransform.anchorMin = Vector2.zero;
            background.rectTransform.anchorMax = Vector2.one;
            background.rectTransform.offsetMin = Vector2.zero;
            background.rectTransform.offsetMax = Vector2.zero;

            Image creatureImage = CreateImage("CreatureImage", safeArea, Color.white, new Vector2(0f, 125f), new Vector2(300f, 300f));
            creatureImage.preserveAspect = true;

            Text creatureName = CreateText("CreatureName", safeArea, "Tideling", 36, TextAnchor.MiddleCenter, new Vector2(0f, -50f), new Vector2(520f, 56f));
            creatureName.color = new Color(0.08f, 0.18f, 0.22f);

            RectTransform calmBarTrack = CreateRect("CalmBarTrack", safeArea);
            calmBarTrack.anchoredPosition = new Vector2(0f, -150f);
            calmBarTrack.sizeDelta = new Vector2(560f, 44f);
            Image calmBarImage = calmBarTrack.gameObject.AddComponent<Image>();
            calmBarImage.color = new Color(0.87f, 0.96f, 0.88f);

            RectTransform steadyZone = CreateRect("SteadyZone", calmBarTrack);
            steadyZone.anchoredPosition = Vector2.zero;
            steadyZone.sizeDelta = new Vector2(190f, 44f);
            Image steadyZoneImage = steadyZone.gameObject.AddComponent<Image>();
            steadyZoneImage.color = new Color(0.38f, 0.74f, 0.54f);

            RectTransform marker = CreateRect("Marker", calmBarTrack);
            marker.anchoredPosition = Vector2.zero;
            marker.sizeDelta = new Vector2(18f, 64f);
            Image markerImage = marker.gameObject.AddComponent<Image>();
            markerImage.color = new Color(0.05f, 0.18f, 0.22f);

            Image[] pips = new Image[3];
            for (int i = 0; i < pips.Length; i++)
            {
                pips[i] = CreateImage($"JarPip{i + 1}", safeArea, new Color(0.18f, 0.48f, 0.68f), new Vector2(-48f + i * 48f, -218f), new Vector2(30f, 30f));
            }

            Text resultText = CreateText("ResultText", safeArea, string.Empty, 28, TextAnchor.MiddleCenter, new Vector2(0f, -272f), new Vector2(640f, 48f));
            resultText.color = new Color(0.08f, 0.18f, 0.22f);

            Button letGoButton = CreateButton("LetGoButton", safeArea, "Let it go", new Vector2(0f, -340f), new Vector2(220f, 96f));

            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("creatureImage").objectReferenceValue = creatureImage;
            serializedController.FindProperty("creatureNameText").objectReferenceValue = creatureName;
            serializedController.FindProperty("calmBarTrack").objectReferenceValue = calmBarTrack;
            serializedController.FindProperty("steadyZone").objectReferenceValue = steadyZone;
            serializedController.FindProperty("marker").objectReferenceValue = marker;
            SerializedProperty jarPips = serializedController.FindProperty("jarPips");
            jarPips.arraySize = pips.Length;
            for (int i = 0; i < pips.Length; i++)
            {
                jarPips.GetArrayElementAtIndex(i).objectReferenceValue = pips[i];
            }

            serializedController.FindProperty("resultText").objectReferenceValue = resultText;
            serializedController.FindProperty("letGoButton").objectReferenceValue = letGoButton;
            serializedController.ApplyModifiedProperties();

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

            Text text = CreateText("Label", image.transform, label, 30, TextAnchor.MiddleCenter, Vector2.zero, size);
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
