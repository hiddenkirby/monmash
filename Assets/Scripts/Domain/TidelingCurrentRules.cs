namespace Tidepool.Domain
{
    public static class TidelingCurrentRules
    {
        public const float AdvantageMultiplier = 1.5f;
        public const float NeutralMultiplier = 1f;
        public const float DisadvantageMultiplier = 0.75f;

        public static TidelingCurrent GetAdvantagedAgainst(TidelingCurrent current)
        {
            switch (current)
            {
                case TidelingCurrent.Current:
                    return TidelingCurrent.Coral;
                case TidelingCurrent.Coral:
                    return TidelingCurrent.Stone;
                case TidelingCurrent.Stone:
                    return TidelingCurrent.Glow;
                case TidelingCurrent.Glow:
                    return TidelingCurrent.Tide;
                case TidelingCurrent.Tide:
                    return TidelingCurrent.Current;
                default:
                    return current;
            }
        }

        public static float GetEffectivenessMultiplier(TidelingCurrent attacker, TidelingCurrent defender)
        {
            if (GetAdvantagedAgainst(attacker) == defender)
            {
                return AdvantageMultiplier;
            }

            if (GetAdvantagedAgainst(defender) == attacker)
            {
                return DisadvantageMultiplier;
            }

            return NeutralMultiplier;
        }

        public static string GetDisplayName(TidelingCurrent current)
        {
            switch (current)
            {
                case TidelingCurrent.Current:
                    return "Current";
                case TidelingCurrent.Coral:
                    return "Coral";
                case TidelingCurrent.Stone:
                    return "Stone";
                case TidelingCurrent.Glow:
                    return "Glow";
                case TidelingCurrent.Tide:
                    return "Tide";
                default:
                    return current.ToString();
            }
        }
    }
}
