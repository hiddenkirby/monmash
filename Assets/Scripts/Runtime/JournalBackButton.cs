using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tidepool.Runtime
{
    public class JournalBackButton : MonoBehaviour
    {
        public void BackToOverworld()
        {
            if (SceneManager.sceneCount > 1)
            {
                SceneManager.UnloadSceneAsync(gameObject.scene);
            }
            else
            {
                SceneManager.LoadScene("Overworld");
            }
        }
    }
}
