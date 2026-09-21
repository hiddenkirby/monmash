using System;
using System.Collections.Generic;
using Tidepool.Domain;
using Tidepool.Runtime;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class FieldStationEditModeTests
    {
        [MenuItem("Tools/Tidepool/Verify Field Station Projection")]
        public static void RunAll()
        {
            FieldStationStage always = Stage(ExpeditionStateIds.StationShallows, FieldStationRequirementKind.Always);
            FieldStationStage meadow = Stage(ExpeditionStateIds.StationMeadow, FieldStationRequirementKind.ChapterCompleted, ExpeditionStateIds.ChapterShallows);
            FieldStationStage discoveries = Stage(ExpeditionStateIds.StationSpeciesBoard, FieldStationRequirementKind.MinimumCaughtSpecies, minimumCaughtSpecies: 2);
            FieldStationStage growth = Stage(ExpeditionStateIds.StationGrowthMemory, FieldStationRequirementKind.GrowthMemory);
            FieldStationStage ribbon = Stage(ExpeditionStateIds.StationFestivalRibbon, FieldStationRequirementKind.SetPieceCompleted, ExpeditionStateIds.SetPieceFestival);
            FieldStationStage finale = Stage(ExpeditionStateIds.StationFinale, FieldStationRequirementKind.SpeciesCaught, "old-barnaby");

            SaveData fresh = new SaveData();
            Require(FieldStationProgress.IsEarned(always, fresh), "The station must be reachable on a fresh save.");
            Require(!FieldStationProgress.IsEarned(meadow, fresh), "The Meadow display must wait for chapter progress.");

            SaveData advanced = new SaveData
            {
                completedExpeditionChapterIds = new List<string> { ExpeditionStateIds.ChapterShallows },
                completedSetPieceIds = new List<string> { ExpeditionStateIds.SetPieceFestival },
                caught = new List<CaughtTideling>
                {
                    Caught("blip"),
                    Caught("nubbin", "grown.nubbin")
                }
            };

            Require(FieldStationProgress.IsEarned(meadow, advanced), "Existing chapter progress must backfill the Meadow display.");
            Require(FieldStationProgress.IsEarned(discoveries, advanced), "Distinct catches must backfill the discovery board.");
            Require(FieldStationProgress.IsEarned(growth, advanced), "A remembered growth form must backfill the album.");
            Require(FieldStationProgress.IsEarned(ribbon, advanced), "The existing festival result must remain authoritative for the ribbon.");
            Require(!FieldStationProgress.IsEarned(finale, advanced), "The finale display must not appear before the finale is complete.");

            advanced.caught.Add(Caught("old-barnaby"));
            Require(FieldStationProgress.IsEarned(finale, advanced), "Existing Old Barnaby catches must backfill the finale display.");
            advanced.caught.RemoveAt(advanced.caught.Count - 1);
            advanced.fieldStationUpgradeIds.Add(ExpeditionStateIds.StationFinale);
            Require(FieldStationProgress.IsEarned(finale, advanced), "Remembered station upgrades must never be revoked after backfill.");
            Require(FieldStationProgress.HasCaughtSpecies(advanced, "BLIP"), "Visitor projection must match caught species without changing collection data.");

            Debug.Log("Field station edit-mode tests passed: fresh access, chapter/species/growth/festival backfill, monotonic upgrades, and read-only visitors.");
        }

        private static FieldStationStage Stage(
            string upgradeId,
            FieldStationRequirementKind requirementKind,
            string requirementId = null,
            int minimumCaughtSpecies = 0)
        {
            return new FieldStationStage(upgradeId, upgradeId, upgradeId, requirementKind, requirementId, minimumCaughtSpecies);
        }

        private static CaughtTideling Caught(string speciesId, string rememberedFormId = null)
        {
            CaughtTideling caught = new CaughtTideling { speciesId = speciesId };
            if (!string.IsNullOrWhiteSpace(rememberedFormId))
            {
                caught.rememberedGrowthFormIds.Add(rememberedFormId);
            }

            return caught;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
