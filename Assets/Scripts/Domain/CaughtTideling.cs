using System;
using System.Collections.Generic;

namespace Tidepool.Domain
{
    [Serializable]
    public class CaughtTideling
    {
        public const int NicknameCharacterLimit = 12;
        public const int MinLevel = 1;
        public const int MaxLevel = 20;

        public string speciesId;
        public string nickname;
        public string caughtAtUtc;
        public ZoneId caughtInZone;
        public int timesSeen;
        public int level = MinLevel;
        public int levelProgress;
        public string activeGrowthFormId = TidelingGrowthForms.OriginalFormId;
        public List<string> rememberedGrowthFormIds = new List<string>();
    }
}
