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

            detailImage.sprite = species.Sprite;
            detailImage.color = isCaught ? Color.white : Color.black;
            detailImage.enabled = species.Sprite != null;
            detailNameText.text = isCaught ? caught.nickname : "?";
            detailCurrentText.text = isCaught ? species.Current.ToString() : "Unknown";
            detailHabitatText.text = isCaught ? FormatHabitats(species.HabitatZones) : "Unknown";
            detailFieldNoteText.text = isCaught ? species.FieldNote : "Keep looking in the seagrass.";
            detailTimesSeenText.text = isCaught ? $"Seen {caught.timesSeen}" : "Not found yet";
            nicknameInput.text = isCaught ? caught.nickname : string.Empty;
            nicknameInput.interactable = isCaught;
        }

        public void SaveNickname()
        {
            if (selectedSpecies == null)
            {
                return;
            }

            GameSaveService.Instance?.RenameCaught(selectedSpecies.Id, nicknameInput.text);
            SelectSpecies(selectedSpecies);
            PopulateGrid();
        }

        private static string FormatHabitats(ZoneId[] habitats)
        {
            if (habitats == null || habitats.Length == 0)
            {
                return "Unknown";
            }

            string text = habitats[0].ToString();
            for (int i = 1; i < habitats.Length; i++)
            {
                text += ", " + habitats[i];
            }

            return text;
        }

        private void SetProgressText(int found, int total)
        {
            if (progressText != null)
            {
                progressText.text = $"{found} of {total} found";
            }
        }
    }
}
