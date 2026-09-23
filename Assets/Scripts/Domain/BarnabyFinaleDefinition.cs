using UnityEngine;

namespace Tidepool.Domain
{
    [CreateAssetMenu(menuName = "Tidepool/Barnaby Finale Definition", fileName = "NewBarnabyFinaleDefinition")]
    public class BarnabyFinaleDefinition : ScriptableObject
    {
        [SerializeField] private TidelingSpecies species;
        [SerializeField] private string requiredCompletedChapterId = ExpeditionStateIds.ChapterKelp;
        [SerializeField] private string requiredLandmarkStateId = ExpeditionStateIds.LandmarkRockyOldStones;
        [SerializeField, TextArea(1, 3)] private string invitationText = "The old stones answer with a slow, friendly ripple.";
        [SerializeField, TextArea(1, 3)] private string encounterIntroText = "That's Old Barnaby. He has been here since before the tidepools had names.";
        [SerializeField, TextArea(1, 3)] private string catchCelebrationText = "He chose you. The whole coast is celebrating.";
        [SerializeField, TextArea(1, 3)] private string memoryText = "The coast remembers the day Old Barnaby came to say hello.";
        [SerializeField] private string cameraCueId = "camera.finale.barnaby";
        [SerializeField] private string audioCueId = "soundscape.finale";

        public TidelingSpecies Species => species;
        public string RequiredCompletedChapterId => requiredCompletedChapterId;
        public string RequiredLandmarkStateId => requiredLandmarkStateId;
        public string InvitationText => invitationText;
        public string EncounterIntroText => encounterIntroText;
        public string CatchCelebrationText => catchCelebrationText;
        public string MemoryText => memoryText;
        public string CameraCueId => cameraCueId;
        public string AudioCueId => audioCueId;

#if UNITY_EDITOR
        public void Configure(TidelingSpecies finaleSpecies)
        {
            species = finaleSpecies;
            requiredCompletedChapterId = ExpeditionStateIds.ChapterKelp;
            requiredLandmarkStateId = ExpeditionStateIds.LandmarkRockyOldStones;
            invitationText = "The old stones answer with a slow, friendly ripple.";
            encounterIntroText = "That's Old Barnaby. He has been here since before the tidepools had names.";
            catchCelebrationText = "He chose you. The whole coast is celebrating.";
            memoryText = "The coast remembers the day Old Barnaby came to say hello.";
            cameraCueId = "camera.finale.barnaby";
            audioCueId = "soundscape.finale";
        }
#endif
    }
}
