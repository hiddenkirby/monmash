using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tidepool.Runtime
{
    public class CharacterSelectTrigger : MonoBehaviour
    {
        [SerializeField] private PlayerGridMover playerMover;
        [SerializeField] private string characterSelectSceneName = "CharacterSelect";

        public void OpenCharacterSelect()
        {
            playerMover?.SetInputEnabled(false);
            if (!string.IsNullOrWhiteSpace(characterSelectSceneName))
            {
                SceneManager.LoadScene(characterSelectSceneName, LoadSceneMode.Additive);
            }
        }
    }
}
