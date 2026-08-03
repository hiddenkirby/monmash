using System;

namespace Tidepool.Domain
{
    [Serializable]
    public class ContestParticipantState
    {
        public const int DefaultRestRounds = 1;

        public string speciesId;
        public int restRoundsRemaining;

        public bool IsTuckeredOut => restRoundsRemaining > 0;
        public bool CanChooseMove => !IsTuckeredOut;

        public static ContestParticipantState ForSpecies(TidelingSpecies species)
        {
            return new ContestParticipantState
            {
                speciesId = species == null ? string.Empty : species.Id
            };
        }

        public void MarkTuckeredOut()
        {
            MarkTuckeredOut(DefaultRestRounds);
        }

        public void MarkTuckeredOut(int rounds)
        {
            restRoundsRemaining = rounds < 1 ? DefaultRestRounds : rounds;
        }

        public bool AdvanceRest()
        {
            if (restRoundsRemaining <= 0)
            {
                restRoundsRemaining = 0;
                return false;
            }

            restRoundsRemaining -= 1;
            return restRoundsRemaining == 0;
        }

        public void ClearRest()
        {
            restRoundsRemaining = 0;
        }
    }
}
