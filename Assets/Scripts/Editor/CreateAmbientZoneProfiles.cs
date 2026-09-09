using System.IO;
using Tidepool.Domain;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class CreateAmbientZoneProfiles
    {
        private const string AmbientProfileFolder = "Assets/Data/AmbientProfiles";

        [MenuItem("Tools/Tidepool/Create Ambient Zone Profiles")]
        public static void CreateProfiles()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder(AmbientProfileFolder);

            UpsertProfile(
                ZoneId.TidepoolShallows,
                new Color(0.95f, 0.98f, 1f, 1f),
                new Color(0.55f, 0.68f, 0.86f, 1f),
                new[]
                {
                    Layer("shallows-water-glints", AmbientMotionKind.WaterShimmer, 10, new Vector2(18f, 6f), new Color(0.75f, 0.95f, 1f, 0.7f)),
                    Layer("shallows-shell-pulses", AmbientMotionKind.SoftLightPulse, 4, new Vector2(12f, 4f), new Color(1f, 0.9f, 0.65f, 0.55f))
                });

            UpsertProfile(
                ZoneId.SeagrassMeadow,
                new Color(0.82f, 1f, 0.9f, 1f),
                new Color(0.32f, 0.58f, 0.62f, 1f),
                new[]
                {
                    Layer("meadow-grass-sway", AmbientMotionKind.SwayingPlant, 14, new Vector2(20f, 7f), new Color(0.48f, 0.9f, 0.58f, 0.8f)),
                    Layer("meadow-bubbles", AmbientMotionKind.DriftingParticle, 8, new Vector2(18f, 6f), new Color(0.82f, 0.96f, 1f, 0.5f))
                });

            UpsertProfile(
                ZoneId.KelpCurtain,
                new Color(0.54f, 0.86f, 0.78f, 1f),
                new Color(0.15f, 0.38f, 0.48f, 1f),
                new[]
                {
                    Layer("kelp-ribbon-sway", AmbientMotionKind.SwayingPlant, 16, new Vector2(22f, 9f), new Color(0.4f, 0.82f, 0.48f, 0.85f)),
                    Layer("kelp-distant-glows", AmbientMotionKind.SoftLightPulse, 6, new Vector2(16f, 7f), new Color(0.95f, 0.82f, 0.38f, 0.65f))
                });

            UpsertProfile(
                ZoneId.RockyShelf,
                new Color(0.9f, 0.88f, 0.82f, 1f),
                new Color(0.42f, 0.52f, 0.68f, 1f),
                new[]
                {
                    Layer("rocky-foam-drift", AmbientMotionKind.DriftingParticle, 8, new Vector2(18f, 5f), new Color(0.95f, 0.98f, 1f, 0.6f)),
                    Layer("rocky-distant-silhouettes", AmbientMotionKind.DistantSilhouette, 3, new Vector2(14f, 3f), new Color(0.42f, 0.36f, 0.34f, 0.45f))
                });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static AmbientMotionLayer Layer(
            string id,
            AmbientMotionKind motionKind,
            int poolSize,
            Vector2 spawnArea,
            Color dayTint)
        {
            return new AmbientMotionLayer(
                id,
                motionKind,
                dayTint,
                new Color(dayTint.r * 0.55f, dayTint.g * 0.72f, Mathf.Min(1f, dayTint.b * 1.2f), dayTint.a),
                poolSize,
                spawnArea,
                new Vector2(0.03f, 0.18f),
                new Vector2(0.75f, 1.2f),
                new Vector2(9f, 18f),
                0.16f,
                0.45f,
                0.1f,
                0.12f,
                false,
                1);
        }

        private static void UpsertProfile(
            ZoneId zone,
            Color dayTint,
            Color nightTint,
            AmbientMotionLayer[] layers)
        {
            string path = $"{AmbientProfileFolder}/{zone}.asset";
            AmbientZoneProfile profile = AssetDatabase.LoadAssetAtPath<AmbientZoneProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<AmbientZoneProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            profile.Configure(zone, dayTint, nightTint, 0.35f, layers);
            EditorUtility.SetDirty(profile);
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
