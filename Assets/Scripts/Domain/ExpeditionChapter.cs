using System;
using UnityEngine;

namespace Tidepool.Domain
{
    [CreateAssetMenu(menuName = "Tidepool/Expedition Chapter", fileName = "NewExpeditionChapter")]
    public class ExpeditionChapter : ScriptableObject
    {
        [SerializeField] private string id = ExpeditionStateIds.ChapterShallows;
        [SerializeField] private ZoneId zone = ZoneId.TidepoolShallows;
        [SerializeField] private string title = "The Coast Opens";
        [SerializeField, TextArea(1, 3)] private string openingCopy;
        [SerializeField, TextArea(1, 3)] private string activeObjective;
        [SerializeField, TextArea(1, 3)] private string completionSummary;
        [SerializeField] private string requiredCompletedChapterId;
        [SerializeField] private string requiredLandmarkStateId;
        [SerializeField] private string[] qualifyingDiscoveryIds = Array.Empty<string>();
        [SerializeField, Min(0)] private int minimumAuthoredDiscoveries;
        [SerializeField] private ZoneId catchZone = ZoneId.TidepoolShallows;
        [SerializeField, Min(0)] private int minimumCaughtSpecies;
        [SerializeField] private string completionSpeciesId;
        [SerializeField] private string nextChapterId;

        public string Id => id;
        public ZoneId Zone => zone;
        public string Title => title;
        public string OpeningCopy => openingCopy;
        public string ActiveObjective => activeObjective;
        public string CompletionSummary => completionSummary;
        public string RequiredCompletedChapterId => requiredCompletedChapterId;
        public string RequiredLandmarkStateId => requiredLandmarkStateId;
        public string[] QualifyingDiscoveryIds => qualifyingDiscoveryIds ?? Array.Empty<string>();
        public int MinimumAuthoredDiscoveries => minimumAuthoredDiscoveries;
        public ZoneId CatchZone => catchZone;
        public int MinimumCaughtSpecies => minimumCaughtSpecies;
        public string CompletionSpeciesId => completionSpeciesId;
        public string NextChapterId => nextChapterId;

#if UNITY_EDITOR
        public void Configure(
            string chapterId,
            ZoneId chapterZone,
            string chapterTitle,
            string chapterOpeningCopy,
            string chapterActiveObjective,
            string chapterCompletionSummary,
            string chapterRequiredCompletedChapterId,
            string chapterRequiredLandmarkStateId,
            string[] chapterQualifyingDiscoveryIds,
            int chapterMinimumAuthoredDiscoveries,
            ZoneId chapterCatchZone,
            int chapterMinimumCaughtSpecies,
            string chapterCompletionSpeciesId,
            string chapterNextChapterId)
        {
            id = chapterId;
            zone = chapterZone;
            title = chapterTitle;
            openingCopy = chapterOpeningCopy;
            activeObjective = chapterActiveObjective;
            completionSummary = chapterCompletionSummary;
            requiredCompletedChapterId = chapterRequiredCompletedChapterId;
            requiredLandmarkStateId = chapterRequiredLandmarkStateId;
            qualifyingDiscoveryIds = chapterQualifyingDiscoveryIds ?? Array.Empty<string>();
            minimumAuthoredDiscoveries = Mathf.Max(0, chapterMinimumAuthoredDiscoveries);
            catchZone = chapterCatchZone;
            minimumCaughtSpecies = Mathf.Max(0, chapterMinimumCaughtSpecies);
            completionSpeciesId = chapterCompletionSpeciesId;
            nextChapterId = chapterNextChapterId;
        }
#endif
    }
}
