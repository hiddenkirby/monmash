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
    public static class CoastFestivalEditModeTests
    {
        [MenuItem("Tools/Tidepool/Verify Coast Festival Projection")]
        public static void RunAll()
        {
            VerifyEligibilityAndReplay();
            VerifyOutcomeEvent();
            VerifyWinProjection();
            Debug.Log("Coast festival tests passed: expedition gating, replay, explicit contest outcomes, win-only completion, ribbon backfill, and no-loss preservation.");
        }

        private static void VerifyEligibilityAndReplay()
        {
            CoastFestivalDefinition definition = ScriptableObject.CreateInstance<CoastFestivalDefinition>();
            try
            {
                definition.Configure();
                SaveData data = EligibleData();
                Require(CoastFestivalProgress.CanEnter(definition, data), "The festival must open after the Kelp chapter and Rocky route reveal.");
                data.completedSetPieceIds.Add(ExpeditionStateIds.SetPieceFestival);
                Require(CoastFestivalProgress.CanEnter(definition, data), "A completed festival must remain replayable.");
                Require(CoastFestivalProgress.IsCompleted(data), "The festival set-piece ID must be the completion authority.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(definition);
            }
        }

        private static void VerifyOutcomeEvent()
        {
            ContestOutcome observed = ContestOutcome.Tie;
            void Observe(ContestOutcome outcome) => observed = outcome;
            ContestEvents.ContestResolved += Observe;
            try
            {
                ContestEvents.RaiseContestResolved(ContestOutcome.PlayerWin);
                Require(observed == ContestOutcome.PlayerWin, "Festival presentation must observe the existing contest result without recalculating it.");
            }
            finally
            {
                ContestEvents.ContestResolved -= Observe;
            }
        }

        private static void VerifyWinProjection()
        {
            string saveFileName = $"coast-festival-edit-mode-{Guid.NewGuid():N}.json";
            string savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            GameSaveService previous = GameSaveService.Instance;
            GameObject saveObject = new GameObject("CoastFestivalSaveTest");
            try
            {
                SetStaticInstance(null);
                GameSaveService saveService = saveObject.AddComponent<GameSaveService>();
                SetPrivateField(saveService, "saveFileName", saveFileName);
                SaveData data = EligibleData();
                data.caught.Add(new CaughtTideling { speciesId = "blip", nickname = "Pebble" });
                SetPrivateField(saveService, "<Data>k__BackingField", data);

                Require(!CoastFestivalProgress.IsCompleted(data), "A loss or tie must not complete the festival.");
                Require(data.caught.Count == 1 && data.caught[0].nickname == "Pebble", "Festival projection must not remove or rewrite collection progress.");
                Require(CoastFestivalProgress.RememberWin(saveService) == 2, "The first win must persist the festival and ribbon facts.");
                Require(CoastFestivalProgress.RememberWin(saveService) == 0, "Repeated wins must be idempotent.");
                Require(CoastFestivalProgress.Reconcile(saveService) == 0, "An earned ribbon must not duplicate during backfill.");

                data.fieldStationUpgradeIds.Clear();
                Require(CoastFestivalProgress.Reconcile(saveService) == 1, "Existing festival wins must backfill the field-station ribbon.");
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
                completedSetPieceIds = new List<string> { ExpeditionStateIds.SetPieceRockyUnlock }
            };
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
