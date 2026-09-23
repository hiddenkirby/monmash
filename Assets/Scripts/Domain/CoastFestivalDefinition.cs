using UnityEngine;

namespace Tidepool.Domain
{
    [CreateAssetMenu(menuName = "Tidepool/Coast Festival Definition", fileName = "NewCoastFestivalDefinition")]
    public class CoastFestivalDefinition : ScriptableObject
    {
        [SerializeField] private string requiredCompletedChapterId = ExpeditionStateIds.ChapterKelp;
        [SerializeField] private string requiredCompletedSetPieceId = ExpeditionStateIds.SetPieceRockyUnlock;
        [SerializeField, TextArea(1, 3)] private string firstInvitationText = "The whole coast is ready for one friendly contest.";
        [SerializeField, TextArea(1, 3)] private string returningInvitationText = "The festival ring is ready whenever you are.";
        [SerializeField, TextArea(1, 3)] private string winText = "A bright ribbon is waiting at the field station.";
        [SerializeField, TextArea(1, 3)] private string retryText = "Everyone cheers. Try again whenever you like.";
        [SerializeField] private string audioCueId = "soundscape.festival";

        public string RequiredCompletedChapterId => requiredCompletedChapterId;
        public string RequiredCompletedSetPieceId => requiredCompletedSetPieceId;
        public string FirstInvitationText => firstInvitationText;
        public string ReturningInvitationText => returningInvitationText;
        public string WinText => winText;
        public string RetryText => retryText;
        public string AudioCueId => audioCueId;

#if UNITY_EDITOR
        public void Configure()
        {
            requiredCompletedChapterId = ExpeditionStateIds.ChapterKelp;
            requiredCompletedSetPieceId = ExpeditionStateIds.SetPieceRockyUnlock;
            firstInvitationText = "The whole coast is ready for one friendly contest.";
            returningInvitationText = "The festival ring is ready whenever you are.";
            winText = "A bright ribbon is waiting at the field station.";
            retryText = "Everyone cheers. Try again whenever you like.";
            audioCueId = "soundscape.festival";
        }
#endif
    }
}
