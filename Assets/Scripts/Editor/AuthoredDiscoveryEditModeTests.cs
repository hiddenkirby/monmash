using System;
using System.Collections.Generic;
using System.Reflection;
using Tidepool.Domain;
using Tidepool.Runtime;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class AuthoredDiscoveryEditModeTests
    {
        [MenuItem("Tools/Tidepool/Verify Authored Discoveries")]
        public static void RunAll()
        {
            VerifyUntilCaughtReplayRules();
            VerifyOncePerSaveReplayRules();
            VerifyFallbackClueLines();
            Debug.Log("Authored discovery edit-mode tests passed: eligibility, replay rules, and fallback clue text.");
        }

        private static void VerifyUntilCaughtReplayRules()
        {
            TidelingSpecies species = CreateSpecies("test-gullwing", ZoneId.SeagrassMeadow, TidelingRarity.Rare);
            AuthoredDiscoverySequence sequence = CreateSequence(
                "discovery.test.gullwing",
                species,
                ZoneId.SeagrassMeadow,
                AuthoredDiscoveryReplayRule.UntilCaught);
            GameObject triggerObject = new GameObject("AuthoredDiscoveryEligibilityTest");
            GameObject saveObject = new GameObject("AuthoredDiscoverySaveTest");

            try
            {
                AuthoredDiscoveryTrigger trigger = triggerObject.AddComponent<AuthoredDiscoveryTrigger>();
                SetPrivateField(trigger, "sequence", sequence);
                GameSaveService saveService = saveObject.AddComponent<GameSaveService>();
                SetData(saveService, new SaveData
                {
                    unlockedZoneIds = new List<ZoneId> { ZoneId.TidepoolShallows, ZoneId.SeagrassMeadow }
                });

                Require(trigger.IsEligible(saveService, species), "Until-caught discoveries must be eligible before the species is caught.");
                saveService.Data.authoredDiscoveryIds.Add(sequence.Id);
                Require(trigger.IsEligible(saveService, species), "Until-caught discoveries must recur after a miss or let-go.");
                saveService.Data.caught.Add(new CaughtTideling { speciesId = species.Id, nickname = species.DisplayName, caughtInZone = ZoneId.SeagrassMeadow, timesSeen = 1 });
                Require(!trigger.IsEligible(saveService, species), "Until-caught discoveries must stop after the species is caught.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(triggerObject);
                UnityEngine.Object.DestroyImmediate(saveObject);
                UnityEngine.Object.DestroyImmediate(sequence);
                UnityEngine.Object.DestroyImmediate(species);
            }
        }

        private static void VerifyOncePerSaveReplayRules()
        {
            TidelingSpecies species = CreateSpecies("test-lumen", ZoneId.KelpCurtain, TidelingRarity.Uncommon);
            AuthoredDiscoverySequence sequence = CreateSequence(
                "discovery.test.lumen",
                species,
                ZoneId.KelpCurtain,
                AuthoredDiscoveryReplayRule.OncePerSave);
            GameObject triggerObject = new GameObject("AuthoredDiscoveryOnceTest");
            GameObject saveObject = new GameObject("AuthoredDiscoveryOnceSaveTest");

            try
            {
                AuthoredDiscoveryTrigger trigger = triggerObject.AddComponent<AuthoredDiscoveryTrigger>();
                SetPrivateField(trigger, "sequence", sequence);
                GameSaveService saveService = saveObject.AddComponent<GameSaveService>();
                SetData(saveService, new SaveData
                {
                    unlockedZoneIds = new List<ZoneId> { ZoneId.TidepoolShallows, ZoneId.SeagrassMeadow, ZoneId.KelpCurtain },
                    completedExpeditionChapterIds = new List<string> { ExpeditionStateIds.ChapterMeadow },
                    completedSetPieceIds = new List<string> { ExpeditionStateIds.SetPieceKelpUnlock },
                    landmarkStateIds = new List<string> { ExpeditionStateIds.LandmarkKelpLights }
                });

                Require(trigger.IsEligible(saveService, species), "Once-per-save discoveries must be eligible before their authored state is remembered.");
                saveService.Data.authoredDiscoveryIds.Add(sequence.Id);
                Require(!trigger.IsEligible(saveService, species), "Once-per-save discoveries must not replay after their authored state is remembered.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(triggerObject);
                UnityEngine.Object.DestroyImmediate(saveObject);
                UnityEngine.Object.DestroyImmediate(sequence);
                UnityEngine.Object.DestroyImmediate(species);
            }
        }

        private static void VerifyFallbackClueLines()
        {
            TidelingSpecies species = CreateSpecies("test-clackaw", ZoneId.RockyShelf, TidelingRarity.Uncommon);
            AuthoredDiscoverySequence sequence = CreateSequence(
                "discovery.test.clackaw",
                species,
                ZoneId.RockyShelf,
                AuthoredDiscoveryReplayRule.UntilCaught,
                new[] { " ", "Pebbles click nearby." });

            try
            {
                Require(sequence.GetFallbackClueLine() == "Pebbles click nearby.", "Fallback clue text must use the first nonblank clue line.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sequence);
                UnityEngine.Object.DestroyImmediate(species);
            }
        }

        private static AuthoredDiscoverySequence CreateSequence(
            string id,
            TidelingSpecies species,
            ZoneId zone,
            AuthoredDiscoveryReplayRule replayRule,
            string[] clueLines = null)
        {
            AuthoredDiscoverySequence sequence = ScriptableObject.CreateInstance<AuthoredDiscoverySequence>();
            sequence.Configure(
                id,
                species,
                zone,
                replayRule,
                zone == ZoneId.KelpCurtain ? ExpeditionStateIds.ChapterMeadow : string.Empty,
                zone == ZoneId.KelpCurtain ? ExpeditionStateIds.SetPieceKelpUnlock : string.Empty,
                zone == ZoneId.KelpCurtain ? ExpeditionStateIds.LandmarkKelpLights : string.Empty,
                "setpiece.discovery.test",
                zone == ZoneId.RockyShelf ? ExpeditionStateIds.LandmarkRockyOldStones : string.Empty,
                "camera.test",
                "audio.test",
                clueLines ?? new[] { "Something moves nearby." },
                "Something friendly appears.",
                "It settles softly in the journal.");
            return sequence;
        }

        private static TidelingSpecies CreateSpecies(string id, ZoneId zone, TidelingRarity rarity)
        {
            TidelingSpecies species = ScriptableObject.CreateInstance<TidelingSpecies>();
            species.Configure(
                id,
                id,
                TidelingCurrent.Tide,
                rarity,
                new[] { zone },
                EncounterAvailability.Always,
                string.Empty,
                "A friendly authored discovery test Tideling.",
                ContestAiPattern.Tricky,
                0.3f,
                0.72f);
            return species;
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

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
