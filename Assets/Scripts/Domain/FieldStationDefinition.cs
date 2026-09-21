using System;
using UnityEngine;

namespace Tidepool.Domain
{
    public enum FieldStationRequirementKind
    {
        Always,
        ChapterCompleted,
        MinimumCaughtSpecies,
        GrowthMemory,
        SetPieceCompleted,
        SpeciesCaught
    }

    [Serializable]
    public class FieldStationStage
    {
        [SerializeField] private string upgradeId;
        [SerializeField] private string displayName;
        [SerializeField, TextArea(2, 4)] private string description;
        [SerializeField] private FieldStationRequirementKind requirementKind;
        [SerializeField] private string requirementId;
        [SerializeField, Min(0)] private int minimumCaughtSpecies;

        public string UpgradeId => upgradeId;
        public string DisplayName => displayName;
        public string Description => description;
        public FieldStationRequirementKind RequirementKind => requirementKind;
        public string RequirementId => requirementId;
        public int MinimumCaughtSpecies => minimumCaughtSpecies;

        public FieldStationStage(
            string stageUpgradeId,
            string stageDisplayName,
            string stageDescription,
            FieldStationRequirementKind stageRequirementKind,
            string stageRequirementId = null,
            int stageMinimumCaughtSpecies = 0)
        {
            upgradeId = stageUpgradeId;
            displayName = stageDisplayName;
            description = stageDescription;
            requirementKind = stageRequirementKind;
            requirementId = stageRequirementId;
            minimumCaughtSpecies = Mathf.Max(0, stageMinimumCaughtSpecies);
        }
    }

    [CreateAssetMenu(menuName = "Tidepool/Field Station Definition", fileName = "NewFieldStationDefinition")]
    public class FieldStationDefinition : ScriptableObject
    {
        [SerializeField] private string title = "Field Station";
        [SerializeField, TextArea(2, 4)] private string fallbackDescription = "A quiet place for everything the coast remembers.";
        [SerializeField] private FieldStationStage[] stages = Array.Empty<FieldStationStage>();

        public string Title => title;
        public string FallbackDescription => fallbackDescription;
        public FieldStationStage[] Stages => stages ?? Array.Empty<FieldStationStage>();

#if UNITY_EDITOR
        public void Configure(string stationTitle, string stationFallbackDescription, FieldStationStage[] stationStages)
        {
            title = stationTitle;
            fallbackDescription = stationFallbackDescription;
            stages = stationStages ?? Array.Empty<FieldStationStage>();
        }
#endif
    }
}
