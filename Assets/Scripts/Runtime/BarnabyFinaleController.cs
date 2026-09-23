using System.Collections;
using Tidepool.Domain;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tidepool.Runtime
{
    public class BarnabyFinaleController : MonoBehaviour
    {
        [SerializeField] private BarnabyFinaleDefinition definition;
        [SerializeField] private PlayerGridMover playerMover;
        [SerializeField] private string catchSceneName = "CatchEncounter";
        [SerializeField] private GameObject invitationRoot;
        [SerializeField] private GameObject presentationRoot;
        [SerializeField] private GameObject reducedMotionRoot;
        [SerializeField] private GameObject celebrationRoot;
        [SerializeField] private Text presentationText;
        [SerializeField] private Button skipButton;
        [SerializeField, Min(0f)] private float autoLaunchSeconds = 2f;
        [SerializeField] private bool reducedMotion;
        [SerializeField] private UnityEvent finaleStarted = new UnityEvent();
        [SerializeField] private UnityEvent finaleCompleted = new UnityEvent();
        [SerializeField] private UnityEvent memoryOpened = new UnityEvent();

        private Coroutine runningPresentation;
        private bool encounterActive;
        private bool inputWasLocked;

        public BarnabyFinaleDefinition Definition => definition;
        public UnityEvent FinaleStarted => finaleStarted;
        public UnityEvent FinaleCompleted => finaleCompleted;
        public UnityEvent MemoryOpened => memoryOpened;

        private void OnEnable()
        {
            if (skipButton != null)
            {
                skipButton.onClick.AddListener(SkipPresentation);
            }

            EncounterEvents.EncounterFinished += HandleEncounterFinished;
            BarnabyFinaleProgress.ReconcileCompletion(GameSaveService.Instance);
            RefreshPresentation();
        }

        private void OnDisable()
        {
            if (skipButton != null)
            {
                skipButton.onClick.RemoveListener(SkipPresentation);
            }

            EncounterEvents.EncounterFinished -= HandleEncounterFinished;
            if (runningPresentation != null)
            {
                StopCoroutine(runningPresentation);
                runningPresentation = null;
            }

            encounterActive = false;
            RestoreInput();
        }

        public bool TryBeginFinale()
        {
            GameSaveService saveService = GameSaveService.Instance;
            if (encounterActive || !BarnabyFinaleProgress.IsAvailable(definition, saveService?.Data))
            {
                return false;
            }

            BarnabyFinaleProgress.RememberAttempt(saveService);
            LockInput();
            SetActive(invitationRoot, false);
            SetActive(presentationRoot, true);
            SetActive(reducedMotionRoot, reducedMotion);
            if (presentationText != null)
            {
                presentationText.text = definition.InvitationText;
            }

            finaleStarted?.Invoke();
            if (HasPresentation() && autoLaunchSeconds > 0f)
            {
                runningPresentation = StartCoroutine(LaunchAfterDelay());
            }
            else
            {
                LaunchEncounter();
            }

            return true;
        }

        // Void wrappers so scene generators can attach persistent button listeners
        // (UnityAction cannot bind to bool-returning methods).
        public void BeginFinale()
        {
            TryBeginFinale();
        }

        public void ShowMemory()
        {
            OpenMemory();
        }

        public void SkipPresentation()
        {
            if (runningPresentation != null)
            {
                StopCoroutine(runningPresentation);
                runningPresentation = null;
            }

            if (!encounterActive)
            {
                LaunchEncounter();
            }
        }

        public void SetReducedMotion(bool enabled)
        {
            reducedMotion = enabled;
            SetActive(reducedMotionRoot, enabled && presentationRoot != null && presentationRoot.activeSelf);
        }

        public bool OpenMemory()
        {
            GameSaveService saveService = GameSaveService.Instance;
            if (!BarnabyFinaleProgress.IsCompleted(saveService?.Data))
            {
                return false;
            }

            BarnabyFinaleProgress.RememberMemorySeen(saveService);
            if (presentationText != null)
            {
                presentationText.text = definition == null ? string.Empty : definition.MemoryText;
            }

            memoryOpened?.Invoke();
            return true;
        }

        private IEnumerator LaunchAfterDelay()
        {
            yield return new WaitForSeconds(autoLaunchSeconds);
            LaunchEncounter();
        }

        private void LaunchEncounter()
        {
            if (definition == null || definition.Species == null || encounterActive)
            {
                RestoreInput();
                return;
            }

            runningPresentation = null;
            encounterActive = true;
            SetActive(presentationRoot, false);
            SetActive(reducedMotionRoot, false);
            EncounterContext.CurrentSpecies = definition.Species;
            EncounterContext.CurrentZone = ZoneId.RockyShelf;
            EncounterContext.IsOldBarnabyEncounter = true;
            EncounterContext.IsAuthoredDiscovery = true;
            EncounterContext.AuthoredDiscoveryId = ExpeditionStateIds.FinaleBarnabyMet;
            EncounterContext.EncounterIntroText = definition.EncounterIntroText;
            EncounterContext.CatchCelebrationText = definition.CatchCelebrationText;
            GameSaveService.Instance?.MarkSeen(definition.Species.Id);
            SceneManager.LoadScene(catchSceneName, LoadSceneMode.Additive);
        }

        private void HandleEncounterFinished(bool caught)
        {
            if (!encounterActive)
            {
                return;
            }

            encounterActive = false;
            EncounterContext.Clear();
            if (caught)
            {
                BarnabyFinaleProgress.ReconcileCompletion(GameSaveService.Instance);
                finaleCompleted?.Invoke();
            }

            RestoreInput();
            RefreshPresentation();
        }

        private void RefreshPresentation()
        {
            SaveData data = GameSaveService.Instance?.Data;
            bool complete = BarnabyFinaleProgress.IsCompleted(data);
            SetActive(celebrationRoot, complete);
            SetActive(invitationRoot, !complete && BarnabyFinaleProgress.IsAvailable(definition, data));
            SetActive(presentationRoot, false);
            SetActive(reducedMotionRoot, false);
        }

        private bool HasPresentation()
        {
            return presentationRoot != null || reducedMotionRoot != null || presentationText != null || skipButton != null;
        }

        private void LockInput()
        {
            if (playerMover != null && !inputWasLocked)
            {
                playerMover.SetInputEnabled(false);
                inputWasLocked = true;
            }
        }

        private void RestoreInput()
        {
            if (playerMover != null && inputWasLocked)
            {
                playerMover.SetInputEnabled(true);
                inputWasLocked = false;
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
