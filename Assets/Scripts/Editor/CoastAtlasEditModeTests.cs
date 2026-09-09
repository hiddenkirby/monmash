using System;
using System.Collections.Generic;
using Tidepool.Domain;
using Tidepool.Runtime;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class CoastAtlasEditModeTests
    {
        [MenuItem("Tools/Tidepool/Verify Coast Atlas Projection")]
        public static void RunAll()
        {
            CoastAtlasDefinition definition = ScriptableObject.CreateInstance<CoastAtlasDefinition>();
            definition.Configure(
                "Test Atlas",
                "Fallback",
                new[]
                {
                    new CoastAtlasNode(
                        "shallows",
                        ZoneId.TidepoolShallows,
                        ExpeditionStateIds.ChapterShallows,
                        null,
                        ExpeditionStateIds.SetPieceMeadowUnlock,
                        "Shallows",
                        "Start here.",
                        "Shallows done.",
                        "Start here.",
                        new Vector2(0.1f, 0.5f)),
                    new CoastAtlasNode(
                        "meadow",
                        ZoneId.SeagrassMeadow,
                        ExpeditionStateIds.ChapterMeadow,
                        ExpeditionStateIds.ChapterShallows,
                        ExpeditionStateIds.SetPieceKelpUnlock,
                        "Meadow",
                        "Wave path.",
                        "Meadow done.",
                        "Finish shallows first.",
                        new Vector2(0.5f, 0.5f))
                });

            try
            {
                SaveData fresh = new SaveData();
                CoastAtlasNode shallows = definition.FindNode(ZoneId.TidepoolShallows);
                CoastAtlasNode meadow = definition.FindNode(ZoneId.SeagrassMeadow);
                Require(CoastAtlasProgress.GetNodeState(shallows, fresh) == CoastAtlasNodeState.Current, "Fresh saves should point at the Shallows.");
                Require(CoastAtlasProgress.GetNodeState(meadow, fresh) == CoastAtlasNodeState.Locked, "The Meadow should wait for the Shallows chapter.");

                SaveData progressed = new SaveData
                {
                    currentZone = ZoneId.SeagrassMeadow,
                    activeExpeditionChapterId = ExpeditionStateIds.ChapterMeadow,
                    completedExpeditionChapterIds = new List<string> { ExpeditionStateIds.ChapterShallows }
                };
                Require(CoastAtlasProgress.GetNodeState(shallows, progressed) == CoastAtlasNodeState.Completed, "Completed chapters should read as completed.");
                Require(CoastAtlasProgress.GetNodeState(meadow, progressed) == CoastAtlasNodeState.Current, "Active chapters should read as current.");
                Require(CoastAtlasProgress.FindActiveNode(definition, progressed) == meadow, "The active chapter should choose the matching node.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(definition);
            }

            Debug.Log("Coast atlas edit-mode tests passed: locked, current, completed, and active-node projection.");
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
