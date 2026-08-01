using System.Collections.Generic;
using Tidepool.Domain;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Tidepool.Runtime
{
    public class EncounterDirector : MonoBehaviour
    {
        [SerializeField] private PlayerGridMover player;
        [SerializeField] private Tilemap seagrassTilemap;
        [SerializeField] private SpeciesDatabase speciesDatabase;
        [SerializeField] private ZoneId currentZone = ZoneId.SeagrassMeadow;
        [SerializeField] private string catchSceneName = "CatchEncounter";
        [SerializeField, Range(0f, 1f)] private float encounterChance = 0.12f;
        [SerializeField] private int graceStepsAfterEncounter = 3;
        [SerializeField] private int pitySteps = 25;

        private int remainingGraceSteps;
        private int drySeagrassSteps;

        private void OnEnable()
        {
            if (player != null)
            {
                player.StepCompleted += HandleStepCompleted;
            }

            EncounterEvents.EncounterFinished += HandleEncounterFinished;
        }

        private void OnDisable()
        {
            if (player != null)
            {
                player.StepCompleted -= HandleStepCompleted;
            }

            EncounterEvents.EncounterFinished -= HandleEncounterFinished;
        }

        private void HandleStepCompleted(Vector3Int cell)
        {
            if (seagrassTilemap == null || !seagrassTilemap.HasTile(cell))
            {
                return;
            }

            if (remainingGraceSteps > 0)
            {
                remainingGraceSteps -= 1;
                return;
            }

            drySeagrassSteps += 1;
            bool shouldForceEncounter = drySeagrassSteps >= pitySteps;
            bool rolledEncounter = Random.value < encounterChance;

            if (shouldForceEncounter || rolledEncounter)
            {
                StartEncounter();
            }
        }

        private void StartEncounter()
        {
            TidelingSpecies species = PickSpecies();
            if (species == null)
            {
                return;
            }

            drySeagrassSteps = 0;
            EncounterContext.CurrentSpecies = species;
            EncounterContext.CurrentZone = currentZone;
            GameSaveService.Instance?.MarkSeen(species.Id);
            player.SetInputEnabled(false);
            SceneManager.LoadScene(catchSceneName, LoadSceneMode.Additive);
        }

        private TidelingSpecies PickSpecies()
        {
            TidelingRarity rarity = RollRarity();
            List<TidelingSpecies> matches = speciesDatabase.FindByZoneAndRarity(currentZone, rarity);

            if (matches.Count == 0 && rarity == TidelingRarity.Rare)
            {
                matches = speciesDatabase.FindByZoneAndRarity(currentZone, TidelingRarity.Uncommon);
            }

            if (matches.Count == 0)
            {
                matches = speciesDatabase.FindByZoneAndRarity(currentZone, TidelingRarity.Common);
            }

            return matches.Count == 0 ? null : matches[Random.Range(0, matches.Count)];
        }

        private static TidelingRarity RollRarity()
        {
            float roll = Random.value;
            if (roll < 0.60f)
            {
                return TidelingRarity.Common;
            }

            return roll < 0.92f ? TidelingRarity.Uncommon : TidelingRarity.Rare;
        }

        private void HandleEncounterFinished(bool caught)
        {
            remainingGraceSteps = graceStepsAfterEncounter;
            player.SetInputEnabled(true);
        }
    }
}

