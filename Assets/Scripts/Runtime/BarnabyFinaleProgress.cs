using System;
using System.Collections.Generic;
using Tidepool.Domain;

namespace Tidepool.Runtime
{
    public static class BarnabyFinaleProgress
    {
        public const string SpeciesId = "old-barnaby";

        public static bool IsAvailable(BarnabyFinaleDefinition definition, SaveData data)
        {
            return definition != null
                && definition.Species != null
                && data != null
                && !HasCaughtBarnaby(data)
                && Contains(data.completedExpeditionChapterIds, definition.RequiredCompletedChapterId)
                && Contains(data.landmarkStateIds, definition.RequiredLandmarkStateId);
        }

        public static bool HasCaughtBarnaby(SaveData data)
        {
            if (data?.caught == null)
            {
                return false;
            }

            for (int i = 0; i < data.caught.Count; i++)
            {
                if (string.Equals(data.caught[i]?.speciesId, SpeciesId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsCompleted(SaveData data)
        {
            return HasCaughtBarnaby(data)
                && Contains(data.completedSetPieceIds, ExpeditionStateIds.SetPieceFinale);
        }

        public static int ReconcileCompletion(GameSaveService saveService)
        {
            if (saveService == null || !HasCaughtBarnaby(saveService.Data))
            {
                return 0;
            }

            int changes = 0;
            changes += Remember(saveService.RememberAuthoredDiscovery(ExpeditionStateIds.DiscoveryRockyApproach));
            changes += Remember(saveService.RememberAuthoredDiscovery(ExpeditionStateIds.FinaleBarnabyMet));
            changes += Remember(saveService.RememberCompletedSetPiece(ExpeditionStateIds.SetPieceFinale));
            changes += Remember(saveService.CompleteExpeditionChapter(ExpeditionStateIds.ChapterRocky));
            changes += Remember(saveService.RememberFieldStationUpgrade(ExpeditionStateIds.StationFinale));
            return changes;
        }

        public static bool RememberApproach(GameSaveService saveService)
        {
            return saveService != null
                && saveService.RememberAuthoredDiscovery(ExpeditionStateIds.DiscoveryRockyApproach);
        }

        public static bool RememberAttempt(GameSaveService saveService)
        {
            if (saveService == null)
            {
                return false;
            }

            RememberApproach(saveService);
            return saveService.RememberAuthoredDiscovery(ExpeditionStateIds.FinaleBarnabyMet);
        }

        public static bool RememberMemorySeen(GameSaveService saveService)
        {
            return saveService != null
                && IsCompleted(saveService.Data)
                && saveService.RememberAuthoredDiscovery(ExpeditionStateIds.ExpeditionMemorySeen);
        }

        private static int Remember(bool changed)
        {
            return changed ? 1 : 0;
        }

        private static bool Contains(List<string> ids, string id)
        {
            return ids != null
                && !string.IsNullOrWhiteSpace(id)
                && ids.Contains(id.Trim());
        }
    }
}
