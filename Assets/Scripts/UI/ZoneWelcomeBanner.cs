using System.Collections;
using Tidepool.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Tidepool.UI
{
    public class ZoneWelcomeBanner : MonoBehaviour
    {
        [SerializeField] private GameObject bannerRoot;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text zoneNameText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private float visibleSeconds = 2f;
        [SerializeField] private float fadeSeconds = 0.3f;

        private Coroutine activeRoutine;

        private void Awake()
        {
            SetAlpha(0f);
            SetRootActive(false);
        }

        public void ShowTidepoolShallows()
        {
            Show(ZoneId.TidepoolShallows);
        }

        public void ShowSeagrassMeadow()
        {
            Show(ZoneId.SeagrassMeadow);
        }

        public void ShowKelpCurtain()
        {
            Show(ZoneId.KelpCurtain);
        }

        public void ShowRockyShelf()
        {
            Show(ZoneId.RockyShelf);
        }

        public void ShowKelpGateLocked()
        {
            ShowMessage(
                "The kelp is woven tight here.",
                "Maybe the meadow can teach us more first.");
        }

        public void ShowRockyGateLocked()
        {
            ShowMessage(
                "The rocks are still slick.",
                "Let's learn the kelp path first.");
        }

        public void Show(ZoneId zone)
        {
            ApplyCopy(zone);
            ShowCurrentCopy();
        }

        public void ShowMessage(string title, string subtitle)
        {
            if (zoneNameText != null)
            {
                zoneNameText.text = title;
            }

            if (subtitleText != null)
            {
                subtitleText.text = subtitle;
            }

            ShowCurrentCopy();
        }

        private void ShowCurrentCopy()
        {
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
            }

            activeRoutine = StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            SetRootActive(true);
            SetAlpha(0f);
            yield return FadeTo(1f);
            yield return new WaitForSeconds(visibleSeconds);
            yield return FadeTo(0f);
            SetRootActive(false);
            activeRoutine = null;
        }

        private IEnumerator FadeTo(float targetAlpha)
        {
            if (canvasGroup == null || fadeSeconds <= 0f)
            {
                SetAlpha(targetAlpha);
                yield break;
            }

            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < fadeSeconds)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / fadeSeconds);
                SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, progress));
                yield return null;
            }

            SetAlpha(targetAlpha);
        }

        private void ApplyCopy(ZoneId zone)
        {
            GetZoneCopy(zone, out string zoneName, out string subtitle);

            if (zoneNameText != null)
            {
                zoneNameText.text = zoneName;
            }

            if (subtitleText != null)
            {
                subtitleText.text = subtitle;
            }
        }

        private static void GetZoneCopy(ZoneId zone, out string zoneName, out string subtitle)
        {
            switch (zone)
            {
                case ZoneId.SeagrassMeadow:
                    zoneName = "Seagrass Meadow";
                    subtitle = "The meadow sways in the current";
                    return;
                case ZoneId.KelpCurtain:
                    zoneName = "Kelp Curtain";
                    subtitle = "Tall and green and full of shadows";
                    return;
                case ZoneId.RockyShelf:
                    zoneName = "Rocky Shelf";
                    subtitle = "Barnacles and tide pools";
                    return;
                default:
                    zoneName = "Tidepool Shallows";
                    subtitle = "Where the water is warm and clear";
                    return;
            }
        }

        private void SetRootActive(bool active)
        {
            if (canvasGroup != null)
            {
                return;
            }

            GameObject target = bannerRoot == null ? gameObject : bannerRoot;
            target.SetActive(active);
        }

        private void SetAlpha(float alpha)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = alpha;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
