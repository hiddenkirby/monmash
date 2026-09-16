using System;
using UnityEngine;

namespace Tidepool.Domain
{
    [CreateAssetMenu(menuName = "Tidepool/Authored Discovery Sequence", fileName = "NewAuthoredDiscoverySequence")]
    public class AuthoredDiscoverySequence : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private TidelingSpecies species;
        [SerializeField] private ZoneId zone = ZoneId.SeagrassMeadow;
        [SerializeField] private AuthoredDiscoveryReplayRule replayRule = AuthoredDiscoveryReplayRule.UntilCaught;
        [SerializeField] private string requiredCompletedChapterId;
        [SerializeField] private string requiredCompletedSetPieceId;
        [SerializeField] private string requiredLandmarkStateId;
        [SerializeField] private string completionSetPieceId;
        [SerializeField] private string landmarkStateId;
        [SerializeField] private string cameraCueId;
        [SerializeField] private string audioCueId;
        [SerializeField, TextArea(1, 3)] private string[] clueLines = Array.Empty<string>();
        [SerializeField, TextArea(1, 3)] private string encounterIntroText;
        [SerializeField, TextArea(1, 3)] private string catchCelebrationText;

        public string Id => id;
        public TidelingSpecies Species => species;
        public ZoneId Zone => zone;
        public AuthoredDiscoveryReplayRule ReplayRule => replayRule;
        public string RequiredCompletedChapterId => requiredCompletedChapterId;
        public string RequiredCompletedSetPieceId => requiredCompletedSetPieceId;
        public string RequiredLandmarkStateId => requiredLandmarkStateId;
        public string CompletionSetPieceId => completionSetPieceId;
        public string LandmarkStateId => landmarkStateId;
        public string CameraCueId => cameraCueId;
        public string AudioCueId => audioCueId;
        public string[] ClueLines => clueLines ?? Array.Empty<string>();
        public string EncounterIntroText => encounterIntroText;
        public string CatchCelebrationText => catchCelebrationText;

#if UNITY_EDITOR
        public void Configure(
            string id,
            TidelingSpecies species,
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
            this.id = id;
            this.species = species;
            this.zone = zone;
            this.replayRule = replayRule;
            this.requiredCompletedChapterId = requiredCompletedChapterId;
            this.requiredCompletedSetPieceId = requiredCompletedSetPieceId;
            this.requiredLandmarkStateId = requiredLandmarkStateId;
            this.completionSetPieceId = completionSetPieceId;
            this.landmarkStateId = landmarkStateId;
            this.cameraCueId = cameraCueId;
            this.audioCueId = audioCueId;
            this.clueLines = clueLines ?? Array.Empty<string>();
            this.encounterIntroText = encounterIntroText;
            this.catchCelebrationText = catchCelebrationText;
        }
#endif

        public string GetFallbackClueLine()
        {
            string[] lines = ClueLines;
            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    return lines[i].Trim();
                }
            }

            return "Something moves nearby.";
        }
    }
}
