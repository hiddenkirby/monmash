using System;
using Tidepool.Domain;
using Tidepool.Runtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Tidepool.UI
{
    [Serializable]
    public class FieldStationDecorationBinding
    {
        [SerializeField] private string upgradeId;
        [SerializeField] private GameObject standardRoot;
        [SerializeField] private GameObject reducedMotionRoot;

        public string UpgradeId => upgradeId;
        public GameObject StandardRoot => standardRoot;
        public GameObject ReducedMotionRoot => reducedMotionRoot;
    }

    [Serializable]
    public class FieldStationVisitorBinding
    {
        [SerializeField] private string speciesId;
        [SerializeField] private GameObject visitorRoot;

        public string SpeciesId => speciesId;
        public GameObject VisitorRoot => visitorRoot;
    }

    public class FieldStationController : MonoBehaviour
    {
        [SerializeField] private FieldStationDefinition definition;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private FieldStationDecorationBinding[] decorations = Array.Empty<FieldStationDecorationBinding>();
        [SerializeField] private FieldStationVisitorBinding[] visitors = Array.Empty<FieldStationVisitorBinding>();
        [SerializeField] private UnityEvent openJournal = new UnityEvent();
        [SerializeField] private UnityEvent openAtlas = new UnityEvent();
        [SerializeField] private UnityEvent openGoals = new UnityEvent();
        [SerializeField] private UnityEvent openSettings = new UnityEvent();

        private bool isRefreshing;

        private void OnEnable()
        {
            Subscribe();
            RefreshStation();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void RefreshStation()
        {
            if (isRefreshing)
            {
                return;
            }

            isRefreshing = true;
            try
            {
                GameSaveService saveService = GameSaveService.Instance;
                FieldStationProgress.Reconcile(definition, saveService);
                SaveData data = saveService == null ? null : saveService.Data;
                bool reducedMotion = TidepoolSettingsService.ReducedMotion;

                FieldStationStage latest = FieldStationProgress.FindLatestEarnedStage(definition, data);
                SetText(titleText, definition == null ? "Field Station" : definition.Title);
                SetText(descriptionText, latest == null ? GetFallbackDescription() : latest.Description);

                FieldStationDecorationBinding[] decorationBindings = decorations ?? Array.Empty<FieldStationDecorationBinding>();
                for (int i = 0; i < decorationBindings.Length; i++)
                {
                    RefreshDecoration(decorationBindings[i], data, reducedMotion);
                }

                FieldStationVisitorBinding[] visitorBindings = visitors ?? Array.Empty<FieldStationVisitorBinding>();
                for (int i = 0; i < visitorBindings.Length; i++)
                {
                    RefreshVisitor(visitorBindings[i], data);
                }
            }
            finally
            {
                isRefreshing = false;
            }
        }

        public void OpenJournal()
        {
            openJournal?.Invoke();
        }

        public void OpenAtlas()
        {
            openAtlas?.Invoke();
        }

        public void OpenGoals()
        {
            openGoals?.Invoke();
        }

        public void OpenSettings()
        {
            openSettings?.Invoke();
        }

        private void Subscribe()
        {
            GameSaveService saveService = GameSaveService.Instance;
            if (saveService == null)
            {
                return;
            }

            saveService.ExpeditionStateChanged -= HandleProgressChanged;
            saveService.ExpeditionStateChanged += HandleProgressChanged;
            saveService.SpeciesCaught -= HandleSpeciesCaught;
            saveService.SpeciesCaught += HandleSpeciesCaught;
        }

        private void Unsubscribe()
        {
            GameSaveService saveService = GameSaveService.Instance;
            if (saveService == null)
            {
                return;
            }

            saveService.ExpeditionStateChanged -= HandleProgressChanged;
            saveService.SpeciesCaught -= HandleSpeciesCaught;
        }

        private void HandleProgressChanged()
        {
            RefreshStation();
        }

        private void HandleSpeciesCaught(TidelingSpecies species, ZoneId zone)
        {
            RefreshStation();
        }

        private void RefreshDecoration(FieldStationDecorationBinding binding, SaveData data, bool reducedMotion)
        {
            if (binding == null)
            {
                return;
            }

            bool earned = data != null
                && data.fieldStationUpgradeIds != null
                && data.fieldStationUpgradeIds.Contains(binding.UpgradeId);
            SetActive(binding.StandardRoot, earned && (!reducedMotion || binding.ReducedMotionRoot == null));
            SetActive(binding.ReducedMotionRoot, earned && reducedMotion);
        }

        private static void RefreshVisitor(FieldStationVisitorBinding binding, SaveData data)
        {
            if (binding == null || binding.VisitorRoot == null)
            {
                return;
            }

            bool visible = FieldStationProgress.HasCaughtSpecies(data, binding.SpeciesId);
            binding.VisitorRoot.SetActive(visible);

            Collider2D[] colliders2D = binding.VisitorRoot.GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < colliders2D.Length; i++)
            {
                colliders2D[i].enabled = false;
            }

            Collider[] colliders = binding.VisitorRoot.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
        }

        private string GetFallbackDescription()
        {
            return definition == null
                ? "A quiet place for everything the coast remembers."
                : definition.FallbackDescription;
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
