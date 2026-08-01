using System;

namespace Tidepool.Domain
{
    [Serializable]
    public class CaughtTideling
    {
        public string speciesId;
        public string nickname;
        public string caughtAtUtc;
        public ZoneId caughtInZone;
        public int timesSeen;
    }
}

