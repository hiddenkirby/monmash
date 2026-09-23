using System;

namespace Tidepool.Runtime
{
    public enum ContestOutcome
    {
        PlayerWin,
        Tie,
        VisitorWin
    }

    public static class ContestEvents
    {
        public static event Action ContestFinished;
        public static event Action<ContestOutcome> ContestResolved;

        public static void RaiseContestResolved(ContestOutcome outcome)
        {
            ContestResolved?.Invoke(outcome);
        }

        public static void RaiseContestFinished()
        {
            ContestFinished?.Invoke();
        }
    }
}
