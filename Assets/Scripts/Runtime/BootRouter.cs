using UnityEngine;
using UnityEngine.EventSystems;
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

    internal static class EventSystemDeduplicator
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            NormalizeLoadedEventSystems();
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            NormalizeLoadedEventSystems();
        }

        private static void NormalizeLoadedEventSystems()
        {
            EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            EventSystem primary = FindPrimaryEventSystem(eventSystems);
            if (primary == null)
            {
                return;
            }

            foreach (EventSystem eventSystem in eventSystems)
            {
                if (!IsLoadedSceneObject(eventSystem))
                {
                    continue;
                }

                bool shouldEnable = eventSystem == primary;
                eventSystem.enabled = shouldEnable;

                foreach (BaseInputModule inputModule in eventSystem.GetComponents<BaseInputModule>())
                {
                    inputModule.enabled = shouldEnable;
                }
            }
        }

        private static EventSystem FindPrimaryEventSystem(EventSystem[] eventSystems)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            foreach (EventSystem eventSystem in eventSystems)
            {
                if (IsLoadedSceneObject(eventSystem) && eventSystem.gameObject.scene == activeScene)
                {
                    return eventSystem;
                }
            }

            foreach (EventSystem eventSystem in eventSystems)
            {
                if (IsLoadedSceneObject(eventSystem))
                {
                    return eventSystem;
                }
            }

            return null;
        }

        private static bool IsLoadedSceneObject(EventSystem eventSystem)
        {
            Scene scene = eventSystem.gameObject.scene;
            return scene.IsValid() && scene.isLoaded;
        }
    }
}
