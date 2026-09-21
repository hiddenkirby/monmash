using System;
using System.Collections.Generic;
using Tidepool.Domain;
using Tidepool.Runtime;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class ExpeditionChapterEditModeTests
    {
        [MenuItem("Tools/Tidepool/Verify Great Low Tide Chapters")]
        public static void RunAll()
        {
            ExpeditionChapter shallows = Chapter(
                ExpeditionStateIds.ChapterShallows,
                ZoneId.TidepoolShallows,
                string.Empty,
                ExpeditionStateIds.LandmarkShallowsArch,
                Array.Empty<string>(),
                0,
                ZoneId.TidepoolShallows,
                1,
                string.Empty,
                ExpeditionStateIds.ChapterMeadow);
            ExpeditionChapter meadow = Chapter(
                ExpeditionStateIds.ChapterMeadow,
                ZoneId.SeagrassMeadow,
                ExpeditionStateIds.ChapterShallows,
                ExpeditionStateIds.LandmarkMeadowWave,
                new[] { "discovery.meadow.gullwing", "discovery.meadow.tanglemaw" },
                1,
                ZoneId.SeagrassMeadow,
                5,
                string.Empty,
                ExpeditionStateIds.ChapterKelp);

            try
            {
                SaveData fresh = new SaveData();
                Require(!ExpeditionChapterProgress.IsCompletionReady(shallows, fresh), "A fresh save should receive the first objective without completing it.");
                Require(ExpeditionChapterProgress.FindActiveChapter(new[] { shallows, meadow }, fresh) == shallows, "A fresh save should point at the Shallows chapter.");

                fresh.landmarkStateIds.Add(ExpeditionStateIds.LandmarkShallowsArch);
                fresh.caught.Add(Caught("blip", ZoneId.TidepoolShallows));
                Require(ExpeditionChapterProgress.IsCompletionReady(shallows, fresh), "The Shallows landmark and first catch should complete chapter one.");

                SaveData deterministicMeadow = new SaveData
                {
                    completedExpeditionChapterIds = new List<string> { ExpeditionStateIds.ChapterShallows },
                    activeExpeditionChapterId = ExpeditionStateIds.ChapterMeadow,
                    landmarkStateIds = new List<string> { ExpeditionStateIds.LandmarkMeadowWave },
                    authoredDiscoveryIds = new List<string> { "discovery.meadow.gullwing" }
                };
                Require(ExpeditionChapterProgress.IsCompletionReady(meadow, deterministicMeadow), "The deterministic Meadow discovery should avoid a random-catch-only gate.");

                SaveData catchRoute = new SaveData
                {
                    completedExpeditionChapterIds = new List<string> { ExpeditionStateIds.ChapterShallows },
                    landmarkStateIds = new List<string> { ExpeditionStateIds.LandmarkMeadowWave }
                };
                for (int i = 0; i < 5; i++)
                {
                    catchRoute.caught.Add(Caught($"meadow-{i}", ZoneId.SeagrassMeadow));
                }

                Require(ExpeditionChapterProgress.IsCompletionReady(meadow, catchRoute), "Existing Meadow catch progress should remain a valid completion path.");

                SaveData advanced = new SaveData
                {
                    currentZone = ZoneId.KelpCurtain,
                    unlockedZoneIds = new List<ZoneId> { ZoneId.TidepoolShallows, ZoneId.SeagrassMeadow, ZoneId.KelpCurtain }
                };
                Require(ExpeditionChapterProgress.ShouldBackfillCompletion(shallows, advanced), "Advanced saves should backfill the Shallows chapter.");
                Require(ExpeditionChapterProgress.ShouldBackfillCompletion(meadow, advanced), "Advanced saves should backfill the Meadow chapter without replaying it.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(shallows);
                UnityEngine.Object.DestroyImmediate(meadow);
            }

            Debug.Log("Great Low Tide chapter edit-mode tests passed: active goal, deterministic route, catch route, and advanced-save backfill.");
        }

        private static ExpeditionChapter Chapter(
            string id,
            ZoneId zone,
            string requiredChapterId,
            string requiredLandmarkId,
            string[] discoveries,
            int minimumDiscoveries,
            ZoneId catchZone,
            int minimumCatches,
            string completionSpeciesId,
            string nextChapterId)
        {
            ExpeditionChapter chapter = ScriptableObject.CreateInstance<ExpeditionChapter>();
            chapter.Configure(
                id,
                zone,
                id,
                string.Empty,
                "Look here.",
                "The next path is ready.",
                requiredChapterId,
                requiredLandmarkId,
                discoveries,
                minimumDiscoveries,
                catchZone,
                minimumCatches,
                completionSpeciesId,
                nextChapterId);
            return chapter;
        }

        private static CaughtTideling Caught(string speciesId, ZoneId zone)
        {
            return new CaughtTideling { speciesId = speciesId, caughtInZone = zone };
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
