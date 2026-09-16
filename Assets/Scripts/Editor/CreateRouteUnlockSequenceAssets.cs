using System.IO;
using Tidepool.Domain;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class CreateRouteUnlockSequenceAssets
    {
        public const string RouteUnlockFolder = "Assets/Data/RouteUnlockSequences";

        [MenuItem("Tools/Tidepool/Create Route Unlock Sequence Assets")]
        public static void CreateAssets()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder(RouteUnlockFolder);

            UpsertSequence(
                "route-unlock.meadow",
                ZoneId.SeagrassMeadow,
                ExpeditionStateIds.ChapterShallows,
                ExpeditionStateIds.ChapterMeadow,
                ExpeditionStateIds.LandmarkShallowsArch,
                ExpeditionStateIds.StationShallows,
                ExpeditionStateIds.SetPieceMeadowUnlock,
                "The arch points toward the meadow.");

            UpsertSequence(
                "route-unlock.kelp",
                ZoneId.KelpCurtain,
                ExpeditionStateIds.ChapterMeadow,
                ExpeditionStateIds.ChapterKelp,
                ExpeditionStateIds.LandmarkMeadowWave,
                ExpeditionStateIds.StationMeadow,
                ExpeditionStateIds.SetPieceKelpUnlock,
                "The kelp parts into a quiet path.");

            UpsertSequence(
                "route-unlock.rocky",
                ZoneId.RockyShelf,
                ExpeditionStateIds.ChapterKelp,
                ExpeditionStateIds.ChapterRocky,
                ExpeditionStateIds.LandmarkKelpLights,
                ExpeditionStateIds.StationKelp,
                ExpeditionStateIds.SetPieceRockyUnlock,
                "The old stones are ready to be noticed.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static RouteUnlockSequence LoadForDestination(ZoneId destinationZone)
        {
            return AssetDatabase.LoadAssetAtPath<RouteUnlockSequence>(
                $"{RouteUnlockFolder}/{GetFileName(destinationZone)}.asset");
        }

        private static void UpsertSequence(
            string id,
            ZoneId destinationZone,
            string completedChapterId,
            string nextActiveChapterId,
            string landmarkStateId,
            string fieldStationUpgradeId,
            string completedSetPieceId,
            string fallbackMessage)
        {
            string path = $"{RouteUnlockFolder}/{GetFileName(destinationZone)}.asset";
            RouteUnlockSequence sequence = AssetDatabase.LoadAssetAtPath<RouteUnlockSequence>(path);
            if (sequence == null)
            {
                sequence = ScriptableObject.CreateInstance<RouteUnlockSequence>();
                AssetDatabase.CreateAsset(sequence, path);
            }

            sequence.Configure(
                id,
                destinationZone,
                completedChapterId,
                nextActiveChapterId,
                landmarkStateId,
                fieldStationUpgradeId,
                completedSetPieceId,
                fallbackMessage);
            EditorUtility.SetDirty(sequence);
        }

        private static string GetFileName(ZoneId destinationZone)
        {
            switch (destinationZone)
            {
                case ZoneId.SeagrassMeadow:
                    return "route-unlock-meadow";
                case ZoneId.KelpCurtain:
                    return "route-unlock-kelp";
                case ZoneId.RockyShelf:
                    return "route-unlock-rocky";
                default:
                    return "route-unlock-shallows";
            }
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
