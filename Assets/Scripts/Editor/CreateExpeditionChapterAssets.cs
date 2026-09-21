using System.IO;
using Tidepool.Domain;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class CreateExpeditionChapterAssets
    {
        public const string ChapterFolder = "Assets/Data/ExpeditionChapters";

        [MenuItem("Tools/Tidepool/Create Great Low Tide Chapter Assets")]
        public static void CreateAssets()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder(ChapterFolder);

            Upsert(
                "chapter-shallows",
                ExpeditionStateIds.ChapterShallows,
                ZoneId.TidepoolShallows,
                "The Coast Opens",
                "The tide went farther out than anyone expected.",
                "Follow the shell glint near the stone arch, then meet one shallows friend.",
                "The arch points toward the wider coast.",
                string.Empty,
                ExpeditionStateIds.LandmarkShallowsArch,
                new[] { "discovery.shallows.blip" },
                0,
                ZoneId.TidepoolShallows,
                1,
                string.Empty,
                ExpeditionStateIds.ChapterMeadow);

            Upsert(
                "chapter-meadow",
                ExpeditionStateIds.ChapterMeadow,
                ZoneId.SeagrassMeadow,
                "The Waving Path",
                "The meadow is waving again.",
                "Follow the broad grass wave to the unusual shell.",
                "The meadow showed the way through the kelp.",
                ExpeditionStateIds.ChapterShallows,
                ExpeditionStateIds.LandmarkMeadowWave,
                new[] { "discovery.meadow.gullwing", "discovery.meadow.tanglemaw" },
                1,
                ZoneId.SeagrassMeadow,
                5,
                string.Empty,
                ExpeditionStateIds.ChapterKelp);

            Upsert(
                "chapter-kelp",
                ExpeditionStateIds.ChapterKelp,
                ZoneId.KelpCurtain,
                "Lights Behind the Kelp",
                "Small lights are blinking behind the kelp.",
                "Follow two quiet clues toward the sheltered kelp lights.",
                "The kelp lights point toward the old stones.",
                ExpeditionStateIds.ChapterMeadow,
                ExpeditionStateIds.LandmarkKelpLights,
                new[] { "discovery.kelp.lumen", "discovery.kelp.tanglemaw" },
                2,
                ZoneId.KelpCurtain,
                3,
                string.Empty,
                ExpeditionStateIds.ChapterRocky);

            Upsert(
                "chapter-rocky",
                ExpeditionStateIds.ChapterRocky,
                ZoneId.RockyShelf,
                "The Old Stones",
                "The oldest shells are waiting near the foam.",
                "Follow the shell marks and listen for Old Barnaby.",
                "The coast remembers your Great Low Tide.",
                ExpeditionStateIds.ChapterKelp,
                ExpeditionStateIds.LandmarkRockyOldStones,
                new[] { "discovery.rocky.clackaw" },
                0,
                ZoneId.RockyShelf,
                0,
                "old-barnaby",
                string.Empty);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static ExpeditionChapter[] LoadAll()
        {
            return new[]
            {
                Load("chapter-shallows"),
                Load("chapter-meadow"),
                Load("chapter-kelp"),
                Load("chapter-rocky")
            };
        }

        private static ExpeditionChapter Load(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<ExpeditionChapter>($"{ChapterFolder}/{fileName}.asset");
        }

        private static void Upsert(
            string fileName,
            string id,
            ZoneId zone,
            string title,
            string openingCopy,
            string activeObjective,
            string completionSummary,
            string requiredChapterId,
            string requiredLandmarkStateId,
            string[] qualifyingDiscoveryIds,
            int minimumAuthoredDiscoveries,
            ZoneId catchZone,
            int minimumCaughtSpecies,
            string completionSpeciesId,
            string nextChapterId)
        {
            string path = $"{ChapterFolder}/{fileName}.asset";
            ExpeditionChapter chapter = AssetDatabase.LoadAssetAtPath<ExpeditionChapter>(path);
            if (chapter == null)
            {
                chapter = ScriptableObject.CreateInstance<ExpeditionChapter>();
                AssetDatabase.CreateAsset(chapter, path);
            }

            chapter.Configure(
                id,
                zone,
                title,
                openingCopy,
                activeObjective,
                completionSummary,
                requiredChapterId,
                requiredLandmarkStateId,
                qualifyingDiscoveryIds,
                minimumAuthoredDiscoveries,
                catchZone,
                minimumCaughtSpecies,
                completionSpeciesId,
                nextChapterId);
            EditorUtility.SetDirty(chapter);
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
