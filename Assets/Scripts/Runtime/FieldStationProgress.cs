using System;
using System.Collections.Generic;
using Tidepool.Domain;

namespace Tidepool.Runtime
{
    public static class FieldStationProgress
    {
        public static bool IsEarned(FieldStationStage stage, SaveData data)
        {
            if (stage == null || data == null || string.IsNullOrWhiteSpace(stage.UpgradeId))
            {
                return false;
            }

            if (Contains(data.fieldStationUpgradeIds, stage.UpgradeId))
            {
                return true;
            }

            switch (stage.RequirementKind)
            {
                case FieldStationRequirementKind.Always:
                    return true;
                case FieldStationRequirementKind.ChapterCompleted:
                    return Contains(data.completedExpeditionChapterIds, stage.RequirementId);
                case FieldStationRequirementKind.MinimumCaughtSpecies:
                    return CountCaughtSpecies(data) >= stage.MinimumCaughtSpecies;
                case FieldStationRequirementKind.GrowthMemory:
                    return HasGrowthMemory(data);
                case FieldStationRequirementKind.SetPieceCompleted:
                    return Contains(data.completedSetPieceIds, stage.RequirementId);
                case FieldStationRequirementKind.SpeciesCaught:
                    return HasCaughtSpecies(data, stage.RequirementId);
                default:
                    return false;
            }
        }

        public static int Reconcile(FieldStationDefinition definition, GameSaveService saveService)
        {
            if (definition == null || saveService == null || saveService.Data == null)
            {
                return 0;
            }

            int added = 0;
            FieldStationStage[] stages = definition.Stages;
            for (int i = 0; i < stages.Length; i++)
            {
                FieldStationStage stage = stages[i];
                if (IsEarned(stage, saveService.Data)
                    && saveService.RememberFieldStationUpgrade(stage.UpgradeId))
                {
                    added += 1;
                }
            }

            return added;
        }

        public static FieldStationStage FindLatestEarnedStage(FieldStationDefinition definition, SaveData data)
        {
            if (definition == null)
            {
                return null;
            }

            FieldStationStage latest = null;
            FieldStationStage[] stages = definition.Stages;
            for (int i = 0; i < stages.Length; i++)
            {
                if (IsEarned(stages[i], data))
                {
                    latest = stages[i];
                }
            }

            return latest;
        }

        public static bool HasCaughtSpecies(SaveData data, string speciesId)
        {
            if (data?.caught == null || string.IsNullOrWhiteSpace(speciesId))
            {
                return false;
            }

            for (int i = 0; i < data.caught.Count; i++)
            {
                CaughtTideling caught = data.caught[i];
                if (caught != null
                    && string.Equals(caught.speciesId, speciesId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static int CountCaughtSpecies(SaveData data)
        {
            if (data.caught == null)
            {
                return 0;
            }

            HashSet<string> speciesIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < data.caught.Count; i++)
            {
                string speciesId = data.caught[i]?.speciesId;
                if (!string.IsNullOrWhiteSpace(speciesId))
                {
                    speciesIds.Add(speciesId.Trim());
                }
            }

            return speciesIds.Count;
        }

        private static bool HasGrowthMemory(SaveData data)
        {
            if (data.caught == null)
            {
                return false;
            }

            for (int i = 0; i < data.caught.Count; i++)
            {
                List<string> memories = data.caught[i]?.rememberedGrowthFormIds;
                if (memories != null && memories.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Contains(List<string> ids, string id)
        {
            return ids != null
                && !string.IsNullOrWhiteSpace(id)
                && ids.Contains(id.Trim());
        }
    }
}
