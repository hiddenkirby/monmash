using System;

namespace Tidepool.Runtime
{
    public static class ContestEvents
    {
        public static event Action ContestFinished;

        public static void RaiseContestFinished()
        {
            ContestFinished?.Invoke();
        }
    }
}
