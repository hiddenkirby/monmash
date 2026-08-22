using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tidepool.Runtime
{
    public class JournalTrigger : MonoBehaviour
    {
        [SerializeField] private PlayerGridMover playerMover;
        [SerializeField] private string journalSceneName = "Journal";

        public void OpenJournal()
        {
            playerMover?.SetInputEnabled(false);
            if (!string.IsNullOrWhiteSpace(journalSceneName))
            {
                SceneManager.LoadScene(journalSceneName, LoadSceneMode.Additive);
            }
        }
    }
}
