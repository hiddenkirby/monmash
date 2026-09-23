using System.IO;
using Tidepool.Domain;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class CreateFieldStationDefinition
    {
        private const string DefinitionFolder = "Assets/Data/FieldStation";
        private const string DefinitionPath = DefinitionFolder + "/GreatLowTideFieldStation.asset";

        [MenuItem("Tools/Tidepool/Create Field Station Definition")]
        public static void CreateDefinition()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder(DefinitionFolder);

            FieldStationDefinition definition = AssetDatabase.LoadAssetAtPath<FieldStationDefinition>(DefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<FieldStationDefinition>();
                AssetDatabase.CreateAsset(definition, DefinitionPath);
            }

            definition.Configure(
                "Field Station",
                "A quiet place for everything the coast remembers.",
                new[]
                {
                    Stage(ExpeditionStateIds.StationShallows, "Coast Atlas Table", "The atlas is open to the warm shallows.", FieldStationRequirementKind.Always),
                    Stage(ExpeditionStateIds.StationSpeciesBoard, "Discovery Board", "New sketches remember the friends you found.", FieldStationRequirementKind.MinimumCaughtSpecies, minimumCaughtSpecies: 5),
                    Stage(ExpeditionStateIds.StationMeadow, "Meadow Drawing", "Pressed grass and a wide green drawing remember the waving path.", FieldStationRequirementKind.ChapterCompleted, ExpeditionStateIds.ChapterShallows),
                    Stage(ExpeditionStateIds.StationKelp, "Hanging Glow", "A soft light remembers the path behind the kelp.", FieldStationRequirementKind.ChapterCompleted, ExpeditionStateIds.ChapterMeadow),
                    Stage(ExpeditionStateIds.StationGrowthMemory, "Growing-Up Album", "A small album keeps every remembered form safe.", FieldStationRequirementKind.GrowthMemory),
                    Stage(ExpeditionStateIds.StationFestivalRibbon, "Festival Ribbon", "The coast festival left a bright ribbon for the station.", FieldStationRequirementKind.SetPieceCompleted, ExpeditionStateIds.SetPieceFestival),
                    Stage(ExpeditionStateIds.StationRocky, "Old-Stone Rubbing", "A careful rubbing remembers the quiet old stones.", FieldStationRequirementKind.ChapterCompleted, ExpeditionStateIds.ChapterKelp),
                    Stage(ExpeditionStateIds.StationFinale, "Coast Celebration", "The whole coast remembers your Great Low Tide.", FieldStationRequirementKind.SpeciesCaught, "old-barnaby")
                });

            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static FieldStationStage Stage(
            string upgradeId,
            string displayName,
            string description,
            FieldStationRequirementKind requirementKind,
            string requirementId = null,
            int minimumCaughtSpecies = 0)
        {
            return new FieldStationStage(upgradeId, displayName, description, requirementKind, requirementId, minimumCaughtSpecies);
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
