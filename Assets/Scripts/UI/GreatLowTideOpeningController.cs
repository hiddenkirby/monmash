using Tidepool.Domain;
using Tidepool.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Tidepool.UI
{
    public enum BootPresentationMode
    {
        Opening,
        Returning
    }

    public class GreatLowTideOpeningController : MonoBehaviour
    {
        [SerializeField] private BootRouter bootRouter;
        [SerializeField] private GameObject openingRoot;
        [SerializeField] private GameObject returningRoot;
        [SerializeField] private Text returningChapterText;
        [SerializeField] private Text returningSummaryText;
        [SerializeField] private Button openingContinueButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private Button returningContinueButton;
        [SerializeField] private Toggle reducedMotionToggle;
        [SerializeField] private RectTransform[] panoramaLayers;
        [SerializeField] private float panoramaDrift = 7f;
        [SerializeField] private AudioSource openingAudioSource;

        private Vector2[] panoramaStartPositions;
        private BootPresentationMode presentationMode;

        private void OnEnable()
        {
            BindControls();
        }

        private void Start()
        {
            CapturePanoramaPositions();
            if (reducedMotionToggle != null)
            {
                reducedMotionToggle.SetIsOnWithoutNotify(TidepoolSettingsService.ReducedMotion);
            }

            GameSaveService saveService = GameSaveService.Instance;
            presentationMode = DeterminePresentation(saveService);
            if (presentationMode == BootPresentationMode.Opening && saveService != null)
            {
                saveService.RememberCompletedSetPiece(ExpeditionStateIds.SetPieceOpening);
            }

            ShowPresentation(saveService);
        }

        private void Update()
        {
            if (presentationMode != BootPresentationMode.Opening
                || TidepoolSettingsService.ReducedMotion
                || panoramaLayers == null
                || panoramaStartPositions == null)
            {
                return;
            }

            for (int i = 0; i < panoramaLayers.Length && i < panoramaStartPositions.Length; i++)
            {
                RectTransform layer = panoramaLayers[i];
                if (layer == null)
                {
                    continue;
                }

                float direction = i % 2 == 0 ? 1f : -1f;
                float distance = panoramaDrift * (i + 1f) / panoramaLayers.Length;
                layer.anchoredPosition = panoramaStartPositions[i]
                    + Vector2.right * (Mathf.Sin(Time.unscaledTime * 0.25f + i) * distance * direction);
            }
        }

        private void OnDisable()
        {
            UnbindControls();
        }

        public void ContinueToCoast()
        {
            StopOpeningAudio();
            bootRouter?.ContinueToOverworld();
        }

        public void SetReducedMotion(bool reducedMotion)
        {
            TidepoolSettingsService.SetReducedMotion(reducedMotion);
            if (reducedMotion)
            {
                RestorePanoramaPositions();
            }
        }

        public static BootPresentationMode DeterminePresentation(GameSaveService saveService)
        {
            if (saveService == null || saveService.Data == null)
            {
                return BootPresentationMode.Opening;
            }

            bool openingSeen = saveService.HasCompletedSetPiece(ExpeditionStateIds.SetPieceOpening);
            return !openingSeen && !saveService.HasAnyProgress()
                ? BootPresentationMode.Opening
                : BootPresentationMode.Returning;
        }

        public static string GetChapterTitle(string chapterId)
        {
            switch (chapterId)
            {
                case ExpeditionStateIds.ChapterMeadow:
                    return "The Waving Path";
                case ExpeditionStateIds.ChapterKelp:
                    return "Lights Behind the Kelp";
                case ExpeditionStateIds.ChapterRocky:
                    return "The Old Stones";
                case ExpeditionStateIds.ChapterShallows:
                    return "The Coast Opens";
                default:
                    return "The coast is waiting";
            }
        }

        public static string GetReturnSummary(string chapterId)
        {
            switch (chapterId)
            {
                case ExpeditionStateIds.ChapterMeadow:
                    return "The grass is waving. See what the meadow wants to show you.";
                case ExpeditionStateIds.ChapterKelp:
                    return "A soft light is moving beyond the tall kelp.";
                case ExpeditionStateIds.ChapterRocky:
                    return "The old stones are ready for careful steps.";
                case ExpeditionStateIds.ChapterShallows:
                    return "Small ripples and bright shells are waiting in the shallows.";
                default:
                    return "Wander a little. The coast is good at surprises.";
            }
        }

        private void ShowPresentation(GameSaveService saveService)
        {
            bool showOpening = presentationMode == BootPresentationMode.Opening;
            if (openingRoot != null)
            {
                openingRoot.SetActive(showOpening);
            }

            if (returningRoot != null)
            {
                returningRoot.SetActive(!showOpening);
            }

            if (showOpening)
            {
                if (openingRoot == null)
                {
                    ContinueToCoast();
                    return;
                }

                PlayOpeningAudio();
                return;
            }

            if (returningRoot == null)
            {
                ContinueToCoast();
                return;
            }

            string chapterId = saveService?.Data?.activeExpeditionChapterId;
            if (returningChapterText != null)
            {
                returningChapterText.text = GetChapterTitle(chapterId);
            }

            if (returningSummaryText != null)
            {
                returningSummaryText.text = saveService != null
                    && saveService.IsExpeditionChapterCompleted(ExpeditionStateIds.ChapterRocky)
                    ? "The coast remembers your Great Low Tide."
                    : GetReturnSummary(chapterId);
            }
        }

        private void BindControls()
        {
            UnbindControls();
            openingContinueButton?.onClick.AddListener(ContinueToCoast);
            skipButton?.onClick.AddListener(ContinueToCoast);
            returningContinueButton?.onClick.AddListener(ContinueToCoast);
            reducedMotionToggle?.onValueChanged.AddListener(SetReducedMotion);
        }

        private void UnbindControls()
        {
            openingContinueButton?.onClick.RemoveListener(ContinueToCoast);
            skipButton?.onClick.RemoveListener(ContinueToCoast);
            returningContinueButton?.onClick.RemoveListener(ContinueToCoast);
            reducedMotionToggle?.onValueChanged.RemoveListener(SetReducedMotion);
        }

        private void CapturePanoramaPositions()
        {
            if (panoramaLayers == null)
            {
                panoramaStartPositions = null;
                return;
            }

            panoramaStartPositions = new Vector2[panoramaLayers.Length];
            for (int i = 0; i < panoramaLayers.Length; i++)
            {
                panoramaStartPositions[i] = panoramaLayers[i] == null
                    ? Vector2.zero
                    : panoramaLayers[i].anchoredPosition;
            }
        }

        private void RestorePanoramaPositions()
        {
            if (panoramaLayers == null || panoramaStartPositions == null)
            {
                return;
            }

            for (int i = 0; i < panoramaLayers.Length && i < panoramaStartPositions.Length; i++)
            {
                if (panoramaLayers[i] != null)
                {
                    panoramaLayers[i].anchoredPosition = panoramaStartPositions[i];
                }
            }
        }

        private void PlayOpeningAudio()
        {
            if (openingAudioSource == null || openingAudioSource.clip == null || TidepoolSettingsService.Muted)
            {
                return;
            }

            openingAudioSource.volume = TidepoolSettingsService.MasterVolume;
            openingAudioSource.Play();
        }

        private void StopOpeningAudio()
        {
            if (openingAudioSource != null && openingAudioSource.isPlaying)
            {
                openingAudioSource.Stop();
            }
        }
    }
}
