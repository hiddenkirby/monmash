using UnityEngine;

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

        public static string GetIconName(TidelingCurrent current)
        {
            switch (current)
            {
                case TidelingCurrent.Current:
                    return "Wave";
                case TidelingCurrent.Coral:
                    return "Coral Branch";
                case TidelingCurrent.Stone:
                    return "Pebble";
                case TidelingCurrent.Glow:
                    return "Lantern";
                case TidelingCurrent.Tide:
                    return "Shell";
                default:
                    return "Current Mark";
            }
        }

        public static Color GetDisplayColor(TidelingCurrent current)
        {
            switch (current)
            {
                case TidelingCurrent.Current:
                    return new Color32(45, 132, 184, 255);
                case TidelingCurrent.Coral:
                    return new Color32(219, 108, 119, 255);
                case TidelingCurrent.Stone:
                    return new Color32(118, 113, 103, 255);
                case TidelingCurrent.Glow:
                    return new Color32(215, 165, 55, 255);
                case TidelingCurrent.Tide:
                    return new Color32(58, 150, 139, 255);
                default:
                    return Color.white;
            }
        }
    }
}
