using Tidepool.Domain;

namespace Tidepool.Runtime
{
    public static class EncounterContext
    {
        public static TidelingSpecies CurrentSpecies { get; set; }
        public static ZoneId CurrentZone { get; set; }
    }
}

