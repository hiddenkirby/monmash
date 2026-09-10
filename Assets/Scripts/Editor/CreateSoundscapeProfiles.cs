using System.IO;
using Tidepool.Domain;
using Tidepool.Runtime;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class CreateSoundscapeProfiles
    {
        private const string SoundscapeFolder = "Assets/Data/Soundscape";

        [MenuItem("Tools/Tidepool/Create Soundscape Profiles")]
        public static void CreateProfiles()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder(SoundscapeFolder);

            UpsertProfile("soundscape.shallows", SoundscapeProfileRole.Zone, ZoneId.TidepoolShallows, 2f, 1f, 0f,
                Layer("water-laps", 0.28f, true),
                Layer("pebble-ticks", 0.16f, true),
                Layer("shore-birds", 0.12f, true));
            UpsertProfile("soundscape.meadow", SoundscapeProfileRole.Zone, ZoneId.SeagrassMeadow, 2f, 1f, 0f,
                Layer("grass-wash", 0.24f, true),
                Layer("bubbles", 0.16f, true),
                Layer("reed-color", 0.10f, true));
            UpsertProfile("soundscape.kelp", SoundscapeProfileRole.Zone, ZoneId.KelpCurtain, 2.4f, 1f, 0f,
                Layer("lower-water-bed", 0.28f, true),
                Layer("kelp-creaks", 0.18f, true),
                Layer("glow-tones", 0.10f, true));
            UpsertProfile("soundscape.rocky", SoundscapeProfileRole.Zone, ZoneId.RockyShelf, 2.4f, 1f, 0f,
                Layer("wide-surf", 0.30f, true),
                Layer("shell-resonance", 0.14f, true),
                Layer("low-notes", 0.10f, true));
            UpsertProfile("soundscape.field-station", SoundscapeProfileRole.FieldStation, ZoneId.TidepoolShallows, 1.5f, 1f, 0f,
                Layer("sheltered-water", 0.24f, true),
                Layer("paper-shell-details", 0.14f, true),
                Layer("quiet-motif", 0.12f, true));
            UpsertProfile("soundscape.discovery", SoundscapeProfileRole.Discovery, ZoneId.TidepoolShallows, 0.35f, 0.45f, 1.8f,
                Layer("local-cue", 0.35f, false),
                Layer("soft-stinger", 0.30f, false));
            UpsertProfile("soundscape.route-unlock", SoundscapeProfileRole.RouteUnlock, ZoneId.TidepoolShallows, 0.5f, 0.55f, 2f,
                Layer("rising-flourish", 0.32f, false));
            UpsertProfile("soundscape.festival", SoundscapeProfileRole.Festival, ZoneId.SeagrassMeadow, 1.5f, 1f, 0f,
                Layer("warm-rhythm", 0.24f, true),
                Layer("coast-motif", 0.18f, true));
            UpsertProfile("soundscape.finale", SoundscapeProfileRole.Finale, ZoneId.RockyShelf, 2f, 1f, 0f,
                Layer("sparse-coast-motif", 0.26f, true),
                Layer("ordinary-coast-return", 0.16f, true));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/Tidepool/Verify Soundscape Profiles")]
        public static void VerifyProfiles()
        {
            string[] expectedIds =
            {
                "soundscape.shallows",
                "soundscape.meadow",
                "soundscape.kelp",
                "soundscape.rocky",
                "soundscape.field-station",
                "soundscape.discovery",
                "soundscape.route-unlock",
                "soundscape.festival",
                "soundscape.finale"
            };

            foreach (string id in expectedIds)
            {
                string path = $"{SoundscapeFolder}/{id}.asset";
                SoundscapeProfile profile = AssetDatabase.LoadAssetAtPath<SoundscapeProfile>(path);
                if (profile == null)
                {
                    Debug.LogWarning($"Missing soundscape profile: {path}. Run Tools/Tidepool/Create Soundscape Profiles.");
                    continue;
                }

                if (profile.Layers == null || profile.Layers.Length == 0)
                {
                    Debug.LogWarning($"Soundscape profile has no layer rows: {profile.Id}");
                }
            }

            Debug.Log("Soundscape profile verification complete. Missing clips are allowed until local audio assets are imported and logged.");
        }

        private static void UpsertProfile(
            string id,
            SoundscapeProfileRole role,
            ZoneId zone,
            float fadeSeconds,
            float duckExistingTo,
            float duckSeconds,
            params LayerSeed[] layers)
        {
            string path = $"{SoundscapeFolder}/{id}.asset";
            SoundscapeProfile profile = AssetDatabase.LoadAssetAtPath<SoundscapeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<SoundscapeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("id").stringValue = id;
            serializedProfile.FindProperty("role").enumValueIndex = (int)role;
            serializedProfile.FindProperty("zone").enumValueIndex = (int)zone;
            serializedProfile.FindProperty("fadeSeconds").floatValue = fadeSeconds;
            serializedProfile.FindProperty("duckExistingTo").floatValue = duckExistingTo;
            serializedProfile.FindProperty("duckSeconds").floatValue = duckSeconds;

            SerializedProperty layersProperty = serializedProfile.FindProperty("layers");
            layersProperty.arraySize = layers.Length;
            for (int index = 0; index < layers.Length; index++)
            {
                SerializedProperty layerProperty = layersProperty.GetArrayElementAtIndex(index);
                layerProperty.FindPropertyRelative("id").stringValue = layers[index].Id;
                layerProperty.FindPropertyRelative("clip").objectReferenceValue = null;
                layerProperty.FindPropertyRelative("loop").boolValue = layers[index].Loop;
                layerProperty.FindPropertyRelative("playOnEnter").boolValue = true;
                layerProperty.FindPropertyRelative("volume").floatValue = layers[index].Volume;
                layerProperty.FindPropertyRelative("pitch").floatValue = 1f;
                layerProperty.FindPropertyRelative("spatialBlend").floatValue = 0f;
            }

            serializedProfile.ApplyModifiedProperties();
            EditorUtility.SetDirty(profile);
        }

        private static LayerSeed Layer(string id, float volume, bool loop)
        {
            return new LayerSeed(id, volume, loop);
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

        private readonly struct LayerSeed
        {
            public LayerSeed(string id, float volume, bool loop)
            {
                Id = id;
                Volume = volume;
                Loop = loop;
            }

            public string Id { get; }
            public float Volume { get; }
            public bool Loop { get; }
        }
    }
}
