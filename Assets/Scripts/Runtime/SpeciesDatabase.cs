using System.Collections.Generic;
using Tidepool.Domain;
using UnityEngine;

namespace Tidepool.Runtime
{
    [CreateAssetMenu(menuName = "Tidepool/Species Database", fileName = "SpeciesDatabase")]
    public class SpeciesDatabase : ScriptableObject
    {
        [SerializeField] private List<TidelingSpecies> species = new List<TidelingSpecies>();

        public IReadOnlyList<TidelingSpecies> All => species;

        public TidelingSpecies FindById(string id)
        {
            for (int i = 0; i < species.Count; i++)
            {
                if (species[i] != null && species[i].Id == id)
                {
                    return species[i];
                }
            }

            return null;
        }

        public List<TidelingSpecies> FindByZoneAndRarity(ZoneId zone, TidelingRarity rarity)
        {
            List<TidelingSpecies> matches = new List<TidelingSpecies>();

            for (int i = 0; i < species.Count; i++)
            {
                TidelingSpecies candidate = species[i];
                if (candidate != null && candidate.Rarity == rarity && candidate.LivesIn(zone))
                {
                    matches.Add(candidate);
                }
            }

            return matches;
        }
    }
}

