using Tidepool.Domain;
using Tidepool.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Tidepool.UI
{
    public class GoalsPanelController : MonoBehaviour
    {
        private const int TotalSpeciesCount = 13;
        private const string OldBarnabySpeciesId = "old-barnaby";

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text[] checkTexts;
        [SerializeField] private Text[] goalTexts;
        [SerializeField] private Button closeButton;

        private void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseGoals);
            }

            SetVisible(false);
        }

        public void OpenGoals()
        {
            RefreshGoals();
            SetVisible(true);
        }

        public void CloseGoals()
        {
            SetVisible(false);
        }

        private void RefreshGoals()
        {
            GameSaveService saveService = GameSaveService.Instance;
            int meadowCaught = saveService == null ? 0 : saveService.CountCaughtSpeciesInZone(ZoneId.SeagrassMeadow);
            int kelpCaught = saveService == null ? 0 : saveService.CountCaughtSpeciesInZone(ZoneId.KelpCurtain);
            int totalCaught = saveService == null ? 0 : saveService.CountCaughtSpecies();
            bool lookedInKelp = HasLookedInKelp(saveService, kelpCaught);
            bool foundOldBarnaby = saveService != null && saveService.FindCaught(OldBarnabySpeciesId) != null;

            SetGoal(0, meadowCaught >= 3, "Find 3 creatures in the Seagrass Meadow.", "You found meadow friends.");
            SetGoal(1, meadowCaught >= 5, "Find 5 creatures in the Seagrass Meadow.", "The meadow knows you now.");
            SetGoal(2, lookedInKelp, "Look in the Kelp Curtain.", "You peeked through the kelp.");
            SetGoal(3, foundOldBarnaby, "Find Old Barnaby in the shallows.", "Old Barnaby chose to say hello.");
            SetGoal(4, totalCaught >= TotalSpeciesCount,
                $"Fill the journal. {totalCaught} of {TotalSpeciesCount} found.",
                "The journal is full.");
        }

        private static bool HasLookedInKelp(GameSaveService saveService, int kelpCaught)
        {
            if (saveService == null || saveService.Data == null)
            {
                return false;
            }

            return saveService.Data.currentZone == ZoneId.KelpCurtain
                || saveService.Data.currentZone == ZoneId.RockyShelf
                || kelpCaught > 0
                || saveService.IsZoneUnlocked(ZoneId.RockyShelf);
        }

        private void SetGoal(int index, bool complete, string activeText, string completionText)
        {
            if (checkTexts != null && index < checkTexts.Length && checkTexts[index] != null)
            {
                checkTexts[index].text = complete ? "✓" : "-";
                checkTexts[index].color = complete ? new Color(0.15f, 0.42f, 0.28f) : new Color(0.38f, 0.44f, 0.46f);
            }

            if (goalTexts == null || index >= goalTexts.Length || goalTexts[index] == null)
            {
                return;
            }

            goalTexts[index].text = complete ? completionText : activeText;
            goalTexts[index].color = complete ? new Color(0.12f, 0.28f, 0.22f) : new Color(0.08f, 0.18f, 0.22f);
        }

        private void SetVisible(bool visible)
        {
            GameObject target = panelRoot == null ? gameObject : panelRoot;
            target.SetActive(visible);
        }
    }
}
