using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tidepool.Runtime
{
    public class BootRouter : MonoBehaviour
    {
        [SerializeField] private string overworldSceneName = "Overworld";
        [SerializeField] private bool loadOverworldOnStart = true;

        private void Start()
        {
            if (loadOverworldOnStart)
            {
                ContinueToOverworld();
            }
        }

        public void ContinueToOverworld()
        {
            if (!string.IsNullOrWhiteSpace(overworldSceneName))
            {
                SceneManager.LoadScene(overworldSceneName);
            }
        }
    }
}
