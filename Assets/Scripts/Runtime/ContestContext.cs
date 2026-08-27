using System.Collections.Generic;
using Tidepool.Domain;

namespace Tidepool.Runtime
{
    public static class ContestContext
    {
        private static readonly List<TidelingSpecies> playerParty = new List<TidelingSpecies>();
        private static readonly List<TidelingSpecies> visitingParty = new List<TidelingSpecies>();

        public static TidelingSpecies PlayerSpecies { get; set; }
        public static TidelingSpecies VisitingSpecies { get; set; }
        public static IReadOnlyList<TidelingSpecies> PlayerParty => playerParty;
        public static IReadOnlyList<TidelingSpecies> VisitingParty => visitingParty;

        public static void SetPlayerParty(IEnumerable<TidelingSpecies> species)
        {
            SetParty(playerParty, species);
        }

        public static void SetVisitingParty(IEnumerable<TidelingSpecies> species)
        {
            SetParty(visitingParty, species);
        }

        public static void Clear()
        {
            PlayerSpecies = null;
            VisitingSpecies = null;
            playerParty.Clear();
            visitingParty.Clear();
        }

        private static void SetParty(List<TidelingSpecies> party, IEnumerable<TidelingSpecies> species)
        {
            party.Clear();
            if (species == null)
            {
                return;
            }

            foreach (TidelingSpecies entry in species)
            {
                if (entry == null || party.Contains(entry))
                {
                    continue;
                }

                party.Add(entry);
            }
        }
    }
}
