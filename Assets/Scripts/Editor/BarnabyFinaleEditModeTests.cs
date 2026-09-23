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
    public static class BarnabyFinaleEditModeTests
    {
        [MenuItem("Tools/Tidepool/Verify Old Barnaby Finale")]
        public static void RunAll()
        {
            VerifyEligibilityAndRetry();
            VerifyCatchBackfill();
            Debug.Log("Old Barnaby finale tests passed: gated invitation, retry after an attempt, catch-authoritative completion, advanced-save backfill, and idempotent memory state.");
        }

        private static void VerifyEligibilityAndRetry()
        {
            BarnabyFinaleDefinition definition = ScriptableObject.CreateInstance<BarnabyFinaleDefinition>();
            TidelingSpecies species = ScriptableObject.CreateInstance<TidelingSpecies>();
            try
            {
                ConfigureSpecies(species, BarnabyFinaleProgress.SpeciesId);
                definition.Configure(species);
                SaveData data = EligibleData();
                Require(BarnabyFinaleProgress.IsAvailable(definition, data), "The invitation must appear after the Kelp chapter and Rocky landmark.");
                data.authoredDiscoveryIds.Add(ExpeditionStateIds.FinaleBarnabyMet);
                Require(BarnabyFinaleProgress.IsAvailable(definition, data), "Meeting or letting Old Barnaby go must keep the finale retryable.");
                data.caught.Add(new CaughtTideling { speciesId = BarnabyFinaleProgress.SpeciesId });
                Require(!BarnabyFinaleProgress.IsAvailable(definition, data), "An existing Old Barnaby catch must prevent a duplicate finale encounter.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(definition);
                UnityEngine.Object.DestroyImmediate(species);
            }
        }

        private static void VerifyCatchBackfill()
        {
            string saveFileName = $"barnaby-finale-edit-mode-{Guid.NewGuid():N}.json";
            string savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            GameSaveService previous = GameSaveService.Instance;
            GameObject saveObject = new GameObject("BarnabyFinaleSaveTest");
            try
            {
                SetStaticInstance(null);
                GameSaveService saveService = saveObject.AddComponent<GameSaveService>();
                SetPrivateField(saveService, "saveFileName", saveFileName);
                SaveData data = EligibleData();
                data.caught.Add(new CaughtTideling { speciesId = BarnabyFinaleProgress.SpeciesId });
                SetData(saveService, data);

                Require(BarnabyFinaleProgress.ReconcileCompletion(saveService) == 5, "An advanced save must receive every earned finale state.");
                Require(BarnabyFinaleProgress.IsCompleted(data), "The existing catch must remain authoritative for completion.");
                Require(data.completedExpeditionChapterIds.Contains(ExpeditionStateIds.ChapterRocky), "Finale completion must finish the Rocky chapter.");
                Require(data.fieldStationUpgradeIds.Contains(ExpeditionStateIds.StationFinale), "Finale completion must unlock the station celebration.");
                Require(BarnabyFinaleProgress.ReconcileCompletion(saveService) == 0, "Backfill must be idempotent.");
                Require(BarnabyFinaleProgress.RememberMemorySeen(saveService), "A completed save may acknowledge the shortened memory once.");
                Require(!BarnabyFinaleProgress.RememberMemorySeen(saveService), "Memory acknowledgement must be idempotent.");
                Require(data.caught.Count == 1, "Finale projection must never duplicate the authoritative catch.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(saveObject);
                SetStaticInstance(previous);
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }
            }
        }

        private static SaveData EligibleData()
        {
            return new SaveData
            {
                completedExpeditionChapterIds = new List<string> { ExpeditionStateIds.ChapterKelp },
                landmarkStateIds = new List<string> { ExpeditionStateIds.LandmarkRockyOldStones }
            };
        }

        private static void ConfigureSpecies(TidelingSpecies species, string speciesId)
        {
            FieldInfo field = typeof(TidelingSpecies).GetField("id", BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(species, speciesId);
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

        private static void SetStaticInstance(GameSaveService value)
        {
            FieldInfo field = typeof(GameSaveService).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(typeof(GameSaveService).Name, "Instance");
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
