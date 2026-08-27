using UnityEngine;

namespace Tidepool.Domain
{
    public enum StoryBeatTriggerCondition
    {
        OnFirstCatch,
        OnSpeciesCount,
        OnCaughtInZoneCount,
        OnZoneEntered,
        OnOldBarnaby
    }

    [CreateAssetMenu(menuName = "Tidepool/Story Beat", fileName = "NewStoryBeat")]
    public class StoryBeat : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private StoryBeatTriggerCondition triggerCondition = StoryBeatTriggerCondition.OnSpeciesCount;
        [SerializeField, Min(0)] private int triggerThreshold;
        [SerializeField] private ZoneId triggerZone = ZoneId.TidepoolShallows;
        [SerializeField] private Sprite npcSprite;
        [SerializeField, TextArea(1, 3)] private string dialogueText;
        [SerializeField] private bool unlockZoneOnTrigger;
        [SerializeField] private ZoneId unlockZoneId = ZoneId.KelpCurtain;

        public string Id => id;
        public StoryBeatTriggerCondition TriggerCondition => triggerCondition;
        public int TriggerThreshold => triggerThreshold;
        public ZoneId TriggerZone => triggerZone;
        public Sprite NpcSprite => npcSprite;
        public string DialogueText => dialogueText;
        public bool UnlockZoneOnTrigger => unlockZoneOnTrigger;
        public ZoneId UnlockZoneId => unlockZoneId;
    }
}
