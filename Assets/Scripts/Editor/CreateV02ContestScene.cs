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
    public static class CreateV02ContestScene
    {
        private const string ScenePath = "Assets/Scenes/Contest.unity";

        [MenuItem("Tools/Tidepool/Create v0.2 Contest Scene")]
        public static void CreateContestScene()
        {
            EnsureFolder("Assets/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject controllerObject = new GameObject("ContestFlowController");
            ContestFlowController controller = controllerObject.AddComponent<ContestFlowController>();

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

            Image playerPanel = CreatePanel("PlayerTideling", safeArea, new Vector2(-270f, 132f));
            Image playerImage = CreateImage("PlayerImage", playerPanel.transform, Color.white, new Vector2(0f, 42f), new Vector2(210f, 210f));
            playerImage.preserveAspect = true;
            Text playerName = CreateText("PlayerName", playerPanel.transform, "Tideling", 30, TextAnchor.MiddleCenter, new Vector2(0f, -106f), new Vector2(300f, 56f));
            playerName.color = Color.white;
            Text playerStatus = CreateText("PlayerStatus", playerPanel.transform, string.Empty, 22, TextAnchor.MiddleCenter, new Vector2(0f, -160f), new Vector2(300f, 40f));
            playerStatus.color = new Color(0.5f, 0.5f, 0.5f);

            Image visitingPanel = CreatePanel("VisitingTideling", safeArea, new Vector2(270f, 132f));
            Image visitingImage = CreateImage("VisitingImage", visitingPanel.transform, Color.white, new Vector2(0f, 42f), new Vector2(210f, 210f));
            visitingImage.preserveAspect = true;
            Text visitingName = CreateText("VisitingName", visitingPanel.transform, "Tideling", 30, TextAnchor.MiddleCenter, new Vector2(0f, -106f), new Vector2(300f, 56f));
            visitingName.color = Color.white;
            Text visitingStatus = CreateText("VisitingStatus", visitingPanel.transform, string.Empty, 22, TextAnchor.MiddleCenter, new Vector2(0f, -160f), new Vector2(300f, 40f));
            visitingStatus.color = new Color(0.5f, 0.5f, 0.5f);

            Text resultText = CreateText("ResultText", safeArea, "Pick a friendly move.", 30, TextAnchor.MiddleCenter, new Vector2(0f, -92f), new Vector2(720f, 80f));
            resultText.color = new Color(0.08f, 0.18f, 0.22f);

            Button firstMoveButton = CreateButton("FirstMoveButton", safeArea, "Move 1", new Vector2(-190f, -220f), new Vector2(300f, 96f), new Color(0.12f, 0.44f, 0.50f), Color.white);
            Text firstMoveLabel = firstMoveButton.transform.Find("Label")?.GetComponent<Text>();

            Button secondMoveButton = CreateButton("SecondMoveButton", safeArea, "Move 2", new Vector2(190f, -220f), new Vector2(300f, 96f), new Color(0.12f, 0.44f, 0.50f), Color.white);
            Text secondMoveLabel = secondMoveButton.transform.Find("Label")?.GetComponent<Text>();

            Button retryButton = CreateButton("RetryButton", safeArea, "Retry", new Vector2(-120f, -340f), new Vector2(220f, 96f), new Color(0.78f, 0.92f, 0.76f), new Color(0.06f, 0.16f, 0.18f));
            Button exitButton = CreateButton("ExitButton", safeArea, "Back", new Vector2(120f, -340f), new Vector2(220f, 96f), new Color(0.78f, 0.92f, 0.76f), new Color(0.06f, 0.16f, 0.18f));

            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("playerImage").objectReferenceValue = playerImage;
            serializedController.FindProperty("playerNameText").objectReferenceValue = playerName;
            serializedController.FindProperty("playerStatusText").objectReferenceValue = playerStatus;
            serializedController.FindProperty("visitingImage").objectReferenceValue = visitingImage;
            serializedController.FindProperty("visitingNameText").objectReferenceValue = visitingName;
            serializedController.FindProperty("visitingStatusText").objectReferenceValue = visitingStatus;
            serializedController.FindProperty("firstMoveButton").objectReferenceValue = firstMoveButton;
            serializedController.FindProperty("firstMoveButtonText").objectReferenceValue = firstMoveLabel;
            serializedController.FindProperty("secondMoveButton").objectReferenceValue = secondMoveButton;
            serializedController.FindProperty("secondMoveButtonText").objectReferenceValue = secondMoveLabel;
            serializedController.FindProperty("resultText").objectReferenceValue = resultText;
            serializedController.FindProperty("retryButton").objectReferenceValue = retryButton;
            serializedController.FindProperty("exitButton").objectReferenceValue = exitButton;
            serializedController.FindProperty("exitSceneName").stringValue = "Overworld";
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

        private static Image CreatePanel(string name, Transform parent, Vector2 anchoredPosition)
        {
            Image panel = CreateImage(name, parent, new Color(0.08f, 0.22f, 0.24f, 0.90f), anchoredPosition, new Vector2(340f, 330f));
            return panel;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPosition, Vector2 size, Color backgroundColor, Color textColor)
        {
            Image image = CreateImage(name, parent, backgroundColor, anchoredPosition, size);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            Text text = CreateText("Label", image.transform, label, 30, TextAnchor.MiddleCenter, Vector2.zero, size);
            text.color = textColor;
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
