using Tidepool.Domain;

namespace Tidepool.Runtime
{
    public static class ContestContext
    {
        public static TidelingSpecies PlayerSpecies { get; set; }
        public static TidelingSpecies VisitingSpecies { get; set; }

        public static void Clear()
        {
            PlayerSpecies = null;
            VisitingSpecies = null;
        }
    }
}
