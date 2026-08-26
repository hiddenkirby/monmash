using System.Collections.Generic;
using Tidepool.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Tidepool.Runtime
{
    public class StoryBeatDirector : MonoBehaviour
    {
        private const string OldBarnabySpeciesId = "old-barnaby";

        [SerializeField] private StoryBeat[] storyBeats;
        [SerializeField] private GameObject dialogueRoot;
        [SerializeField] private Image npcImage;
        [SerializeField] private Text dialogueText;
        [SerializeField] private Button continueButton;

        private readonly Queue<StoryBeat> pendingBeats = new Queue<StoryBeat>();
        private GameSaveService subscribedSaveService;
        private bool dialogueShowing;

        private void OnEnable()
        {
            SubscribeToSaveService();
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(DismissCurrentBeat);
            }
        }

        private void Start()
        {
            SubscribeToSaveService();
            EvaluateAllBeats();
        }

        private void OnDisable()
        {
            if (subscribedSaveService != null)
            {
                subscribedSaveService.SpeciesCaught -= HandleSpeciesCaught;
                subscribedSaveService.ZoneChanged -= HandleZoneChanged;
                subscribedSaveService = null;
            }

            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(DismissCurrentBeat);
            }
        }

        public void EvaluateAllBeats()
        {
            if (storyBeats == null)
            {
                return;
            }

            for (int i = 0; i < storyBeats.Length; i++)
            {
                TryFireBeat(storyBeats[i]);
            }
        }

        public void DismissCurrentBeat()
        {
            if (pendingBeats.Count > 0)
            {
                ShowBeat(pendingBeats.Dequeue());
                return;
            }

            dialogueShowing = false;
            if (dialogueRoot != null)
            {
                dialogueRoot.SetActive(false);
            }
        }

        private void SubscribeToSaveService()
        {
            GameSaveService saveService = GameSaveService.Instance;
            if (saveService == null || subscribedSaveService == saveService)
            {
                return;
            }

            if (subscribedSaveService != null)
            {
                subscribedSaveService.SpeciesCaught -= HandleSpeciesCaught;
                subscribedSaveService.ZoneChanged -= HandleZoneChanged;
            }

            subscribedSaveService = saveService;
            subscribedSaveService.SpeciesCaught += HandleSpeciesCaught;
            subscribedSaveService.ZoneChanged += HandleZoneChanged;
        }

        private void HandleSpeciesCaught(TidelingSpecies species, ZoneId zone)
        {
            EvaluateAllBeats();
        }

        private void HandleZoneChanged(ZoneId zone)
        {
            EvaluateAllBeats();
        }

        private void TryFireBeat(StoryBeat beat)
        {
            GameSaveService saveService = GameSaveService.Instance;
            if (beat == null || saveService == null || string.IsNullOrWhiteSpace(beat.Id) || saveService.HasTriggeredBeat(beat.Id))
            {
                return;
            }

            if (!IsBeatReady(beat, saveService))
            {
                return;
            }

            saveService.MarkBeatTriggered(beat.Id);
            if (beat.UnlockZoneOnTrigger)
            {
                saveService.UnlockZone(beat.UnlockZoneId);
            }

            QueueOrShowBeat(beat);
        }

        private static bool IsBeatReady(StoryBeat beat, GameSaveService saveService)
        {
            switch (beat.TriggerCondition)
            {
                case StoryBeatTriggerCondition.OnFirstCatch:
                    return saveService.CountCaughtSpecies() >= 1;
                case StoryBeatTriggerCondition.OnSpeciesCount:
                    return saveService.CountCaughtSpecies() >= beat.TriggerThreshold;
                case StoryBeatTriggerCondition.OnCaughtInZoneCount:
                    return saveService.CountCaughtSpeciesInZone(beat.TriggerZone) >= beat.TriggerThreshold;
                case StoryBeatTriggerCondition.OnZoneEntered:
                    return saveService.Data != null && saveService.Data.currentZone == beat.TriggerZone;
                case StoryBeatTriggerCondition.OnOldBarnaby:
                    return saveService.HasSeen(OldBarnabySpeciesId) || saveService.FindCaught(OldBarnabySpeciesId) != null;
                default:
                    return false;
            }
        }

        private void QueueOrShowBeat(StoryBeat beat)
        {
            if (dialogueShowing)
            {
                pendingBeats.Enqueue(beat);
                return;
            }

            ShowBeat(beat);
        }

        private void ShowBeat(StoryBeat beat)
        {
            if (dialogueRoot == null && npcImage == null && dialogueText == null)
            {
                dialogueShowing = false;
                return;
            }

            dialogueShowing = true;
            if (dialogueRoot != null)
            {
                dialogueRoot.SetActive(true);
            }

            if (npcImage != null)
            {
                npcImage.sprite = beat.NpcSprite;
                npcImage.enabled = beat.NpcSprite != null;
                npcImage.preserveAspect = true;
            }

            if (dialogueText != null)
            {
                dialogueText.text = string.IsNullOrWhiteSpace(beat.DialogueText)
                    ? "The tidepool has something to show you."
                    : beat.DialogueText;
            }
        }
    }
}
