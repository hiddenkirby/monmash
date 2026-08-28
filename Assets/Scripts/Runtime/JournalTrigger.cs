using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tidepool.Runtime
{
    public class JournalTrigger : MonoBehaviour
    {
        [SerializeField] private PlayerGridMover playerMover;
        [SerializeField] private string journalSceneName = "Journal";

        private bool waitingForJournalClose;

        private void OnEnable()
        {
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= HandleSceneUnloaded;
        }

        public void OpenJournal()
        {
            if (string.IsNullOrWhiteSpace(journalSceneName))
            {
                return;
            }

            playerMover?.SetInputEnabled(false);
            waitingForJournalClose = true;
            SceneManager.LoadScene(journalSceneName, LoadSceneMode.Additive);
        }

        private void HandleSceneUnloaded(Scene scene)
        {
            if (!waitingForJournalClose || scene.name != journalSceneName)
            {
                return;
            }

            waitingForJournalClose = false;
            playerMover?.SetInputEnabled(true);
        }
    }
}
