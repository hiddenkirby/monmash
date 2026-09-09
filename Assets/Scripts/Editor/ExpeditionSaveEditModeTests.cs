using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Tidepool.Domain;
using Tidepool.Runtime;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class ExpeditionSaveEditModeTests
    {
        private static readonly MethodInfo NormalizeLoadedDataMethod = typeof(GameSaveService).GetMethod(
            "NormalizeLoadedData",
            BindingFlags.Instance | BindingFlags.NonPublic);

        [MenuItem("Tools/Tidepool/Verify Expedition Save Foundation")]
        public static void RunAll()
        {
            VerifyNewSaveDefaults();
            WithSaveService(VerifyLegacyMigration);
            WithSaveService(VerifyIdempotentAutosaveAndRoundTrip);
            Debug.Log("Expedition save edit-mode tests passed: defaults, legacy migration, idempotent writes, unknown IDs, and JSON round trip.");
        }

        private static void VerifyNewSaveDefaults()
        {
            SaveData data = new SaveData();
            Require(data.schemaVersion == 3, "New saves must use schema version 3.");
            Require(data.activeExpeditionChapterId == ExpeditionStateIds.ChapterShallows, "New saves must begin in the Shallows chapter.");
            Require(data.completedExpeditionChapterIds != null, "New chapter state must be initialized.");
            Require(data.authoredDiscoveryIds != null, "New discovery state must be initialized.");
            Require(data.landmarkStateIds != null, "New landmark state must be initialized.");
            Require(data.fieldStationUpgradeIds != null, "New field-station state must be initialized.");
            Require(data.completedSetPieceIds != null, "New set-piece state must be initialized.");
        }

        private static void VerifyLegacyMigration(GameSaveService saveService, string savePath)
        {
            SaveData legacy = new SaveData
            {
                schemaVersion = 2,
                caught = new List<CaughtTideling>
                {
                    new CaughtTideling
                    {
                        speciesId = "blip",
                        nickname = "Ripple",
                        caughtAtUtc = "2026-08-28T12:00:00.0000000Z",
                        caughtInZone = ZoneId.SeagrassMeadow,
                        timesSeen = 3,
                        level = 7,
                        levelProgress = 2,
                        activeGrowthFormId = "growth-form-5",
                        rememberedGrowthFormIds = new List<string> { "growth-form-5" }
                    }
                },
                seenSpeciesIds = new List<string> { "blip" },
                triggeredStoryBeatIds = new List<string> { "kelp_unlock" },
                completedQuestIds = new List<string> { "look_in_kelp" },
                unlockedZoneIds = new List<ZoneId>
                {
                    ZoneId.TidepoolShallows,
                    ZoneId.SeagrassMeadow,
                    ZoneId.KelpCurtain
                },
                playerTile = new SerializableVector2Int(17, 4),
                currentZone = ZoneId.KelpCurtain,
                activeExpeditionChapterId = null,
                completedExpeditionChapterIds = null,
                authoredDiscoveryIds = null,
                landmarkStateIds = null,
                fieldStationUpgradeIds = null,
                completedSetPieceIds = null
            };

            SetData(saveService, legacy);
            NormalizeLoadedDataMethod.Invoke(saveService, null);

            SaveData migrated = saveService.Data;
            Require(migrated.schemaVersion == 3, "Schema 2 saves must migrate to schema 3.");
            Require(migrated.caught.Count == 1 && migrated.caught[0].nickname == "Ripple", "Migration must preserve catches and nicknames.");
            Require(migrated.caught[0].level == 7 && migrated.caught[0].activeGrowthFormId == "growth-form-5", "Migration must preserve levels and growth memories.");
            Require(migrated.triggeredStoryBeatIds.Contains("kelp_unlock"), "Migration must preserve story progress.");
            Require(migrated.completedQuestIds.Contains("look_in_kelp"), "Migration must preserve goal progress.");
            Require(migrated.unlockedZoneIds.Contains(ZoneId.KelpCurtain), "Migration must preserve zone unlocks.");
            Require(migrated.playerTile.x == 17 && migrated.playerTile.y == 4, "Migration must preserve player position.");
            Require(migrated.activeExpeditionChapterId == ExpeditionStateIds.ChapterKelp, "Migration must infer the active chapter from the current zone.");
            Require(migrated.completedExpeditionChapterIds != null && migrated.completedSetPieceIds != null, "Migration must initialize all v0.8 lists.");
        }

        private static void VerifyIdempotentAutosaveAndRoundTrip(GameSaveService saveService, string savePath)
        {
            SaveData data = new SaveData
            {
                activeExpeditionChapterId = ExpeditionStateIds.ChapterMeadow,
                authoredDiscoveryIds = new List<string> { "  discovery.unknown-kept  ", "discovery.unknown-kept", "", null },
                landmarkStateIds = new List<string> { ExpeditionStateIds.LandmarkShallowsArch }
            };
            SetData(saveService, data);
            NormalizeLoadedDataMethod.Invoke(saveService, null);

            Require(saveService.Data.authoredDiscoveryIds.Count == 1, "Normalization must trim, drop blank IDs, and remove duplicates.");
            Require(saveService.HasAuthoredDiscovery("discovery.unknown-kept"), "Unknown IDs must remain queryable after normalization.");

            bool firstWrite = saveService.RememberCompletedSetPiece(ExpeditionStateIds.SetPieceOpening);
            Require(firstWrite && File.Exists(savePath), "The first state change must autosave immediately.");
            string firstJson = File.ReadAllText(savePath);
            bool duplicateWrite = saveService.RememberCompletedSetPiece(ExpeditionStateIds.SetPieceOpening);
            string duplicateJson = File.ReadAllText(savePath);
            Require(!duplicateWrite && firstJson == duplicateJson, "Duplicate writes must be idempotent.");

            Require(saveService.CompleteExpeditionChapter(ExpeditionStateIds.ChapterShallows), "A new chapter completion must be recorded.");
            Require(saveService.RememberLandmarkState(ExpeditionStateIds.LandmarkMeadowWave), "A new landmark state must be recorded.");
            Require(saveService.RememberFieldStationUpgrade(ExpeditionStateIds.StationMeadow), "A new station state must be recorded.");
            Require(saveService.SetActiveExpeditionChapter(ExpeditionStateIds.ChapterKelp), "The active chapter must change.");

            saveService.Load();
            Require(saveService.Data.activeExpeditionChapterId == ExpeditionStateIds.ChapterKelp, "The active chapter must round-trip.");
            Require(saveService.IsExpeditionChapterCompleted(ExpeditionStateIds.ChapterShallows), "Chapter completion must round-trip.");
            Require(saveService.HasAuthoredDiscovery("discovery.unknown-kept"), "Unknown discovery IDs must round-trip.");
            Require(saveService.HasLandmarkState(ExpeditionStateIds.LandmarkMeadowWave), "Landmark state must round-trip.");
            Require(saveService.HasFieldStationUpgrade(ExpeditionStateIds.StationMeadow), "Station state must round-trip.");
            Require(saveService.HasCompletedSetPiece(ExpeditionStateIds.SetPieceOpening), "Set-piece state must round-trip.");
        }

        private static void WithSaveService(Action<GameSaveService, string> verification)
        {
            string saveFileName = $"expedition-save-edit-mode-{Guid.NewGuid():N}.json";
            string savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            GameSaveService previousInstance = GameSaveService.Instance;
            GameObject saveObject = new GameObject("ExpeditionSaveEditModeTestService");

            try
            {
                GameSaveService saveService = saveObject.AddComponent<GameSaveService>();
                SetPrivateField(saveService, "saveFileName", saveFileName);
                SetStaticAutoProperty("Instance", saveService);
                verification(saveService, savePath);
            }
            finally
            {
                SetStaticAutoProperty("Instance", previousInstance);
                UnityEngine.Object.DestroyImmediate(saveObject);
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }
            }
        }

        private static void SetData(GameSaveService saveService, SaveData data)
        {
            SetPrivateField(saveService, "<Data>k__BackingField", data);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(target.GetType().Name, fieldName);
            }

            field.SetValue(target, value);
        }

        private static void SetStaticAutoProperty(string propertyName, object value)
        {
            FieldInfo field = typeof(GameSaveService).GetField(
                $"<{propertyName}>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(typeof(GameSaveService).Name, propertyName);
            }

            field.SetValue(null, value);
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
