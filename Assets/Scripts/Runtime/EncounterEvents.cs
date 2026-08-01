using System;

namespace Tidepool.Runtime
{
    public static class EncounterEvents
    {
        public static event Action<bool> EncounterFinished;

        public static void RaiseEncounterFinished(bool caught)
        {
            EncounterFinished?.Invoke(caught);
        }
    }
}

