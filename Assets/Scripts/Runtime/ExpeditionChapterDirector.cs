using Tidepool.Domain;
using UnityEngine;

namespace Tidepool.Runtime
{
    public class ExpeditionChapterDirector : MonoBehaviour
    {
        [SerializeField] private ExpeditionChapter[] chapters;

        private GameSaveService saveService;
        private bool refreshing;

        private void Start()
        {
            Subscribe();
            RefreshProgress();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void RefreshProgress()
        {
            Subscribe();
            if (refreshing || saveService == null || saveService.Data == null || chapters == null)
            {
                return;
            }

            refreshing = true;
            try
            {
                for (int i = 0; i < chapters.Length; i++)
                {
                    ExpeditionChapter chapter = chapters[i];
                    if (chapter == null || saveService.IsExpeditionChapterCompleted(chapter.Id))
                    {
                        continue;
                    }

                    if (ExpeditionChapterProgress.IsCompletionReady(chapter, saveService.Data)
                        || ExpeditionChapterProgress.ShouldBackfillCompletion(chapter, saveService.Data))
                    {
                        saveService.CompleteExpeditionChapter(chapter.Id);
                        if (!string.IsNullOrWhiteSpace(chapter.NextChapterId))
                        {
                            saveService.SetActiveExpeditionChapter(chapter.NextChapterId);
                        }
                    }
                }
            }
            finally
            {
                refreshing = false;
            }
        }

        private void Subscribe()
        {
            GameSaveService current = GameSaveService.Instance;
            if (current == saveService)
            {
                return;
            }

            Unsubscribe();
            saveService = current;
            if (saveService != null)
            {
                saveService.SpeciesCaught += HandleSpeciesCaught;
                saveService.ZoneChanged += HandleZoneChanged;
                saveService.ExpeditionStateChanged += RefreshProgress;
            }
        }

        private void Unsubscribe()
        {
            if (saveService == null)
            {
                return;
            }

            saveService.SpeciesCaught -= HandleSpeciesCaught;
            saveService.ZoneChanged -= HandleZoneChanged;
            saveService.ExpeditionStateChanged -= RefreshProgress;
            saveService = null;
        }

        private void HandleSpeciesCaught(TidelingSpecies species, ZoneId zone)
        {
            RefreshProgress();
        }

        private void HandleZoneChanged(ZoneId zone)
        {
            RefreshProgress();
        }
    }
}
