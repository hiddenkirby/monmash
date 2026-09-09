using System.IO;
using Tidepool.Domain;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class CreateCoastAtlasDefinition
    {
        private const string CoastAtlasFolder = "Assets/Data/CoastAtlas";
        private const string CoastAtlasPath = CoastAtlasFolder + "/GreatLowTideCoastAtlas.asset";

        [MenuItem("Tools/Tidepool/Create Coast Atlas Definition")]
        public static void CreateDefinition()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder(CoastAtlasFolder);

            CoastAtlasDefinition definition = AssetDatabase.LoadAssetAtPath<CoastAtlasDefinition>(CoastAtlasPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CoastAtlasDefinition>();
                AssetDatabase.CreateAsset(definition, CoastAtlasPath);
            }

            definition.Configure(
                "The Great Low Tide",
                "The coast is waiting to be noticed.",
                new[]
                {
                    Node(
                        "atlas.shallows",
                        ZoneId.TidepoolShallows,
                        ExpeditionStateIds.ChapterShallows,
                        null,
                        ExpeditionStateIds.SetPieceMeadowUnlock,
                        "Tidepool Shallows",
                        "Follow the shell glint near the stone arch.",
                        "The arch points toward the wider coast.",
                        "Start in the warm shallows.",
                        new Vector2(0.18f, 0.45f)),
                    Node(
                        "atlas.meadow",
                        ZoneId.SeagrassMeadow,
                        ExpeditionStateIds.ChapterMeadow,
                        ExpeditionStateIds.ChapterShallows,
                        ExpeditionStateIds.SetPieceKelpUnlock,
                        "Seagrass Meadow",
                        "Look for the grass that waves in a broad path.",
                        "The meadow showed the way through the kelp.",
                        "The meadow path opens from the shallows.",
                        new Vector2(0.38f, 0.55f)),
                    Node(
                        "atlas.kelp",
                        ZoneId.KelpCurtain,
                        ExpeditionStateIds.ChapterKelp,
                        ExpeditionStateIds.ChapterMeadow,
                        ExpeditionStateIds.SetPieceRockyUnlock,
                        "Kelp Curtain",
                        "Follow the quiet lights behind the kelp.",
                        "The kelp lights point toward the old stones.",
                        "The kelp parts after the meadow is ready.",
                        new Vector2(0.62f, 0.48f)),
                    Node(
                        "atlas.rocky",
                        ZoneId.RockyShelf,
                        ExpeditionStateIds.ChapterRocky,
                        ExpeditionStateIds.ChapterKelp,
                        ExpeditionStateIds.SetPieceFinale,
                        "Rocky Shelf",
                        "Listen for the old stones near the foam.",
                        "The coast remembers Old Barnaby.",
                        "The shelf appears after the kelp path is known.",
                        new Vector2(0.82f, 0.57f))
                });

            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static CoastAtlasNode Node(
            string id,
            ZoneId zone,
            string chapterId,
            string requiredChapterId,
            string completionSetPieceId,
            string displayName,
            string activeObjective,
            string completedSummary,
            string lockedHint,
            Vector2 normalizedPosition)
        {
            return new CoastAtlasNode(
                id,
                zone,
                chapterId,
                requiredChapterId,
                completionSetPieceId,
                displayName,
                activeObjective,
                completedSummary,
                lockedHint,
                normalizedPosition);
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
