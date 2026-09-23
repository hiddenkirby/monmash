using System.Collections;
using Tidepool.Domain;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Tidepool.Runtime
{
    public class CoastFestivalController : MonoBehaviour
    {
        [SerializeField] private CoastFestivalDefinition definition;
        [SerializeField] private ContestTrigger contestTrigger;
        [SerializeField] private GameObject invitationRoot;
        [SerializeField] private GameObject presentationRoot;
        [SerializeField] private GameObject reducedMotionRoot;
        [SerializeField] private GameObject celebrationRoot;
        [SerializeField] private Text presentationText;
        [SerializeField] private Button skipButton;
        [SerializeField, Min(0f)] private float introductionSeconds = 1.5f;
        [SerializeField] private bool reducedMotion;
        [SerializeField] private UnityEvent festivalStarted = new UnityEvent();
        [SerializeField] private UnityEvent festivalWon = new UnityEvent();
        [SerializeField] private UnityEvent festivalRetryOffered = new UnityEvent();

        private Coroutine introductionRoutine;
        private bool festivalContestActive;

        public CoastFestivalDefinition Definition => definition;
        public UnityEvent FestivalStarted => festivalStarted;
        public UnityEvent FestivalWon => festivalWon;
        public UnityEvent FestivalRetryOffered => festivalRetryOffered;

        private void OnEnable()
        {
            if (skipButton != null)
            {
                skipButton.onClick.AddListener(SkipIntroduction);
            }

            ContestEvents.ContestResolved += HandleContestResolved;
            ContestEvents.ContestFinished += HandleContestFinished;
            CoastFestivalProgress.Reconcile(GameSaveService.Instance);
            RefreshPresentation();
        }

        private void OnDisable()
        {
            if (skipButton != null)
            {
                skipButton.onClick.RemoveListener(SkipIntroduction);
            }

            ContestEvents.ContestResolved -= HandleContestResolved;
            ContestEvents.ContestFinished -= HandleContestFinished;
            if (introductionRoutine != null)
            {
                StopCoroutine(introductionRoutine);
                introductionRoutine = null;
            }

            festivalContestActive = false;
        }

        public bool TryStartFestival()
        {
            GameSaveService saveService = GameSaveService.Instance;
            if (festivalContestActive
                || contestTrigger == null
                || !CoastFestivalProgress.CanEnter(definition, saveService?.Data))
            {
                return false;
            }

            SetActive(invitationRoot, false);
            SetActive(presentationRoot, true);
            SetActive(reducedMotionRoot, reducedMotion);
            if (presentationText != null)
            {
                presentationText.text = CoastFestivalProgress.IsCompleted(saveService.Data)
                    ? definition.ReturningInvitationText
                    : definition.FirstInvitationText;
            }

            festivalStarted?.Invoke();
            if (HasPresentation() && introductionSeconds > 0f)
            {
                introductionRoutine = StartCoroutine(StartContestAfterIntroduction());
            }
            else
            {
                StartContest();
            }

            return true;
        }

        public void SkipIntroduction()
        {
            if (introductionRoutine != null)
            {
                StopCoroutine(introductionRoutine);
                introductionRoutine = null;
            }

            if (!festivalContestActive)
            {
                StartContest();
            }
        }

        public void SetReducedMotion(bool enabled)
        {
            reducedMotion = enabled;
            SetActive(reducedMotionRoot, enabled && presentationRoot != null && presentationRoot.activeSelf);
        }

        private IEnumerator StartContestAfterIntroduction()
        {
            yield return new WaitForSecondsRealtime(introductionSeconds);
            StartContest();
        }

        private void StartContest()
        {
            introductionRoutine = null;
            SetActive(presentationRoot, false);
            SetActive(reducedMotionRoot, false);
            festivalContestActive = contestTrigger != null && contestTrigger.TryStartContest();
            if (!festivalContestActive)
            {
                RefreshPresentation();
            }
        }

        private void HandleContestResolved(ContestOutcome outcome)
        {
            if (!festivalContestActive)
            {
                return;
            }

            if (outcome == ContestOutcome.PlayerWin)
            {
                CoastFestivalProgress.RememberWin(GameSaveService.Instance);
                if (presentationText != null)
                {
                    presentationText.text = definition == null ? string.Empty : definition.WinText;
                }

                festivalWon?.Invoke();
                SetActive(celebrationRoot, true);
                return;
            }

            if (presentationText != null)
            {
                presentationText.text = definition == null ? string.Empty : definition.RetryText;
            }

            festivalRetryOffered?.Invoke();
        }

        private void HandleContestFinished()
        {
            if (!festivalContestActive)
            {
                return;
            }

            festivalContestActive = false;
            RefreshPresentation();
        }

        private void RefreshPresentation()
        {
            SaveData data = GameSaveService.Instance?.Data;
            SetActive(invitationRoot, CoastFestivalProgress.CanEnter(definition, data));
            SetActive(celebrationRoot, CoastFestivalProgress.IsCompleted(data));
            SetActive(presentationRoot, false);
            SetActive(reducedMotionRoot, false);
        }

        private bool HasPresentation()
        {
            return presentationRoot != null || reducedMotionRoot != null || presentationText != null || skipButton != null;
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
