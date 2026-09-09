using System;
using UnityEngine;

namespace Tidepool.Domain
{
    public enum CoastAtlasNodeState
    {
        Locked,
        Available,
        Current,
        Completed
    }

    [Serializable]
    public class CoastAtlasNode
    {
        [SerializeField] private string id;
        [SerializeField] private ZoneId zone = ZoneId.TidepoolShallows;
        [SerializeField] private string chapterId = ExpeditionStateIds.ChapterShallows;
        [SerializeField] private string requiredChapterId;
        [SerializeField] private string completionSetPieceId;
        [SerializeField] private string displayName = "Tidepool Shallows";
        [SerializeField, TextArea(2, 4)] private string activeObjective = "Look for what the tide left behind.";
        [SerializeField, TextArea(2, 4)] private string completedSummary = "This place remembers your visit.";
        [SerializeField, TextArea(2, 4)] private string lockedHint = "Another path will open when the coast is ready.";
        [SerializeField] private Vector2 normalizedPosition = new Vector2(0.2f, 0.5f);
        [SerializeField] private Sprite markerSprite;

        public string Id => id;
        public ZoneId Zone => zone;
        public string ChapterId => chapterId;
        public string RequiredChapterId => requiredChapterId;
        public string CompletionSetPieceId => completionSetPieceId;
        public string DisplayName => displayName;
        public string ActiveObjective => activeObjective;
        public string CompletedSummary => completedSummary;
        public string LockedHint => lockedHint;
        public Vector2 NormalizedPosition => normalizedPosition;
        public Sprite MarkerSprite => markerSprite;

        public CoastAtlasNode()
        {
        }

        public CoastAtlasNode(
            string nodeId,
            ZoneId nodeZone,
            string nodeChapterId,
            string nodeRequiredChapterId,
            string nodeCompletionSetPieceId,
            string nodeDisplayName,
            string nodeActiveObjective,
            string nodeCompletedSummary,
            string nodeLockedHint,
            Vector2 nodeNormalizedPosition)
        {
            id = nodeId;
            zone = nodeZone;
            chapterId = nodeChapterId;
            requiredChapterId = nodeRequiredChapterId;
            completionSetPieceId = nodeCompletionSetPieceId;
            displayName = nodeDisplayName;
            activeObjective = nodeActiveObjective;
            completedSummary = nodeCompletedSummary;
            lockedHint = nodeLockedHint;
            normalizedPosition = nodeNormalizedPosition;
        }
    }
}
