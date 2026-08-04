using Tidepool.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Tidepool.UI
{
    public class SettingsController : MonoBehaviour
    {
        [SerializeField] private Toggle muteToggle;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Text volumeValueText;

        private void OnEnable()
        {
            TidepoolSettingsService.ApplyGlobalAudio();
            BindControls();
            RefreshLabels();
        }

        private void OnDisable()
        {
            if (muteToggle != null)
            {
                muteToggle.onValueChanged.RemoveListener(SetMuted);
            }

            if (volumeSlider != null)
            {
                volumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
            }
        }

        public void SetMuted(bool muted)
        {
            TidepoolSettingsService.SetMuted(muted);
            RefreshLabels();
        }

        public void SetMasterVolume(float volume)
        {
            TidepoolSettingsService.SetMasterVolume(volume);
            RefreshLabels();
        }

        private void BindControls()
        {
            if (muteToggle != null)
            {
                muteToggle.onValueChanged.RemoveListener(SetMuted);
                muteToggle.isOn = TidepoolSettingsService.Muted;
                muteToggle.onValueChanged.AddListener(SetMuted);
            }

            if (volumeSlider != null)
            {
                volumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
                volumeSlider.minValue = 0f;
                volumeSlider.maxValue = 1f;
                volumeSlider.wholeNumbers = false;
                volumeSlider.value = TidepoolSettingsService.MasterVolume;
                volumeSlider.onValueChanged.AddListener(SetMasterVolume);
            }
        }

        private void RefreshLabels()
        {
            if (volumeValueText != null)
            {
                int percent = Mathf.RoundToInt(TidepoolSettingsService.MasterVolume * 100f);
                volumeValueText.text = TidepoolSettingsService.Muted ? "Muted" : $"{percent}%";
            }
        }
    }
}
