using System.Collections.Generic;
using Tidepool.Domain;

namespace Tidepool.Runtime
{
    public static class CoastFestivalProgress
    {
        public static bool CanEnter(CoastFestivalDefinition definition, SaveData data)
        {
            return definition != null
                && data != null
                && Contains(data.completedExpeditionChapterIds, definition.RequiredCompletedChapterId)
                && Contains(data.completedSetPieceIds, definition.RequiredCompletedSetPieceId);
        }

        public static bool IsCompleted(SaveData data)
        {
            return Contains(data?.completedSetPieceIds, ExpeditionStateIds.SetPieceFestival);
        }

        public static int Reconcile(GameSaveService saveService)
        {
            if (saveService == null || !IsCompleted(saveService.Data))
            {
                return 0;
            }

            return saveService.RememberFieldStationUpgrade(ExpeditionStateIds.StationFestivalRibbon) ? 1 : 0;
        }

        public static int RememberWin(GameSaveService saveService)
        {
            if (saveService == null)
            {
                return 0;
            }

            int changes = saveService.RememberCompletedSetPiece(ExpeditionStateIds.SetPieceFestival) ? 1 : 0;
            changes += saveService.RememberFieldStationUpgrade(ExpeditionStateIds.StationFestivalRibbon) ? 1 : 0;
            return changes;
        }

        private static bool Contains(List<string> ids, string id)
        {
            return ids != null
                && !string.IsNullOrWhiteSpace(id)
                && ids.Contains(id.Trim());
        }
    }
}
