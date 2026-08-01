using System;

namespace Tidepool.Domain
{
    [Serializable]
    public class CaughtTideling
    {
        public const int NicknameCharacterLimit = 12;

        public string speciesId;
        public string nickname;
        public string caughtAtUtc;
        public ZoneId caughtInZone;
        public int timesSeen;
    }
}
