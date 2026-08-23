using Tidepool.Domain;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tidepool.Runtime
{
    public class ContestTrigger : MonoBehaviour
    {
        [SerializeField] private SpeciesDatabase speciesDatabase;
        [SerializeField] private string playerSpeciesId = "blip";
        [SerializeField] private string visitingSpeciesId = "wobbet";
        [SerializeField] private string partySelectSceneName = "PartySelect";
        [SerializeField] private PlayerGridMover playerMover;

        private bool contestActive;

        private void OnEnable()
        {
            ContestEvents.ContestFinished += HandleContestFinished;
        }

        private void OnDisable()
        {
            ContestEvents.ContestFinished -= HandleContestFinished;
        }

        public void StartContest()
        {
            if (contestActive || speciesDatabase == null)
            {
                return;
            }

            TidelingSpecies playerSpecies = speciesDatabase.FindById(playerSpeciesId);
            TidelingSpecies visitingSpecies = speciesDatabase.FindById(visitingSpeciesId);
            if (playerSpecies == null || visitingSpecies == null)
            {
                Debug.LogWarning("ContestTrigger could not find species for contest.");
                return;
            }

            contestActive = true;
            playerMover?.SetInputEnabled(false);
            SceneManager.LoadScene(partySelectSceneName, LoadSceneMode.Additive);
        }

        private void HandleContestFinished()
        {
            contestActive = false;
            ContestContext.Clear();
            playerMover?.SetInputEnabled(true);
        }
    }
}
