using System.Collections;
using Tidepool.Domain;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Tidepool.Runtime
{
    public class RouteUnlockSequenceController : MonoBehaviour
    {
        [SerializeField] private RouteUnlockSequence sequence;
        [SerializeField] private PlayerGridMover playerMover;
        [SerializeField] private GameObject lockedVisualRoot;
        [SerializeField] private GameObject unlockedVisualRoot;
        [SerializeField] private GameObject presentationRoot;
        [SerializeField] private GameObject reducedMotionRoot;
        [SerializeField] private Text fallbackText;
        [SerializeField] private Button skipButton;
        [SerializeField, Min(0f)] private float autoCompleteSeconds = 2f;
        [SerializeField] private bool reducedMotion;
        [SerializeField] private UnityEvent sequenceStarted = new UnityEvent();
        [SerializeField] private UnityEvent durableStateApplied = new UnityEvent();
        [SerializeField] private UnityEvent sequenceSkipped = new UnityEvent();
        [SerializeField] private UnityEvent sequenceCompleted = new UnityEvent();

        private Coroutine runningSequence;
        private bool inputWasLocked;

        public RouteUnlockSequence Sequence => sequence;
        public UnityEvent SequenceStarted => sequenceStarted;
        public UnityEvent DurableStateApplied => durableStateApplied;
        public UnityEvent SequenceSkipped => sequenceSkipped;
        public UnityEvent SequenceCompleted => sequenceCompleted;

        private void OnEnable()
        {
            SubscribeToSkip();
            RefreshFromSave();
        }

        private void Start()
        {
            RefreshFromSave();
        }

        private void OnDisable()
        {
            UnsubscribeFromSkip();
            runningSequence = null;
            RestoreInput();
        }

        public bool PlayOrApply()
        {
            GameSaveService saveService = GameSaveService.Instance;
            if (sequence == null || saveService == null)
            {
                RefreshVisualState(false);
                return false;
            }

            bool changed = sequence.ApplyDurableState(saveService);
            durableStateApplied?.Invoke();
            RefreshVisualState(true);

            if (changed)
            {
                PlayPresentation();
            }

            return true;
        }

        public void RefreshFromSave()
        {
            bool open = sequence != null
                && GameSaveService.Instance != null
                && GameSaveService.Instance.IsZoneUnlocked(sequence.DestinationZone);
            RefreshVisualState(open);
            if (presentationRoot != null && runningSequence == null)
            {
                presentationRoot.SetActive(false);
            }
        }

        public void Skip()
        {
            if (sequence != null && GameSaveService.Instance != null)
            {
                sequence.ApplyDurableState(GameSaveService.Instance);
            }

            sequenceSkipped?.Invoke();
            CompletePresentation();
        }

        public void SetReducedMotion(bool enabled)
        {
            reducedMotion = enabled;
            if (reducedMotionRoot != null)
            {
                reducedMotionRoot.SetActive(enabled);
            }
        }

        private void PlayPresentation()
        {
            if (presentationRoot == null && fallbackText == null && skipButton == null)
            {
                sequenceCompleted?.Invoke();
                return;
            }

            if (runningSequence != null)
            {
                StopCoroutine(runningSequence);
            }

            LockInput();
            if (fallbackText != null)
            {
                fallbackText.text = string.IsNullOrWhiteSpace(sequence.FallbackMessage)
                    ? "The path opens."
                    : sequence.FallbackMessage;
            }

            if (presentationRoot != null)
            {
                presentationRoot.SetActive(true);
            }

            if (reducedMotionRoot != null)
            {
                reducedMotionRoot.SetActive(reducedMotion);
            }

            sequenceStarted?.Invoke();
            runningSequence = StartCoroutine(AutoCompleteAfterDelay());
        }

        private IEnumerator AutoCompleteAfterDelay()
        {
            if (autoCompleteSeconds > 0f)
            {
                yield return new WaitForSeconds(autoCompleteSeconds);
            }

            CompletePresentation();
        }

        private void CompletePresentation()
        {
            if (runningSequence != null)
            {
                StopCoroutine(runningSequence);
                runningSequence = null;
            }

            if (presentationRoot != null)
            {
                presentationRoot.SetActive(false);
            }

            RestoreInput();
            RefreshFromSave();
            sequenceCompleted?.Invoke();
        }

        private void RefreshVisualState(bool open)
        {
            if (lockedVisualRoot != null)
            {
                lockedVisualRoot.SetActive(!open);
            }

            if (unlockedVisualRoot != null)
            {
                unlockedVisualRoot.SetActive(open);
            }
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

        private void SubscribeToSkip()
        {
            if (skipButton != null)
            {
                skipButton.onClick.AddListener(Skip);
            }
        }

        private void UnsubscribeFromSkip()
        {
            if (skipButton != null)
            {
                skipButton.onClick.RemoveListener(Skip);
            }
        }
    }
}
