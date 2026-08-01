using Tidepool.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Tidepool.UI
{
    public class FirstRunGuidanceController : MonoBehaviour
    {
        private const string GuidanceText = "Tap to walk. Look in the seagrass.";

        [SerializeField] private GameObject guidanceRoot;
        [SerializeField] private Text guidanceText;
        [SerializeField] private Button dismissButton;

        private void Start()
        {
            if (guidanceText != null)
            {
                guidanceText.text = GuidanceText;
            }

            if (dismissButton != null)
            {
                dismissButton.onClick.AddListener(Dismiss);
            }

            bool shouldShow = GameSaveService.Instance == null || !GameSaveService.Instance.HasAnyProgress();
            SetVisible(shouldShow);
        }

        public void Dismiss()
        {
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            GameObject target = guidanceRoot == null ? gameObject : guidanceRoot;
            target.SetActive(visible);
        }
    }
}
