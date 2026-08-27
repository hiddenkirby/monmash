using Tidepool.Domain;

namespace Tidepool.Runtime
{
    public static class EncounterContext
    {
        public static TidelingSpecies CurrentSpecies { get; set; }
        public static ZoneId CurrentZone { get; set; }
        public static bool IsOldBarnabyEncounter { get; set; }
        public static string EncounterIntroText { get; set; }
        public static string CatchCelebrationText { get; set; }

        public static void Clear()
        {
            CurrentSpecies = null;
            IsOldBarnabyEncounter = false;
            EncounterIntroText = null;
            CatchCelebrationText = null;
        }
    }
}
