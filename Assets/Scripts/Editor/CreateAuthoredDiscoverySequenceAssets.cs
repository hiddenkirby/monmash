using System.IO;
using Tidepool.Domain;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class CreateAuthoredDiscoverySequenceAssets
    {
        public const string AuthoredDiscoveryFolder = "Assets/Data/AuthoredDiscoveries";

        [MenuItem("Tools/Tidepool/Create Authored Discovery Sequence Assets")]
        public static void CreateAssets()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder(AuthoredDiscoveryFolder);

            UpsertSequence(
                "discovery.meadow.gullwing",
                "gullwing",
                ZoneId.SeagrassMeadow,
                AuthoredDiscoveryReplayRule.UntilCaught,
                string.Empty,
                string.Empty,
                ExpeditionStateIds.LandmarkMeadowWave,
                "setpiece.discovery.gullwing",
                ExpeditionStateIds.LandmarkMeadowWave,
                "camera.meadow.last-light",
                "audio.discovery.last-light",
                new[] { "A silver flash skims the last bright water.", "The grass leans toward a quiet ripple." },
                "A bright shape glides above the meadow water.",
                "Gullwing settles softly in the journal.");

            UpsertSequence(
                "discovery.meadow.tanglemaw",
                "tanglemaw",
                ZoneId.SeagrassMeadow,
                AuthoredDiscoveryReplayRule.UntilCaught,
                string.Empty,
                string.Empty,
                ExpeditionStateIds.LandmarkMeadowWave,
                "setpiece.discovery.tanglemaw",
                ExpeditionStateIds.LandmarkMeadowWave,
                "camera.meadow.unusual-shell",
                "audio.discovery.shell",
                new[] { "An unusual shell nudges itself across the sand.", "Something curious waits below it." },
                "Tiny arms peek from under the shell.",
                "Tanglemaw gives the jar one last curious pat.");

            UpsertSequence(
                "discovery.kelp.lumen",
                "lumen",
                ZoneId.KelpCurtain,
                AuthoredDiscoveryReplayRule.UntilCaught,
                ExpeditionStateIds.ChapterMeadow,
                ExpeditionStateIds.SetPieceKelpUnlock,
                ExpeditionStateIds.LandmarkKelpLights,
                "setpiece.discovery.lumen",
                ExpeditionStateIds.LandmarkKelpLights,
                "camera.kelp.glow-trail",
                "audio.discovery.glow",
                new[] { "A small light blinks behind the kelp.", "The glow waits, then blinks again." },
                "A shy lantern light drifts into view.",
                "Lumen glows a little brighter.");

            UpsertSequence(
                "discovery.rocky.clackaw",
                "clackaw",
                ZoneId.RockyShelf,
                AuthoredDiscoveryReplayRule.UntilCaught,
                ExpeditionStateIds.ChapterKelp,
                ExpeditionStateIds.SetPieceRockyUnlock,
                ExpeditionStateIds.LandmarkRockyOldStones,
                "setpiece.discovery.clackaw",
                ExpeditionStateIds.LandmarkRockyOldStones,
                "camera.rocky.snap",
                "audio.discovery.pebble-snap",
                new[] { "Pebbles click in a neat little rhythm.", "One stone seems to answer back." },
                "A serious little claw taps the shelf.",
                "Clackaw gives one proud snap.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static AuthoredDiscoverySequence LoadById(string id)
        {
            return AssetDatabase.LoadAssetAtPath<AuthoredDiscoverySequence>(
                $"{AuthoredDiscoveryFolder}/{id}.asset");
        }

        private static void UpsertSequence(
            string id,
            string speciesId,
            ZoneId zone,
            AuthoredDiscoveryReplayRule replayRule,
            string requiredCompletedChapterId,
            string requiredCompletedSetPieceId,
            string requiredLandmarkStateId,
            string completionSetPieceId,
            string landmarkStateId,
            string cameraCueId,
            string audioCueId,
            string[] clueLines,
            string encounterIntroText,
            string catchCelebrationText)
        {
            string path = $"{AuthoredDiscoveryFolder}/{id}.asset";
            AuthoredDiscoverySequence sequence = AssetDatabase.LoadAssetAtPath<AuthoredDiscoverySequence>(path);
            if (sequence == null)
            {
                sequence = ScriptableObject.CreateInstance<AuthoredDiscoverySequence>();
                AssetDatabase.CreateAsset(sequence, path);
            }

            sequence.Configure(
                id,
                AssetDatabase.LoadAssetAtPath<TidelingSpecies>($"Assets/Data/Species/{speciesId}.asset"),
                zone,
                replayRule,
                requiredCompletedChapterId,
                requiredCompletedSetPieceId,
                requiredLandmarkStateId,
                completionSetPieceId,
                landmarkStateId,
                cameraCueId,
                audioCueId,
                clueLines,
                encounterIntroText,
                catchCelebrationText);
            EditorUtility.SetDirty(sequence);
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
