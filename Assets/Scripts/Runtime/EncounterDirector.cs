using System;
using System.Collections.Generic;
using Tidepool.Domain;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Tidepool.Runtime
{
    public class EncounterDirector : MonoBehaviour
    {
        private const string OldBarnabySpeciesId = "old-barnaby";

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
            player?.SetInputEnabled(false);
            SceneManager.LoadScene(catchSceneName, LoadSceneMode.Additive);
        }

        private TidelingSpecies PickSpecies()
        {
            if (speciesDatabase == null)
            {
                return null;
            }

            TidelingRarity rarity = RollRarity();
            List<TidelingSpecies> matches = FindNormalSpeciesByRarity(rarity);

            if (matches.Count == 0)
            {
                TidelingRarity[] fallbackRarities = GetFallbackRarities(rarity);
                for (int i = 0; i < fallbackRarities.Length && matches.Count == 0; i++)
                {
                    matches = FindNormalSpeciesByRarity(fallbackRarities[i]);
                }
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

        private List<TidelingSpecies> FindNormalSpeciesByRarity(TidelingRarity rarity)
        {
            List<TidelingSpecies> matches = new List<TidelingSpecies>();
            IReadOnlyList<TidelingSpecies> allSpecies = speciesDatabase.All;

            for (int i = 0; i < allSpecies.Count; i++)
            {
                TidelingSpecies candidate = allSpecies[i];
                if (IsNormalEncounterSpecies(candidate)
                    && candidate.Rarity == rarity
                    && candidate.LivesIn(currentZone))
                {
                    matches.Add(candidate);
                }
            }

            return matches;
        }

        private static TidelingRarity[] GetFallbackRarities(TidelingRarity rolledRarity)
        {
            switch (rolledRarity)
            {
                case TidelingRarity.Common:
                    return new[] { TidelingRarity.Uncommon, TidelingRarity.Rare };
                case TidelingRarity.Uncommon:
                    return new[] { TidelingRarity.Common, TidelingRarity.Rare };
                case TidelingRarity.Rare:
                    return new[] { TidelingRarity.Uncommon, TidelingRarity.Common };
                default:
                    return new[] { TidelingRarity.Common, TidelingRarity.Uncommon, TidelingRarity.Rare };
            }
        }

        private static bool IsNormalEncounterSpecies(TidelingSpecies species)
        {
            return species != null
                && species.Rarity != TidelingRarity.Secret
                && !string.Equals(species.Id, OldBarnabySpeciesId, StringComparison.OrdinalIgnoreCase);
        }

        private void HandleEncounterFinished(bool caught)
        {
            remainingGraceSteps = graceStepsAfterEncounter;
            player?.SetInputEnabled(true);
        }
    }
}
