using System;
using System.Collections.Generic;
using Tidepool.Domain;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class RouteUnlockSequenceEditModeTests
    {
        [MenuItem("Tools/Tidepool/Verify Route Unlock Sequences")]
        public static void RunAll()
        {
            VerifyDurableStateIsAppliedTogether();
            VerifyDuplicateApplyIsIdempotent();
            VerifyBlankOptionalIdsAreIgnored();
            Debug.Log("Route unlock sequence edit-mode tests passed: durable state, idempotent writes, and blank optional IDs.");
        }

        private static void VerifyDurableStateIsAppliedTogether()
        {
            RouteUnlockSequence sequence = CreateKelpSequence();
            FakeRouteUnlockSaveState saveState = new FakeRouteUnlockSaveState();

            Require(!sequence.IsDurableStateApplied(saveState), "A closed route must not report durable state applied.");
            Require(sequence.ApplyDurableState(saveState), "First apply must report a durable state change.");
            Require(sequence.IsDurableStateApplied(saveState), "Applied state must satisfy all route sequence facts.");
            Require(saveState.IsZoneUnlocked(ZoneId.KelpCurtain), "Route sequence must unlock the destination zone.");
            Require(saveState.IsExpeditionChapterCompleted(ExpeditionStateIds.ChapterMeadow), "Route sequence must complete the previous chapter.");
            Require(saveState.HasLandmarkState(ExpeditionStateIds.LandmarkMeadowWave), "Route sequence must remember the related landmark.");
            Require(saveState.HasFieldStationUpgrade(ExpeditionStateIds.StationMeadow), "Route sequence must remember the station update.");
            Require(saveState.HasCompletedSetPiece(ExpeditionStateIds.SetPieceKelpUnlock), "Route sequence must remember the set-piece.");
            Require(saveState.ActiveChapterId == ExpeditionStateIds.ChapterKelp, "Route sequence must advance the active chapter.");
            UnityEngine.Object.DestroyImmediate(sequence);
        }

        private static void VerifyDuplicateApplyIsIdempotent()
        {
            RouteUnlockSequence sequence = CreateKelpSequence();
            FakeRouteUnlockSaveState saveState = new FakeRouteUnlockSaveState();

            sequence.ApplyDurableState(saveState);
            int writeCount = saveState.WriteCount;
            Require(!sequence.ApplyDurableState(saveState), "Duplicate apply must not report a new durable state change.");
            Require(saveState.WriteCount == writeCount, "Duplicate apply must not write duplicate state.");
            UnityEngine.Object.DestroyImmediate(sequence);
        }

        private static void VerifyBlankOptionalIdsAreIgnored()
        {
            RouteUnlockSequence sequence = ScriptableObject.CreateInstance<RouteUnlockSequence>();
            sequence.Configure(
                "route-unlock.test",
                ZoneId.RockyShelf,
                " ",
                null,
                string.Empty,
                "station.test",
                null,
                "The path opens.");
            FakeRouteUnlockSaveState saveState = new FakeRouteUnlockSaveState();

            Require(sequence.ApplyDurableState(saveState), "Route unlock with one real optional ID must still apply.");
            Require(saveState.IsZoneUnlocked(ZoneId.RockyShelf), "Route unlock must always unlock its destination.");
            Require(saveState.HasFieldStationUpgrade("station.test"), "Nonblank optional IDs must be saved.");
            Require(sequence.IsDurableStateApplied(saveState), "Blank optional IDs must not block durable-state checks.");
            UnityEngine.Object.DestroyImmediate(sequence);
        }

        private static RouteUnlockSequence CreateKelpSequence()
        {
            RouteUnlockSequence sequence = ScriptableObject.CreateInstance<RouteUnlockSequence>();
            sequence.Configure(
                "route-unlock.kelp",
                ZoneId.KelpCurtain,
                ExpeditionStateIds.ChapterMeadow,
                ExpeditionStateIds.ChapterKelp,
                ExpeditionStateIds.LandmarkMeadowWave,
                ExpeditionStateIds.StationMeadow,
                ExpeditionStateIds.SetPieceKelpUnlock,
                "The kelp parts into a quiet path.");
            return sequence;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private sealed class FakeRouteUnlockSaveState : IRouteUnlockSaveState
        {
            private readonly HashSet<ZoneId> unlockedZones = new HashSet<ZoneId>();
            private readonly HashSet<string> completedChapters = new HashSet<string>();
            private readonly HashSet<string> landmarkStates = new HashSet<string>();
            private readonly HashSet<string> fieldStationUpgrades = new HashSet<string>();
            private readonly HashSet<string> completedSetPieces = new HashSet<string>();

            public string ActiveChapterId { get; private set; }
            public int WriteCount { get; private set; }

            public bool IsZoneUnlocked(ZoneId zone)
            {
                return unlockedZones.Contains(zone);
            }

            public bool UnlockZone(ZoneId zone)
            {
                return Remember(unlockedZones, zone);
            }

            public bool IsExpeditionChapterCompleted(string chapterId)
            {
                return completedChapters.Contains(chapterId);
            }

            public bool CompleteExpeditionChapter(string chapterId)
            {
                return Remember(completedChapters, chapterId);
            }

            public bool HasLandmarkState(string landmarkStateId)
            {
                return landmarkStates.Contains(landmarkStateId);
            }

            public bool RememberLandmarkState(string landmarkStateId)
            {
                return Remember(landmarkStates, landmarkStateId);
            }

            public bool HasFieldStationUpgrade(string upgradeId)
            {
                return fieldStationUpgrades.Contains(upgradeId);
            }

            public bool RememberFieldStationUpgrade(string upgradeId)
            {
                return Remember(fieldStationUpgrades, upgradeId);
            }

            public bool HasCompletedSetPiece(string setPieceId)
            {
                return completedSetPieces.Contains(setPieceId);
            }

            public bool RememberCompletedSetPiece(string setPieceId)
            {
                return Remember(completedSetPieces, setPieceId);
            }

            public bool SetActiveExpeditionChapter(string chapterId)
            {
                if (string.Equals(ActiveChapterId, chapterId, StringComparison.Ordinal))
                {
                    return false;
                }

                ActiveChapterId = chapterId;
                WriteCount++;
                return true;
            }

            private bool Remember<T>(HashSet<T> values, T value)
            {
                bool changed = values.Add(value);
                if (changed)
                {
                    WriteCount++;
                }

                return changed;
            }
        }
    }
}
