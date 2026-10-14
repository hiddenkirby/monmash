using System;
using System.Collections.Generic;
using System.Reflection;
using Tidepool.Domain;
using Tidepool.Runtime;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class AmbientTidelingEditModeTests
    {
        [MenuItem("Tools/Tidepool/Verify Ambient Tideling Actors")]
        public static void RunAll()
        {
            VerifyMissingSpriteFallbackAndNoPhysics();
            VerifySaveAndTimeFilters();
            VerifyLongRunningDriftStaysInMotion();
            Debug.Log("Ambient Tideling edit-mode tests passed: bounded pools, fallback sprites, no physics components, save filters, time filters, and long-running drift.");
        }

        private static void VerifyMissingSpriteFallbackAndNoPhysics()
        {
            TidelingSpecies species = CreateSpecies("test-blip", ZoneId.SeagrassMeadow);
            AmbientTidelingZoneProfile profile = ScriptableObject.CreateInstance<AmbientTidelingZoneProfile>();
            profile.Configure(
                ZoneId.SeagrassMeadow,
                3,
                0.35f,
                new[]
                {
                    new AmbientTidelingSpawnEntry(
                        "test-missing-sprite",
                        species,
                        AmbientTidelingBehaviorKind.Drift,
                        AmbientTidelingTimeWindow.Any,
                        false,
                        null,
                        null,
                        3,
                        1,
                        new Vector2(4f, 2f),
                        new Vector2(0.2f, 0.3f),
                        new Vector2(0.02f, 0.04f),
                        0.1f,
                        0.25f,
                        0.2f,
                        Color.white,
                        Color.cyan,
                        2)
                });

            GameObject controllerObject = new GameObject("AmbientTidelingEditModeTest");
            try
            {
                AmbientTidelingController controller = controllerObject.AddComponent<AmbientTidelingController>();
                controller.SetProfile(profile);
                controller.SetPlaying(true);
                controller.RefreshActorsForEditMode();
                Require(controller.PooledActorCount == 3, "Ambient Tideling profiles must create bounded pools.");
                Require(controller.ActiveActorCount == 3, "Eligible entries must show only their bounded actor count.");
                Require(controllerObject.GetComponentsInChildren<Collider2D>(true).Length == 0, "Ambient Tideling actors must not add 2D colliders.");
                Require(controllerObject.GetComponentsInChildren<Rigidbody2D>(true).Length == 0, "Ambient Tideling actors must not add 2D rigidbodies.");
                SpriteRenderer[] renderers = controllerObject.GetComponentsInChildren<SpriteRenderer>(true);
                Require(renderers.Length == 3, "Each pooled actor must have one SpriteRenderer.");
                Require(renderers[0].sprite != null, "Missing species sprites must receive a neutral fallback sprite.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(controllerObject);
                UnityEngine.Object.DestroyImmediate(profile);
                UnityEngine.Object.DestroyImmediate(species);
            }
        }

        private static void VerifySaveAndTimeFilters()
        {
            TidelingSpecies species = CreateSpecies("test-wobbet", ZoneId.SeagrassMeadow);
            AmbientTidelingSpawnEntry caughtOnly = new AmbientTidelingSpawnEntry(
                "test-caught-only",
                species,
                AmbientTidelingBehaviorKind.ApproachAfterBefriending,
                AmbientTidelingTimeWindow.Day,
                true,
                new[] { ExpeditionStateIds.ChapterMeadow },
                null,
                1,
                1,
                new Vector2(4f, 2f),
                new Vector2(0.2f, 0.3f),
                new Vector2(0.02f, 0.04f),
                0.1f,
                0.25f,
                0.2f,
                Color.white,
                Color.cyan,
                2);
            AmbientTidelingZoneProfile profile = ScriptableObject.CreateInstance<AmbientTidelingZoneProfile>();
            profile.Configure(ZoneId.SeagrassMeadow, 2, 0.35f, new[] { caughtOnly });

            GameSaveService previousInstance = GameSaveService.Instance;
            GameObject saveObject = new GameObject("AmbientTidelingSaveTestService");
            GameObject controllerObject = new GameObject("AmbientTidelingFilterTest");

            try
            {
                GameSaveService saveService = saveObject.AddComponent<GameSaveService>();
                SetStaticAutoProperty("Instance", saveService);
                SetData(saveService, new SaveData
                {
                    activeExpeditionChapterId = ExpeditionStateIds.ChapterMeadow,
                    caught = new List<CaughtTideling>
                    {
                        new CaughtTideling { speciesId = "test-wobbet", nickname = "Wob", caughtInZone = ZoneId.SeagrassMeadow, timesSeen = 1 }
                    }
                });

                AmbientTidelingController controller = controllerObject.AddComponent<AmbientTidelingController>();
                controller.SetProfile(profile);
                controller.SetDayNightBlend(0f);
                Require(controller.IsEntryEligible(caughtOnly), "Caught-only entries must become eligible after the species is in save data.");
                controller.SetDayNightBlend(1f);
                Require(!controller.IsEntryEligible(caughtOnly), "Day-only entries must be filtered out at night.");
            }
            finally
            {
                SetStaticAutoProperty("Instance", previousInstance);
                UnityEngine.Object.DestroyImmediate(controllerObject);
                UnityEngine.Object.DestroyImmediate(saveObject);
                UnityEngine.Object.DestroyImmediate(profile);
                UnityEngine.Object.DestroyImmediate(species);
            }
        }

        private static void VerifyLongRunningDriftStaysInMotion()
        {
            TidelingSpecies species = CreateSpecies("test-drifter", ZoneId.SeagrassMeadow);
            AmbientTidelingZoneProfile profile = ScriptableObject.CreateInstance<AmbientTidelingZoneProfile>();
            profile.Configure(
                ZoneId.SeagrassMeadow,
                1,
                0.35f,
                new[]
                {
                    new AmbientTidelingSpawnEntry(
                        "test-long-running-drift",
                        species,
                        AmbientTidelingBehaviorKind.Drift,
                        AmbientTidelingTimeWindow.Any,
                        false,
                        null,
                        null,
                        1,
                        1,
                        new Vector2(2f, 2f),
                        Vector2.one,
                        new Vector2(0.2f, 0.2f),
                        0f,
                        0f,
                        0f,
                        Color.white,
                        Color.white,
                        2)
                });

            GameObject controllerObject = new GameObject("AmbientTidelingDriftTest");
            try
            {
                AmbientTidelingController controller = controllerObject.AddComponent<AmbientTidelingController>();
                controller.SetProfile(profile);
                controller.SetPlaying(true);

                RefreshActors(controller, 100f);
                SpriteRenderer renderer = controllerObject.GetComponentInChildren<SpriteRenderer>(true);
                Require(renderer != null, "A drift profile must create a visible actor renderer.");
                float firstPosition = renderer.transform.localPosition.x;

                RefreshActors(controller, 1f);
                float secondPosition = renderer.transform.localPosition.x;
                Require(Mathf.Abs(secondPosition - firstPosition) > 0.001f,
                    "Long-running drift actors must keep moving instead of clamping permanently to a spawn-area edge.");
                Require(Mathf.Abs(secondPosition) <= 1f,
                    "Long-running drift actors must remain inside their configured spawn area.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(controllerObject);
                UnityEngine.Object.DestroyImmediate(profile);
                UnityEngine.Object.DestroyImmediate(species);
            }
        }

        private static void RefreshActors(AmbientTidelingController controller, float deltaTime)
        {
            MethodInfo method = typeof(AmbientTidelingController).GetMethod(
                "RefreshActors",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new MissingMethodException(typeof(AmbientTidelingController).Name, "RefreshActors");
            }

            method.Invoke(controller, new object[] { deltaTime });
        }

        private static TidelingSpecies CreateSpecies(string id, ZoneId zone)
        {
            TidelingSpecies species = ScriptableObject.CreateInstance<TidelingSpecies>();
            species.Configure(
                id,
                id,
                TidelingCurrent.Tide,
                TidelingRarity.Common,
                new[] { zone },
                EncounterAvailability.Always,
                string.Empty,
                "A friendly test Tideling.",
                ContestAiPattern.Defensive,
                0.35f,
                0.65f);
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
