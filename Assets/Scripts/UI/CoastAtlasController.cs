using Tidepool.Domain;
using Tidepool.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Tidepool.UI
{
    public class CoastAtlasController : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CoastAtlasDefinition atlasDefinition;
        [SerializeField] private Image mapImage;
        [SerializeField] private Text titleText;
        [SerializeField] private Text currentPlaceText;
        [SerializeField] private Text objectiveText;
        [SerializeField] private Text progressText;
        [SerializeField] private Text[] nodeLabels;
        [SerializeField] private Image[] nodeMarkers;
        [SerializeField] private Button closeButton;

        private readonly Color lockedColor = new Color(0.42f, 0.47f, 0.48f, 0.65f);
        private readonly Color availableColor = new Color(0.12f, 0.36f, 0.42f, 0.9f);
        private readonly Color currentColor = new Color(0.05f, 0.48f, 0.55f, 1f);
        private readonly Color completedColor = new Color(0.16f, 0.44f, 0.26f, 1f);

        private void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseAtlas);
            }

            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(CloseAtlas);
            }
        }

        public void OpenAtlas()
        {
            RefreshAtlas();
            SetVisible(true);
        }

        public void CloseAtlas()
        {
            SetVisible(false);
        }

        public void RefreshAtlas()
        {
            SaveData data = GameSaveService.Instance == null ? null : GameSaveService.Instance.Data;
            CoastAtlasNode activeNode = CoastAtlasProgress.FindActiveNode(atlasDefinition, data);
            CoastAtlasNode[] nodes = atlasDefinition == null ? new CoastAtlasNode[0] : atlasDefinition.Nodes;

            SetText(titleText, atlasDefinition == null ? "The Coast" : atlasDefinition.Title);
            SetText(currentPlaceText, activeNode == null ? "Tidepool Shallows" : activeNode.DisplayName);
            SetText(objectiveText, activeNode == null ? GetFallbackObjective() : CoastAtlasProgress.GetNodeSummary(activeNode, data));
            SetText(progressText, $"{CoastAtlasProgress.CountCompletedNodes(atlasDefinition, data)} of {nodes.Length} chapters remembered");

            if (mapImage != null && atlasDefinition != null && atlasDefinition.MapSprite != null)
            {
                mapImage.sprite = atlasDefinition.MapSprite;
                mapImage.enabled = true;
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                RefreshNode(i, nodes[i], data);
            }

            HideUnusedNodeSlots(nodes.Length);
        }

        private void RefreshNode(int index, CoastAtlasNode node, SaveData data)
        {
            if (node == null)
            {
                if (nodeLabels != null && index < nodeLabels.Length && nodeLabels[index] != null)
                {
                    nodeLabels[index].gameObject.SetActive(false);
                }

                if (nodeMarkers != null && index < nodeMarkers.Length && nodeMarkers[index] != null)
                {
                    nodeMarkers[index].gameObject.SetActive(false);
                }

                return;
            }

            CoastAtlasNodeState state = CoastAtlasProgress.GetNodeState(node, data);
            Color stateColor = StateColor(state);
            string labelPrefix = StatePrefix(state);

            if (nodeLabels != null && index < nodeLabels.Length && nodeLabels[index] != null)
            {
                nodeLabels[index].text = $"{labelPrefix} {node.DisplayName}";
                nodeLabels[index].color = stateColor;
                nodeLabels[index].gameObject.SetActive(true);
            }

            if (nodeMarkers == null || index >= nodeMarkers.Length || nodeMarkers[index] == null)
            {
                return;
            }

            Image marker = nodeMarkers[index];
            marker.color = stateColor;
            if (node.MarkerSprite != null)
            {
                marker.sprite = node.MarkerSprite;
            }

            RectTransform markerRect = marker.rectTransform;
            if (markerRect != null)
            {
                markerRect.anchorMin = node.NormalizedPosition;
                markerRect.anchorMax = node.NormalizedPosition;
                markerRect.anchoredPosition = Vector2.zero;
            }

            marker.gameObject.SetActive(true);
        }

        private void HideUnusedNodeSlots(int usedCount)
        {
            if (nodeLabels != null)
            {
                for (int i = usedCount; i < nodeLabels.Length; i++)
                {
                    if (nodeLabels[i] != null)
                    {
                        nodeLabels[i].gameObject.SetActive(false);
                    }
                }
            }

            if (nodeMarkers == null)
            {
                return;
            }

            for (int i = usedCount; i < nodeMarkers.Length; i++)
            {
                if (nodeMarkers[i] != null)
                {
                    nodeMarkers[i].gameObject.SetActive(false);
                }
            }
        }

        private string GetFallbackObjective()
        {
            return atlasDefinition == null
                ? "The coast is waiting to be noticed."
                : atlasDefinition.FallbackObjective;
        }

        private Color StateColor(CoastAtlasNodeState state)
        {
            switch (state)
            {
                case CoastAtlasNodeState.Completed:
                    return completedColor;
                case CoastAtlasNodeState.Current:
                    return currentColor;
                case CoastAtlasNodeState.Available:
                    return availableColor;
                default:
                    return lockedColor;
            }
        }

        private static string StatePrefix(CoastAtlasNodeState state)
        {
            switch (state)
            {
                case CoastAtlasNodeState.Completed:
                    return "done";
                case CoastAtlasNodeState.Current:
                    return "now";
                case CoastAtlasNodeState.Available:
                    return "next";
                default:
                    return "later";
            }
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private void SetVisible(bool visible)
        {
            GameObject target = panelRoot == null ? gameObject : panelRoot;
            target.SetActive(visible);
        }
    }
}
