using System.IO;
using Tidepool.Domain;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class CreateAmbientTidelingZoneProfiles
    {
        private const string ProfileFolder = "Assets/Data/AmbientTidelingProfiles";

        [MenuItem("Tools/Tidepool/Create Ambient Tideling Zone Profiles")]
        public static void CreateProfiles()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder(ProfileFolder);

            UpsertProfile(
                ZoneId.TidepoolShallows,
                8,
                new[]
                {
                    Entry("shallows-blip-surface", "blip", AmbientTidelingBehaviorKind.Surface, AmbientTidelingTimeWindow.Day, false, null, null, 3, 1, new Vector2(9f, 3.5f), new Color(0.88f, 0.97f, 1f, 0.86f), 3),
                    Entry("shallows-nubbin-peek", "nubbin", AmbientTidelingBehaviorKind.Peek, AmbientTidelingTimeWindow.Any, false, null, null, 2, 1, new Vector2(7f, 2.5f), new Color(1f, 0.92f, 0.78f, 0.88f), 3),
                    Entry("shallows-frillick-hide", "frillick", AmbientTidelingBehaviorKind.Hide, AmbientTidelingTimeWindow.Any, false, null, null, 2, 1, new Vector2(7f, 2.5f), new Color(1f, 0.74f, 0.78f, 0.82f), 3),
                    Entry("shallows-sputter-glow", "sputter", AmbientTidelingBehaviorKind.Glow, AmbientTidelingTimeWindow.Night, false, null, null, 2, 1, new Vector2(6f, 2.5f), new Color(1f, 0.9f, 0.48f, 0.84f), 4),
                    Entry("shallows-barnaby-rest", "old-barnaby", AmbientTidelingBehaviorKind.Rest, AmbientTidelingTimeWindow.Any, true, new[] { ExpeditionStateIds.SetPieceFinale }, null, 1, 1, new Vector2(4f, 1.5f), new Color(0.88f, 0.86f, 0.78f, 0.9f), 2)
                });

            UpsertProfile(
                ZoneId.SeagrassMeadow,
                10,
                new[]
                {
                    Entry("meadow-wobbet-drift", "wobbet", AmbientTidelingBehaviorKind.Drift, AmbientTidelingTimeWindow.Any, false, null, null, 3, 1, new Vector2(10f, 4.5f), new Color(0.88f, 0.98f, 1f, 0.84f), 3),
                    Entry("meadow-clackaw-peek", "clackaw", AmbientTidelingBehaviorKind.Peek, AmbientTidelingTimeWindow.Any, false, null, null, 2, 1, new Vector2(8f, 3f), new Color(0.9f, 0.74f, 0.62f, 0.84f), 3),
                    Entry("meadow-sweepfin-school", "sweepfin", AmbientTidelingBehaviorKind.School, AmbientTidelingTimeWindow.Day, false, null, null, 4, 1, new Vector2(12f, 4f), new Color(0.74f, 0.95f, 1f, 0.82f), 3),
                    Entry("meadow-mossback-approach", "mossback", AmbientTidelingBehaviorKind.ApproachAfterBefriending, AmbientTidelingTimeWindow.Any, true, null, null, 2, 1, new Vector2(7f, 3f), new Color(0.72f, 0.94f, 0.7f, 0.84f), 3),
                    Entry("meadow-lumen-glow", "lumen", AmbientTidelingBehaviorKind.Glow, AmbientTidelingTimeWindow.Night, false, null, null, 2, 1, new Vector2(8f, 3.5f), new Color(1f, 0.86f, 0.45f, 0.88f), 4),
                    Entry("meadow-thistlecoat-hide", "thistlecoat", AmbientTidelingBehaviorKind.Hide, AmbientTidelingTimeWindow.Any, false, null, null, 2, 1, new Vector2(8f, 3f), new Color(0.96f, 0.74f, 0.92f, 0.8f), 3),
                    Entry("meadow-gullwing-last-light", "gullwing", AmbientTidelingBehaviorKind.Surface, AmbientTidelingTimeWindow.LastHourOfDaylight, false, null, null, 1, 1, new Vector2(10f, 4f), new Color(0.82f, 0.96f, 1f, 0.86f), 4),
                    Entry("meadow-tanglemaw-hide", "tanglemaw", AmbientTidelingBehaviorKind.Hide, AmbientTidelingTimeWindow.Any, false, null, null, 1, 1, new Vector2(7f, 3f), new Color(0.66f, 0.82f, 1f, 0.78f), 3)
                });

            UpsertProfile(
                ZoneId.KelpCurtain,
                8,
                new[]
                {
                    Entry("kelp-tanglemaw-hide", "tanglemaw", AmbientTidelingBehaviorKind.Hide, AmbientTidelingTimeWindow.Any, false, new[] { ExpeditionStateIds.ChapterKelp }, null, 2, 1, new Vector2(8f, 4f), new Color(0.72f, 0.88f, 1f, 0.78f), 3),
                    Entry("kelp-lumen-glow-trail", "lumen", AmbientTidelingBehaviorKind.Glow, AmbientTidelingTimeWindow.Night, false, null, null, 3, 1, new Vector2(10f, 4f), new Color(1f, 0.84f, 0.44f, 0.82f), 4),
                    Entry("kelp-mossback-approach", "mossback", AmbientTidelingBehaviorKind.ApproachAfterBefriending, AmbientTidelingTimeWindow.Any, true, null, null, 2, 1, new Vector2(6f, 3f), new Color(0.78f, 0.96f, 0.76f, 0.84f), 3)
                });

            UpsertProfile(
                ZoneId.RockyShelf,
                7,
                new[]
                {
                    Entry("rocky-nubbin-rest", "nubbin", AmbientTidelingBehaviorKind.Rest, AmbientTidelingTimeWindow.Any, false, null, null, 2, 1, new Vector2(7f, 2.5f), new Color(0.88f, 0.84f, 0.76f, 0.86f), 3),
                    Entry("rocky-clackaw-peek", "clackaw", AmbientTidelingBehaviorKind.Peek, AmbientTidelingTimeWindow.Day, false, null, null, 2, 1, new Vector2(8f, 3f), new Color(0.92f, 0.78f, 0.64f, 0.84f), 3),
                    Entry("rocky-barnaby-approach", "old-barnaby", AmbientTidelingBehaviorKind.ApproachAfterBefriending, AmbientTidelingTimeWindow.Any, true, new[] { ExpeditionStateIds.SetPieceFinale }, null, 1, 1, new Vector2(5f, 2f), new Color(0.82f, 0.8f, 0.72f, 0.9f), 2)
                });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static AmbientTidelingSpawnEntry Entry(
            string id,
            string speciesId,
            AmbientTidelingBehaviorKind behaviorKind,
            AmbientTidelingTimeWindow timeWindow,
            bool requireSpeciesCaught,
            string[] requiredStateIds,
            string[] blockedStateIds,
            int poolSize,
            int reducedMotionVisibleCount,
            Vector2 spawnArea,
            Color dayTint,
            int sortingOrder)
        {
            return new AmbientTidelingSpawnEntry(
                id,
                LoadSpecies(speciesId),
                behaviorKind,
                timeWindow,
                requireSpeciesCaught,
                requiredStateIds,
                blockedStateIds,
                poolSize,
                reducedMotionVisibleCount,
                spawnArea,
                new Vector2(0.26f, 0.42f),
                new Vector2(0.03f, 0.12f),
                0.22f,
                0.42f,
                0.28f,
                dayTint,
                new Color(dayTint.r * 0.68f, dayTint.g * 0.78f, Mathf.Min(1f, dayTint.b * 1.18f), dayTint.a),
                sortingOrder);
        }

        private static void UpsertProfile(ZoneId zone, int maxVisibleActors, AmbientTidelingSpawnEntry[] entries)
        {
            string path = $"{ProfileFolder}/{zone}.asset";
            AmbientTidelingZoneProfile profile = AssetDatabase.LoadAssetAtPath<AmbientTidelingZoneProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<AmbientTidelingZoneProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            profile.Configure(zone, maxVisibleActors, 0.35f, entries);
            EditorUtility.SetDirty(profile);
        }

        private static TidelingSpecies LoadSpecies(string speciesId)
        {
            return AssetDatabase.LoadAssetAtPath<TidelingSpecies>($"Assets/Data/Species/{speciesId}.asset");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string name = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
