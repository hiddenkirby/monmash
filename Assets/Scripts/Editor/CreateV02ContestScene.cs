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
        private const string WinChimePath = "Assets/Audio/catch_chime.wav";

        [MenuItem("Tools/Tidepool/Create v0.2 Contest Scene")]
        public static void CreateContestScene()
        {
            EnsureFolder("Assets/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject controllerObject = new GameObject("ContestFlowController");
            ContestFlowController controller = controllerObject.AddComponent<ContestFlowController>();
            AudioSource controllerAudioSource = controllerObject.AddComponent<AudioSource>();
            controllerAudioSource.playOnAwake = false;
            controllerAudioSource.spatialBlend = 0f;

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

            Image telegraphBadge = CreateImage("VisitingTelegraphBadge", safeArea, new Color(1f, 0.97f, 0.89f, 0.94f), new Vector2(300f, 318f), new Vector2(284f, 64f));
            Image telegraphCategoryBadge = CreateTelegraphCategoryBadge(telegraphBadge.transform, out Text telegraphCategoryIconText);
            Text visitingTelegraphText = CreateText("VisitingTelegraphText", telegraphBadge.transform, "Watching...", 18, TextAnchor.MiddleLeft, new Vector2(44f, 0f), new Vector2(218f, 48f));
            visitingTelegraphText.color = new Color(0.08f, 0.18f, 0.22f);

            Image currentRingPanel = CreateImage("CurrentRingPanel", safeArea, new Color(0.95f, 0.98f, 0.94f, 0.92f), new Vector2(0f, 318f), new Vector2(360f, 104f));
            Image[] currentRingNodes = new Image[5];
            Text[] currentRingLabels = new Text[5];
            CreateCurrentRingNode(currentRingPanel.transform, 0, "Current", new Vector2(0f, 28f), new Color(0.18f, 0.52f, 0.72f), currentRingNodes, currentRingLabels);
            CreateCurrentRingNode(currentRingPanel.transform, 1, "Coral", new Vector2(86f, 8f), new Color(0.86f, 0.42f, 0.47f), currentRingNodes, currentRingLabels);
            CreateCurrentRingNode(currentRingPanel.transform, 2, "Stone", new Vector2(52f, -30f), new Color(0.46f, 0.44f, 0.40f), currentRingNodes, currentRingLabels);
            CreateCurrentRingNode(currentRingPanel.transform, 3, "Glow", new Vector2(-52f, -30f), new Color(0.84f, 0.65f, 0.22f), currentRingNodes, currentRingLabels);
            CreateCurrentRingNode(currentRingPanel.transform, 4, "Tide", new Vector2(-86f, 8f), new Color(0.23f, 0.59f, 0.55f), currentRingNodes, currentRingLabels);
            Text currentAdvantageText = CreateText("CurrentAdvantageText", currentRingPanel.transform, "Currents are even.", 18, TextAnchor.MiddleCenter, new Vector2(0f, -48f), new Vector2(320f, 26f));
            currentAdvantageText.color = new Color(0.08f, 0.18f, 0.22f);

            Text roundCounterText = CreateText("RoundCounterText", safeArea, "Round 1 of 3 - You 0, Visitor 0", 26, TextAnchor.MiddleCenter, new Vector2(0f, -20f), new Vector2(720f, 48f));
            roundCounterText.color = new Color(0.08f, 0.18f, 0.22f);

            Text resultText = CreateText("RoundResultText", safeArea, "Pick a friendly move.", 28, TextAnchor.MiddleCenter, new Vector2(0f, -88f), new Vector2(720f, 56f));
            resultText.color = new Color(0.08f, 0.18f, 0.22f);
            Text contestResultText = CreateText("ContestResultText", safeArea, string.Empty, 24, TextAnchor.MiddleCenter, new Vector2(0f, -144f), new Vector2(720f, 48f));
            contestResultText.color = new Color(0.08f, 0.18f, 0.22f);

            Button firstMoveButton = CreateButton("FirstMoveButton", safeArea, "Move 1", new Vector2(-190f, -220f), new Vector2(300f, 96f), new Color(0.12f, 0.44f, 0.50f), Color.white);
            Text firstMoveLabel = firstMoveButton.transform.Find("Label")?.GetComponent<Text>();
            LayoutMoveButtonLabel(firstMoveLabel);
            Image firstMoveCategoryBadge = CreateMoveCategoryBadge(firstMoveButton.transform, "FirstMoveCategoryBadge", out Text firstMoveCategoryText);

            Button secondMoveButton = CreateButton("SecondMoveButton", safeArea, "Move 2", new Vector2(190f, -220f), new Vector2(300f, 96f), new Color(0.12f, 0.44f, 0.50f), Color.white);
            Text secondMoveLabel = secondMoveButton.transform.Find("Label")?.GetComponent<Text>();
            LayoutMoveButtonLabel(secondMoveLabel);
            Image secondMoveCategoryBadge = CreateMoveCategoryBadge(secondMoveButton.transform, "SecondMoveCategoryBadge", out Text secondMoveCategoryText);

            Button[] playerPartyButtons = new Button[3];
            Image[] playerPartyImages = new Image[3];
            Text[] playerPartyLabels = new Text[3];
            for (int i = 0; i < playerPartyButtons.Length; i++)
            {
                CreatePartySlot(safeArea, i, playerPartyButtons, playerPartyImages, playerPartyLabels);
            }

            Button retryButton = CreateButton("RetryButton", safeArea, "Retry", new Vector2(-424f, -332f), new Vector2(152f, 88f), new Color(0.78f, 0.92f, 0.76f), new Color(0.06f, 0.16f, 0.18f));
            Button exitButton = CreateButton("ExitButton", safeArea, "Back", new Vector2(424f, -332f), new Vector2(152f, 88f), new Color(0.78f, 0.92f, 0.76f), new Color(0.06f, 0.16f, 0.18f));

            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("playerImage").objectReferenceValue = playerImage;
            serializedController.FindProperty("playerNameText").objectReferenceValue = playerName;
            serializedController.FindProperty("playerStatusText").objectReferenceValue = playerStatus;
            serializedController.FindProperty("visitingImage").objectReferenceValue = visitingImage;
            serializedController.FindProperty("visitingNameText").objectReferenceValue = visitingName;
            serializedController.FindProperty("visitingStatusText").objectReferenceValue = visitingStatus;
            serializedController.FindProperty("firstMoveButton").objectReferenceValue = firstMoveButton;
            serializedController.FindProperty("firstMoveButtonText").objectReferenceValue = firstMoveLabel;
            serializedController.FindProperty("firstMoveCategoryBadge").objectReferenceValue = firstMoveCategoryBadge;
            serializedController.FindProperty("firstMoveCategoryText").objectReferenceValue = firstMoveCategoryText;
            serializedController.FindProperty("secondMoveButton").objectReferenceValue = secondMoveButton;
            serializedController.FindProperty("secondMoveButtonText").objectReferenceValue = secondMoveLabel;
            serializedController.FindProperty("secondMoveCategoryBadge").objectReferenceValue = secondMoveCategoryBadge;
            serializedController.FindProperty("secondMoveCategoryText").objectReferenceValue = secondMoveCategoryText;
            SetObjectReferenceArray(serializedController.FindProperty("playerPartyButtons"), playerPartyButtons);
            SetObjectReferenceArray(serializedController.FindProperty("playerPartyImages"), playerPartyImages);
            SetObjectReferenceArray(serializedController.FindProperty("playerPartyLabels"), playerPartyLabels);
            serializedController.FindProperty("visitingTelegraphCategoryBadge").objectReferenceValue = telegraphCategoryBadge;
            serializedController.FindProperty("visitingTelegraphCategoryIconText").objectReferenceValue = telegraphCategoryIconText;
            serializedController.FindProperty("visitingTelegraphText").objectReferenceValue = visitingTelegraphText;
            serializedController.FindProperty("roundCounterText").objectReferenceValue = roundCounterText;
            serializedController.FindProperty("resultText").objectReferenceValue = resultText;
            serializedController.FindProperty("contestResultText").objectReferenceValue = contestResultText;
            serializedController.FindProperty("audioSource").objectReferenceValue = controllerAudioSource;
            serializedController.FindProperty("winChimeClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(WinChimePath);
            serializedController.FindProperty("retryButton").objectReferenceValue = retryButton;
            serializedController.FindProperty("exitButton").objectReferenceValue = exitButton;
            serializedController.FindProperty("exitSceneName").stringValue = "Overworld";
            SetObjectReferenceArray(serializedController.FindProperty("currentRingNodes"), currentRingNodes);
            SetObjectReferenceArray(serializedController.FindProperty("currentRingLabels"), currentRingLabels);
            serializedController.FindProperty("currentAdvantageText").objectReferenceValue = currentAdvantageText;
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

        private static void LayoutMoveButtonLabel(Text label)
        {
            if (label == null)
            {
                return;
            }

            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchoredPosition = new Vector2(38f, 0f);
            labelRect.sizeDelta = new Vector2(208f, 84f);
            label.fontSize = 26;
        }

        private static Image CreateMoveCategoryBadge(Transform parent, string name, out Text label)
        {
            Image badge = CreateImage(name, parent, new Color(0.74f, 0.28f, 0.30f), new Vector2(-108f, 0f), new Vector2(64f, 44f));
            label = CreateText("CategoryLabel", badge.transform, "ATK", 16, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(60f, 38f));
            label.color = Color.white;
            return badge;
        }

        private static Image CreateTelegraphCategoryBadge(Transform parent, out Text label)
        {
            Image badge = CreateImage("TelegraphCategoryBadge", parent, new Color(0.74f, 0.28f, 0.30f), new Vector2(-110f, 0f), new Vector2(52f, 52f));
            label = CreateText("Icon", badge.transform, "!", 24, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(48f, 48f));
            label.color = Color.white;
            return badge;
        }

        private static void CreateCurrentRingNode(Transform parent, int index, string label, Vector2 anchoredPosition, Color color, Image[] nodes, Text[] labels)
        {
            Image node = CreateImage($"{label}CurrentNode", parent, color, anchoredPosition, new Vector2(76f, 32f));
            Text text = CreateText("Label", node.transform, label, 14, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(76f, 32f));
            text.color = Color.white;

            nodes[index] = node;
            labels[index] = text;
        }

        private static void CreatePartySlot(Transform parent, int index, Button[] buttons, Image[] images, Text[] labels)
        {
            float x = -220f + (index * 220f);
            Button button = CreateButton($"PlayerPartySlot{index + 1}", parent, "Tideling", new Vector2(x, -332f), new Vector2(188f, 88f), new Color(0.95f, 0.98f, 0.94f), new Color(0.06f, 0.16f, 0.18f));
            Image portrait = CreateImage("Portrait", button.transform, Color.white, new Vector2(-58f, 0f), new Vector2(64f, 64f));
            portrait.preserveAspect = true;

            Text label = button.transform.Find("Label")?.GetComponent<Text>();
            if (label != null)
            {
                RectTransform labelRect = label.GetComponent<RectTransform>();
                labelRect.anchoredPosition = new Vector2(32f, 0f);
                labelRect.sizeDelta = new Vector2(112f, 76f);
                label.fontSize = 18;
            }

            buttons[index] = button;
            images[index] = portrait;
            labels[index] = label;
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

        private static void SetObjectReferenceArray(SerializedProperty property, Object[] values)
        {
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
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
