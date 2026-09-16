using System;
using UnityEngine;

namespace Tidepool.Domain
{
    [CreateAssetMenu(menuName = "Tidepool/Route Unlock Sequence", fileName = "NewRouteUnlockSequence")]
    public class RouteUnlockSequence : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private ZoneId destinationZone = ZoneId.KelpCurtain;
        [SerializeField] private string completedChapterId;
        [SerializeField] private string nextActiveChapterId;
        [SerializeField] private string landmarkStateId;
        [SerializeField] private string fieldStationUpgradeId;
        [SerializeField] private string completedSetPieceId;
        [SerializeField, TextArea(1, 3)] private string fallbackMessage;

        public string Id => id;
        public ZoneId DestinationZone => destinationZone;
        public string CompletedChapterId => completedChapterId;
        public string NextActiveChapterId => nextActiveChapterId;
        public string LandmarkStateId => landmarkStateId;
        public string FieldStationUpgradeId => fieldStationUpgradeId;
        public string CompletedSetPieceId => completedSetPieceId;
        public string FallbackMessage => fallbackMessage;

        public void Configure(
            string id,
            ZoneId destinationZone,
            string completedChapterId,
            string nextActiveChapterId,
            string landmarkStateId,
            string fieldStationUpgradeId,
            string completedSetPieceId,
            string fallbackMessage)
        {
            this.id = id;
            this.destinationZone = destinationZone;
            this.completedChapterId = completedChapterId;
            this.nextActiveChapterId = nextActiveChapterId;
            this.landmarkStateId = landmarkStateId;
            this.fieldStationUpgradeId = fieldStationUpgradeId;
            this.completedSetPieceId = completedSetPieceId;
            this.fallbackMessage = fallbackMessage;
        }

        public bool IsDurableStateApplied(IRouteUnlockSaveState saveState)
        {
            if (saveState == null)
            {
                return false;
            }

            return saveState.IsZoneUnlocked(destinationZone)
                && HasExpectedId(completedChapterId, saveState.IsExpeditionChapterCompleted)
                && HasExpectedId(landmarkStateId, saveState.HasLandmarkState)
                && HasExpectedId(fieldStationUpgradeId, saveState.HasFieldStationUpgrade)
                && HasExpectedId(completedSetPieceId, saveState.HasCompletedSetPiece);
        }

        public bool ApplyDurableState(IRouteUnlockSaveState saveState)
        {
            if (saveState == null)
            {
                return false;
            }

            bool changed = saveState.UnlockZone(destinationZone);
            changed |= RememberId(completedChapterId, saveState.CompleteExpeditionChapter);
            changed |= RememberId(landmarkStateId, saveState.RememberLandmarkState);
            changed |= RememberId(fieldStationUpgradeId, saveState.RememberFieldStationUpgrade);
            changed |= RememberId(completedSetPieceId, saveState.RememberCompletedSetPiece);
            changed |= RememberId(nextActiveChapterId, saveState.SetActiveExpeditionChapter);
            return changed;
        }

        private static bool HasExpectedId(string id, Func<string, bool> hasId)
        {
            return string.IsNullOrWhiteSpace(id) || hasId(id.Trim());
        }

        private static bool RememberId(string id, Func<string, bool> rememberId)
        {
            return !string.IsNullOrWhiteSpace(id) && rememberId(id.Trim());
        }
    }
}
