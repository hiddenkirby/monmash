using System;
using System.Collections;
using Tidepool.Domain;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tidepool.Runtime
{
    [RequireComponent(typeof(Collider2D))]
    public class AuthoredDiscoveryTrigger : MonoBehaviour
    {
        private const string OldBarnabySpeciesId = "old-barnaby";

        [SerializeField] private AuthoredDiscoverySequence sequence;
        [SerializeField] private Transform playerRoot;
        [SerializeField] private PlayerGridMover playerMover;
        [SerializeField] private string catchSceneName = "CatchEncounter";
        [SerializeField] private GameObject presentationRoot;
        [SerializeField] private GameObject reducedMotionRoot;
        [SerializeField] private Text clueText;
        [SerializeField] private Button skipButton;
        [SerializeField, Min(0f)] private float autoLaunchSeconds = 1.5f;
        [SerializeField] private bool reducedMotion;
        [SerializeField] private UnityEvent discoveryStarted = new UnityEvent();
        [SerializeField] private UnityEvent presentationSkipped = new UnityEvent();
        [SerializeField] private UnityEvent encounterLaunched = new UnityEvent();

        private Coroutine runningPresentation;
        private bool encounterActive;
        private bool inputWasLocked;

        public AuthoredDiscoverySequence Sequence => sequence;
        public UnityEvent DiscoveryStarted => discoveryStarted;
        public UnityEvent PresentationSkipped => presentationSkipped;
        public UnityEvent EncounterLaunched => encounterLaunched;

        private void Reset()
        {
            Collider2D trigger = GetComponent<Collider2D>();
            if (trigger != null)
            {
                trigger.isTrigger = true;
            }
        }

        private void OnValidate()
        {
            Collider2D trigger = GetComponent<Collider2D>();
            if (trigger != null)
            {
                trigger.isTrigger = true;
            }
        }

        private void OnEnable()
        {
            if (skipButton != null)
            {
                skipButton.onClick.AddListener(SkipPresentation);
            }

            EncounterEvents.EncounterFinished += HandleEncounterFinished;
            SetPresentationVisible(false);
        }

        private void OnDisable()
        {
            if (skipButton != null)
            {
                skipButton.onClick.RemoveListener(SkipPresentation);
            }

            EncounterEvents.EncounterFinished -= HandleEncounterFinished;
            runningPresentation = null;
            encounterActive = false;
            RestoreInput();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other != null && ShouldStartFor(other.transform))
            {
                TryStartDiscovery();
            }
        }

        public bool TryStartDiscovery()
        {
            GameSaveService saveService = GameSaveService.Instance;
            TidelingSpecies species = ResolveSpecies();
            if (!IsEligible(saveService, species))
            {
                return false;
            }

            ApplyDiscoveryState(saveService);
            LockInput();
            SetPresentationVisible(true);
            if (clueText != null)
            {
                clueText.text = sequence.GetFallbackClueLine();
            }

            discoveryStarted?.Invoke();
            if (HasPresentation())
            {
                if (runningPresentation != null)
                {
                    StopCoroutine(runningPresentation);
                }

                runningPresentation = StartCoroutine(LaunchAfterDelay());
            }
            else
            {
                LaunchEncounter();
            }

            return true;
        }

        public bool IsEligible(GameSaveService saveService, TidelingSpecies species)
        {
            if (encounterActive
                || sequence == null
                || saveService == null
                || species == null
                || string.IsNullOrWhiteSpace(sequence.Id)
                || !species.LivesIn(sequence.Zone)
                || !saveService.IsZoneUnlocked(sequence.Zone)
                || !HasRequiredState(saveService))
            {
                return false;
            }

            switch (sequence.ReplayRule)
            {
                case AuthoredDiscoveryReplayRule.UntilCaught:
                    return saveService.FindCaught(species.Id) == null;
                case AuthoredDiscoveryReplayRule.OncePerSave:
                    return !saveService.HasAuthoredDiscovery(sequence.Id);
                case AuthoredDiscoveryReplayRule.Always:
                    return true;
                default:
                    return false;
            }
        }

        public void SkipPresentation()
        {
            if (runningPresentation != null)
            {
                StopCoroutine(runningPresentation);
                runningPresentation = null;
            }

            ApplyDiscoveryState(GameSaveService.Instance);
            presentationSkipped?.Invoke();
            LaunchEncounter();
        }

        public void SetReducedMotion(bool enabled)
        {
            reducedMotion = enabled;
            if (reducedMotionRoot != null)
            {
                reducedMotionRoot.SetActive(enabled);
            }
        }

        private IEnumerator LaunchAfterDelay()
        {
            if (autoLaunchSeconds > 0f)
            {
                yield return new WaitForSeconds(autoLaunchSeconds);
            }

            LaunchEncounter();
        }

        private void LaunchEncounter()
        {
            TidelingSpecies species = ResolveSpecies();
            if (species == null || encounterActive)
            {
                RestoreInput();
                return;
            }

            runningPresentation = null;
            encounterActive = true;
            SetPresentationVisible(false);
            EncounterContext.CurrentSpecies = species;
            EncounterContext.CurrentZone = sequence.Zone;
            EncounterContext.IsOldBarnabyEncounter = string.Equals(species.Id, OldBarnabySpeciesId, StringComparison.OrdinalIgnoreCase);
            EncounterContext.IsAuthoredDiscovery = true;
            EncounterContext.AuthoredDiscoveryId = sequence.Id;
            EncounterContext.EncounterIntroText = sequence.EncounterIntroText;
            EncounterContext.CatchCelebrationText = sequence.CatchCelebrationText;
            GameSaveService.Instance?.MarkSeen(species.Id);
            encounterLaunched?.Invoke();
            SceneManager.LoadScene(catchSceneName, LoadSceneMode.Additive);
        }

        private bool ShouldStartFor(Transform candidate)
        {
            if (candidate == null)
            {
                return false;
            }

            if (playerRoot != null)
            {
                return candidate == playerRoot || candidate.IsChildOf(playerRoot);
            }

            PlayerGridMover candidateMover = candidate.GetComponentInParent<PlayerGridMover>();
            return playerMover == null ? candidateMover != null : candidateMover == playerMover;
        }

        private bool HasRequiredState(GameSaveService saveService)
        {
            return HasOptionalState(sequence.RequiredCompletedChapterId, saveService.IsExpeditionChapterCompleted)
                && HasOptionalState(sequence.RequiredCompletedSetPieceId, saveService.HasCompletedSetPiece)
                && HasOptionalState(sequence.RequiredLandmarkStateId, saveService.HasLandmarkState);
        }

        private static bool HasOptionalState(string id, Func<string, bool> hasState)
        {
            return string.IsNullOrWhiteSpace(id) || hasState(id.Trim());
        }

        private void ApplyDiscoveryState(GameSaveService saveService)
        {
            if (saveService == null || sequence == null)
            {
                return;
            }

            saveService.RememberAuthoredDiscovery(sequence.Id);
            if (!string.IsNullOrWhiteSpace(sequence.LandmarkStateId))
            {
                saveService.RememberLandmarkState(sequence.LandmarkStateId);
            }

            if (!string.IsNullOrWhiteSpace(sequence.CompletionSetPieceId))
            {
                saveService.RememberCompletedSetPiece(sequence.CompletionSetPieceId);
            }
        }

        private TidelingSpecies ResolveSpecies()
        {
            if (sequence == null)
            {
                return null;
            }

            if (sequence.Species != null)
            {
                return sequence.Species;
            }

            return null;
        }

        private bool HasPresentation()
        {
            return presentationRoot != null || clueText != null || skipButton != null;
        }

        private void SetPresentationVisible(bool visible)
        {
            if (presentationRoot != null)
            {
                presentationRoot.SetActive(visible);
            }

            if (reducedMotionRoot != null)
            {
                reducedMotionRoot.SetActive(visible && reducedMotion);
            }
        }

        private void HandleEncounterFinished(bool caught)
        {
            if (!encounterActive)
            {
                return;
            }

            encounterActive = false;
            EncounterContext.Clear();
            RestoreInput();
        }

        private void LockInput()
        {
            if (playerMover == null || inputWasLocked)
            {
                return;
            }

            playerMover.SetInputEnabled(false);
            inputWasLocked = true;
        }

        private void RestoreInput()
        {
            if (playerMover == null || !inputWasLocked)
            {
                return;
            }

            playerMover.SetInputEnabled(true);
            inputWasLocked = false;
        }
    }
}
