using System;
using System.Globalization;
using Tidepool.Domain;
using Tidepool.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Tidepool.UI
{
    public class JournalController : MonoBehaviour
    {
        private const int V01JournalSlotCount = 13;

        [SerializeField] private SpeciesDatabase speciesDatabase;
        [SerializeField] private JournalSlotView slotPrefab;
        [SerializeField] private Transform gridRoot;
        [SerializeField] private Image detailImage;
        [SerializeField] private Text detailNameText;
        [SerializeField] private Text detailCurrentText;
        [SerializeField] private Text detailHabitatText;
        [SerializeField] private Text detailCaughtText;
        [SerializeField] private Text detailFieldNoteText;
        [SerializeField] private Text detailTimesSeenText;
        [SerializeField] private InputField nicknameInput;
        [SerializeField] private Text progressText;

        private TidelingSpecies selectedSpecies;

        private void OnEnable()
        {
            PopulateGrid();
        }

        public void PopulateGrid()
        {
            if (gridRoot == null || slotPrefab == null)
            {
                SetProgressText(0, V01JournalSlotCount);
                return;
            }

            for (int i = gridRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(gridRoot.GetChild(i).gameObject);
            }

            int found = 0;
            int speciesCount = speciesDatabase == null ? 0 : speciesDatabase.All.Count;
            int slotCount = Mathf.Max(V01JournalSlotCount, speciesCount);
            for (int i = 0; i < slotCount; i++)
            {
                TidelingSpecies species = i < speciesCount ? speciesDatabase.All[i] : null;
                CaughtTideling caught = species == null ? null : GameSaveService.Instance?.FindCaught(species.Id);
                bool isCaught = caught != null;
                if (isCaught)
                {
                    found += 1;
                }

                JournalSlotView slot = Instantiate(slotPrefab, gridRoot);
                slot.Bind(species, isCaught, () => SelectSpecies(species));
            }

            SetProgressText(found, slotCount);
        }

        public void SelectSpecies(TidelingSpecies species)
        {
            if (species == null)
            {
                return;
            }

            selectedSpecies = species;
            CaughtTideling caught = GameSaveService.Instance?.FindCaught(species.Id);
            bool isCaught = caught != null;

            if (detailImage != null)
            {
                detailImage.sprite = species.Sprite;
                detailImage.color = isCaught ? Color.white : Color.black;
                detailImage.enabled = species.Sprite != null;
                detailImage.preserveAspect = true;
            }

            SetText(detailNameText, isCaught ? FormatCaughtName(species, caught) : "?");
            SetText(detailCurrentText, isCaught ? species.Current.ToString() : "Unknown");
            SetText(detailHabitatText, isCaught ? FormatHabitats(species.HabitatZones) : "Unknown");
            SetText(detailCaughtText, isCaught ? FormatCatchDetails(caught) : "Not found yet");
            SetText(detailFieldNoteText, isCaught ? species.FieldNote : "Keep looking in the seagrass.");
            SetText(detailTimesSeenText, isCaught ? $"Seen {caught.timesSeen}" : "Not found yet");

            if (nicknameInput != null)
            {
                nicknameInput.text = isCaught ? GetSavedName(species, caught) : string.Empty;
                nicknameInput.interactable = isCaught;
            }
        }

        public void SaveNickname()
        {
            if (selectedSpecies == null)
            {
                return;
            }

            GameSaveService.Instance?.RenameCaught(selectedSpecies.Id, nicknameInput == null ? string.Empty : nicknameInput.text);
            SelectSpecies(selectedSpecies);
            PopulateGrid();
        }

        private static string FormatCaughtName(TidelingSpecies species, CaughtTideling caught)
        {
            string savedName = GetSavedName(species, caught);
            if (string.IsNullOrWhiteSpace(species.DisplayName) || savedName == species.DisplayName)
            {
                return savedName;
            }

            return $"{savedName} ({species.DisplayName})";
        }

        private static string GetSavedName(TidelingSpecies species, CaughtTideling caught)
        {
            if (!string.IsNullOrWhiteSpace(caught.nickname))
            {
                return caught.nickname.Trim();
            }

            return string.IsNullOrWhiteSpace(species.DisplayName) ? "Tideling" : species.DisplayName;
        }

        private static string FormatCatchDetails(CaughtTideling caught)
        {
            string location = FormatZone(caught.caughtInZone);
            if (DateTime.TryParse(
                    caught.caughtAtUtc,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out DateTime caughtAt))
            {
                return $"Caught {caughtAt.ToLocalTime():MMM d, yyyy} in {location}";
            }

            return $"Caught in {location}";
        }

        private static string FormatHabitats(ZoneId[] habitats)
        {
            if (habitats == null || habitats.Length == 0)
            {
                return "Unknown";
            }

            string text = FormatZone(habitats[0]);
            for (int i = 1; i < habitats.Length; i++)
            {
                text += ", " + FormatZone(habitats[i]);
            }

            return text;
        }

        private static string FormatZone(ZoneId zone)
        {
            switch (zone)
            {
                case ZoneId.TidepoolShallows:
                    return "Tidepool Shallows";
                case ZoneId.SeagrassMeadow:
                    return "Seagrass Meadow";
                case ZoneId.KelpCurtain:
                    return "Kelp Curtain";
                case ZoneId.RockyShelf:
                    return "Rocky Shelf";
                default:
                    return zone.ToString();
            }
        }

        private void SetProgressText(int found, int total)
        {
            SetText(progressText, $"{found} of {total} found");
        }

        private static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}
