using System;
using System.Collections.Generic;
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
        [SerializeField] private Image detailCurrentIcon;
        [SerializeField] private Text detailHabitatText;
        [SerializeField] private Text detailCaughtText;
        [SerializeField] private Text detailLevelText;
        [SerializeField] private Text detailGrowthText;
        [SerializeField] private Text detailGrowthMemoryText;
        [SerializeField] private Dropdown growthFormDropdown;
        [SerializeField] private Button selectOriginalGrowthFormButton;
        [SerializeField] private Text detailMovesText;
        [SerializeField] private Text detailFieldNoteText;
        [SerializeField] private Text detailTimesSeenText;
        [SerializeField] private InputField nicknameInput;
        [SerializeField] private Text progressText;

        private TidelingSpecies selectedSpecies;

        private void OnEnable()
        {
            ConfigureNicknameInput();
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
            if (isCaught)
            {
                TidelingLevelProgression.Normalize(caught);
                TidelingGrowthForms.Normalize(caught);
            }

            if (detailImage != null)
            {
                detailImage.sprite = species.Sprite;
                detailImage.color = isCaught ? Color.white : Color.black;
                detailImage.enabled = species.Sprite != null;
                detailImage.preserveAspect = true;
            }

            SetText(detailNameText, isCaught ? FormatCaughtName(species, caught) : "?");
            ApplyCurrentDetail(species, isCaught);
            SetText(detailHabitatText, isCaught ? FormatHabitatsAndAvailability(species) : "Unknown");
            SetText(detailCaughtText, isCaught ? FormatCatchDetails(caught) : "Not found yet");
            SetText(detailLevelText, isCaught ? FormatLevelDetails(caught) : "Unknown");
            SetText(detailGrowthText, isCaught ? FormatGrowthDetails(caught) : "Keep looking to learn more.");
            SetText(detailGrowthMemoryText, isCaught ? FormatGrowthMemoryDetails(caught) : "Unknown");
            ConfigureGrowthFormControls(caught, isCaught);
            SetText(detailMovesText, isCaught ? FormatMoveDetails(species, caught.level) : "Unknown");
            SetText(detailFieldNoteText, isCaught ? species.FieldNote : "Keep looking in the seagrass.");
            SetText(detailTimesSeenText, isCaught ? $"Seen {caught.timesSeen}" : "Not found yet");

            if (nicknameInput != null)
            {
                ConfigureNicknameInput();
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

        public void SelectGrowthFormFromDropdown()
        {
            if (selectedSpecies == null || growthFormDropdown == null)
            {
                return;
            }

            CaughtTideling caught = GameSaveService.Instance?.FindCaught(selectedSpecies.Id);
            if (caught == null)
            {
                return;
            }

            string formId = GetGrowthFormIdAtIndex(caught, growthFormDropdown.value);
            GameSaveService.Instance?.SelectGrowthForm(selectedSpecies.Id, formId);
            SelectSpecies(selectedSpecies);
        }

        public void SelectOriginalGrowthForm()
        {
            if (selectedSpecies == null)
            {
                return;
            }

            GameSaveService.Instance?.SelectOriginalGrowthForm(selectedSpecies.Id);
            SelectSpecies(selectedSpecies);
        }

        private void ConfigureNicknameInput()
        {
            if (nicknameInput != null)
            {
                nicknameInput.characterLimit = CaughtTideling.NicknameCharacterLimit;
            }
        }

        private void ConfigureGrowthFormControls(CaughtTideling caught, bool isCaught)
        {
            if (growthFormDropdown != null)
            {
                growthFormDropdown.onValueChanged.RemoveAllListeners();
                growthFormDropdown.ClearOptions();

                if (isCaught && caught != null)
                {
                    TidelingGrowthForms.Normalize(caught);
                    growthFormDropdown.AddOptions(BuildGrowthFormOptions(caught));
                    growthFormDropdown.value = FindActiveGrowthFormIndex(caught);
                    growthFormDropdown.interactable = CountGrowthFormChoices(caught) > 1;
                    growthFormDropdown.onValueChanged.AddListener(_ => SelectGrowthFormFromDropdown());
                }
                else
                {
                    growthFormDropdown.AddOptions(new List<string> { "Unknown" });
                    growthFormDropdown.value = 0;
                    growthFormDropdown.interactable = false;
                }
            }

            if (selectOriginalGrowthFormButton != null)
            {
                selectOriginalGrowthFormButton.onClick.RemoveAllListeners();
                selectOriginalGrowthFormButton.onClick.AddListener(SelectOriginalGrowthForm);
                selectOriginalGrowthFormButton.interactable = isCaught
                    && caught != null
                    && !TidelingGrowthForms.IsOriginal(caught.activeGrowthFormId);
            }
        }

        private static List<string> BuildGrowthFormOptions(CaughtTideling caught)
        {
            List<string> options = new List<string> { "Original form" };
            if (caught?.rememberedGrowthFormIds == null)
            {
                return options;
            }

            for (int i = 0; i < caught.rememberedGrowthFormIds.Count; i++)
            {
                options.Add(FormatGrowthFormChoice(caught.rememberedGrowthFormIds[i]));
            }

            return options;
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

        private void ApplyCurrentDetail(TidelingSpecies species, bool isCaught)
        {
            if (!isCaught)
            {
                SetText(detailCurrentText, "Unknown");
                if (detailCurrentIcon != null)
                {
                    detailCurrentIcon.enabled = false;
                }

                return;
            }

            SetText(detailCurrentText, FormatCurrentDetails(species.Current));
            if (detailCurrentIcon != null)
            {
                detailCurrentIcon.enabled = true;
                detailCurrentIcon.color = TidelingCurrentRules.GetDisplayColor(species.Current);
                detailCurrentIcon.preserveAspect = true;
            }
        }

        private static string FormatCurrentDetails(TidelingCurrent current)
        {
            return $"{TidelingCurrentRules.GetIconName(current)} - {TidelingCurrentRules.GetDisplayName(current)}";
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

        private static string FormatLevelDetails(CaughtTideling caught)
        {
            return $"Level {caught.level}";
        }

        private static string FormatGrowthDetails(CaughtTideling caught)
        {
            if (caught.level >= CaughtTideling.MaxLevel)
            {
                return "Fully grown";
            }

            int remainingProgress = TidelingLevelProgression.ProgressPerLevel - caught.levelProgress;
            if (remainingProgress <= 1)
            {
                return "Almost ready to grow";
            }

            return $"{remainingProgress} friendly moments until next growth";
        }

        private static string FormatGrowthMemoryDetails(CaughtTideling caught)
        {
            TidelingGrowthForms.Normalize(caught);
            int rememberedCount = caught.rememberedGrowthFormIds.Count;
            if (TidelingGrowthForms.IsOriginal(caught.activeGrowthFormId))
            {
                if (rememberedCount == 0)
                {
                    return "Original form";
                }

                return rememberedCount == 1
                    ? "Original form selected. 1 memory available."
                    : $"Original form selected. {rememberedCount} memories available.";
            }

            return $"Remembering {FormatGrowthFormId(caught.activeGrowthFormId)}. Original form is still here.";
        }

        private static string FormatGrowthFormChoice(string formId)
        {
            return TidelingGrowthForms.IsOriginal(formId)
                ? "Original form"
                : $"{FormatGrowthFormId(formId)} memory";
        }

        private static string FormatGrowthFormId(string formId)
        {
            string normalized = string.IsNullOrWhiteSpace(formId) ? "original" : formId.Trim();
            normalized = normalized.Replace('_', ' ').Replace('-', ' ');
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(normalized);
        }

        private static int CountGrowthFormChoices(CaughtTideling caught)
        {
            return 1 + (caught?.rememberedGrowthFormIds == null ? 0 : caught.rememberedGrowthFormIds.Count);
        }

        private static int FindActiveGrowthFormIndex(CaughtTideling caught)
        {
            if (caught == null || TidelingGrowthForms.IsOriginal(caught.activeGrowthFormId))
            {
                return 0;
            }

            for (int i = 0; i < caught.rememberedGrowthFormIds.Count; i++)
            {
                if (string.Equals(caught.rememberedGrowthFormIds[i], caught.activeGrowthFormId, StringComparison.OrdinalIgnoreCase))
                {
                    return i + 1;
                }
            }

            return 0;
        }

        private static string GetGrowthFormIdAtIndex(CaughtTideling caught, int index)
        {
            if (caught == null || index <= 0 || caught.rememberedGrowthFormIds == null)
            {
                return TidelingGrowthForms.OriginalFormId;
            }

            int rememberedIndex = index - 1;
            return rememberedIndex < caught.rememberedGrowthFormIds.Count
                ? caught.rememberedGrowthFormIds[rememberedIndex]
                : TidelingGrowthForms.OriginalFormId;
        }

        private static string FormatMoveDetails(TidelingSpecies species, int level)
        {
            if (species == null)
            {
                return "Moves are still being discovered.";
            }

            string details = string.Empty;
            for (int i = 0; i < 2; i++)
            {
                ContestMove move = species.GetContestMove(i);
                if (move == null)
                {
                    continue;
                }

                string moveText = species.IsContestMoveUnlocked(i, level)
                    ? move.DisplayName
                    : $"{move.DisplayName} at level {species.GetContestMoveUnlockLevel(i)}";
                details = string.IsNullOrEmpty(details) ? moveText : details + ", " + moveText;
            }

            return string.IsNullOrEmpty(details) ? "Moves are still being discovered." : details;
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

        private static string FormatHabitatsAndAvailability(TidelingSpecies species)
        {
            string habitats = FormatHabitats(species.HabitatZones);
            if (string.IsNullOrWhiteSpace(species.AvailabilityHint))
            {
                return habitats;
            }

            return $"{habitats} - {species.AvailabilityHint}";
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
