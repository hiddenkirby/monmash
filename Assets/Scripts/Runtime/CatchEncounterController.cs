using System.Collections;
using Tidepool.Domain;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tidepool.Runtime
{
    public class CatchEncounterController : MonoBehaviour
    {
        [SerializeField] private Image creatureImage;
        [SerializeField] private Text creatureNameText;
        [SerializeField] private RectTransform calmBarTrack;
        [SerializeField] private RectTransform steadyZone;
        [SerializeField] private RectTransform marker;
        [SerializeField] private Image[] jarPips = new Image[3];
        [SerializeField] private Text resultText;
        [SerializeField] private Button letGoButton;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip catchChimeClip;
        [SerializeField] private AudioClip escapeNoteClip;
        [SerializeField] private AudioClip uiTapClip;
        [SerializeField, Min(0f)] private float escapeResultSeconds = 0.8f;

        private TidelingSpecies species;
        private float markerPosition;
        private float markerDirection = 1f;
        private float zoneWidth = 0.35f;
        private float markerSpeed = 0.65f;
        private int hits;
        private int misses;
        private bool finished;

        private void Start()
        {
            species = EncounterContext.CurrentSpecies;
            if (species == null)
            {
                Finish(false);
                return;
            }

            zoneWidth = species.CatchZoneWidth;
            markerSpeed = species.CatchMarkerSpeed;
            creatureNameText.text = species.DisplayName;
            creatureImage.sprite = species.Sprite;
            creatureImage.enabled = species.Sprite != null;
            resultText.text = string.Empty;
            audioSource ??= GetComponent<AudioSource>();
            TidepoolSettingsService.ApplyGlobalAudio();
            letGoButton.onClick.AddListener(LetGo);
            RefreshPips();
            LayoutSteadyZone();
        }

        private void Update()
        {
            if (finished)
            {
                return;
            }

            AdvanceMarker();

            if (WasPrimaryTapPressed())
            {
                TryJarTap();
            }
        }

        public void TryJarTap()
        {
            if (finished)
            {
                return;
            }

            float zoneMin = 0.5f - zoneWidth * 0.5f;
            float zoneMax = 0.5f + zoneWidth * 0.5f;
            bool hit = markerPosition >= zoneMin && markerPosition <= zoneMax;

            PlayClip(uiTapClip);

            if (hit)
            {
                hits += 1;
                resultText.text = "Steady!";
                RefreshPips();

                if (hits >= 3)
                {
                    PlayClip(catchChimeClip);
                    GameSaveService.Instance?.RecordCatch(species, EncounterContext.CurrentZone);
                    Finish(true);
                }
            }
            else
            {
                misses += 1;
                markerSpeed *= 1.15f;
                zoneWidth = Mathf.Max(0.16f, zoneWidth * 0.9f);
                resultText.text = misses >= 3 ? "It slipped away!" : "Almost.";
                LayoutSteadyZone();

                if (misses >= 3)
                {
                    PlayClip(escapeNoteClip);
                    Finish(false, escapeResultSeconds);
                }
            }
        }

        public void LetGo()
        {
            if (!finished)
            {
                PlayClip(uiTapClip);
                resultText.text = "Back to the water.";
                Finish(false);
            }
        }

        private void AdvanceMarker()
        {
            markerPosition += markerDirection * markerSpeed * Time.unscaledDeltaTime;

            if (markerPosition >= 1f)
            {
                markerPosition = 1f;
                markerDirection = -1f;
            }
            else if (markerPosition <= 0f)
            {
                markerPosition = 0f;
                markerDirection = 1f;
            }

            float trackWidth = calmBarTrack.rect.width;
            marker.anchoredPosition = new Vector2((markerPosition - 0.5f) * trackWidth, marker.anchoredPosition.y);
        }

        private void LayoutSteadyZone()
        {
            float trackWidth = calmBarTrack.rect.width;
            steadyZone.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, trackWidth * zoneWidth);
            steadyZone.anchoredPosition = Vector2.zero;
        }

        private void RefreshPips()
        {
            for (int i = 0; i < jarPips.Length; i++)
            {
                if (jarPips[i] != null)
                {
                    jarPips[i].enabled = i < hits;
                }
            }
        }

        private void PlayClip(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            TidepoolSettingsService.ApplyGlobalAudio();
            if (audioSource != null)
            {
                audioSource.PlayOneShot(clip);
                return;
            }

            AudioSource.PlayClipAtPoint(clip, Vector3.zero);
        }

        private void Finish(bool caught)
        {
            Finish(caught, 0f);
        }

        private void Finish(bool caught, float delaySeconds)
        {
            finished = true;
            StartCoroutine(FinishAfterDelay(caught, delaySeconds));
        }

        private IEnumerator FinishAfterDelay(bool caught, float delaySeconds)
        {
            if (delaySeconds > 0f)
            {
                yield return new WaitForSecondsRealtime(delaySeconds);
            }

            EncounterEvents.RaiseEncounterFinished(caught);
            SceneManager.UnloadSceneAsync(gameObject.scene);
        }

        private static bool WasPrimaryTapPressed()
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                return true;
            }

            return Input.GetMouseButtonDown(0);
        }
    }
}
