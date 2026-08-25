using System;
using System.Collections;
using Tidepool.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Tidepool.UI
{
    public class JournalSlotView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image creatureImage;
        [SerializeField] private Text nameText;
        [SerializeField, Min(1f)] private float tapPulseScale = 1.08f;
        [SerializeField, Min(0.01f)] private float tapPulseDurationSeconds = 0.16f;

        private Coroutine tapPulseRoutine;
        private Vector3 restingScale;

        private void Awake()
        {
            restingScale = transform.localScale;
        }

        private void OnDisable()
        {
            if (tapPulseRoutine != null)
            {
                StopCoroutine(tapPulseRoutine);
                tapPulseRoutine = null;
            }

            transform.localScale = GetRestingScale();
        }

        public void Bind(TidelingSpecies species, bool isCaught, Action onClick)
        {
            bool hasSpecies = species != null;
            Sprite sprite = hasSpecies ? species.Sprite : null;

            creatureImage.sprite = sprite;
            creatureImage.enabled = sprite != null;
            creatureImage.color = isCaught ? Color.white : Color.black;
            creatureImage.preserveAspect = true;
            nameText.text = isCaught && hasSpecies ? species.DisplayName : "?";
            button.interactable = hasSpecies;

            button.onClick.RemoveAllListeners();
            if (hasSpecies)
            {
                button.onClick.AddListener(() =>
                {
                    PlayTapPulse();
                    onClick?.Invoke();
                });
            }
        }

        private void PlayTapPulse()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (tapPulseRoutine != null)
            {
                StopCoroutine(tapPulseRoutine);
            }

            tapPulseRoutine = StartCoroutine(PulseTappedSlot());
        }

        private IEnumerator PulseTappedSlot()
        {
            Vector3 startScale = GetRestingScale();
            Vector3 peakScale = startScale * tapPulseScale;
            float halfDuration = tapPulseDurationSeconds * 0.5f;

            yield return AnimateScale(startScale, peakScale, halfDuration);
            yield return AnimateScale(peakScale, startScale, halfDuration);

            transform.localScale = startScale;
            tapPulseRoutine = null;
        }

        private IEnumerator AnimateScale(Vector3 from, Vector3 to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                transform.localScale = Vector3.LerpUnclamped(from, to, t);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            transform.localScale = to;
        }

        private Vector3 GetRestingScale()
        {
            return restingScale == Vector3.zero ? Vector3.one : restingScale;
        }
    }
}
