using UnityEngine;

namespace Tidepool.Domain
{
    public static class TidelingRarityRules
    {
        private static readonly Color CommonColor = new Color(0.310f, 0.616f, 0.412f);
        private static readonly Color UncommonColor = new Color(0.247f, 0.561f, 0.710f);
        private static readonly Color RareColor = new Color(0.525f, 0.380f, 0.706f);
        private static readonly Color SecretColor = new Color(0.847f, 0.647f, 0.212f);
        private static readonly Color UndiscoveredColor = new Color(0.435f, 0.459f, 0.431f);

        public static Color GetDisplayColor(TidelingRarity rarity)
        {
            switch (rarity)
            {
                case TidelingRarity.Common:
                    return CommonColor;
                case TidelingRarity.Uncommon:
                    return UncommonColor;
                case TidelingRarity.Rare:
                    return RareColor;
                case TidelingRarity.Secret:
                    return SecretColor;
                default:
                    return UndiscoveredColor;
            }
        }

        public static Color GetUndiscoveredColor()
        {
            return UndiscoveredColor;
        }

        public static string GetDisplayName(TidelingRarity rarity)
        {
            switch (rarity)
            {
                case TidelingRarity.Common:
                    return "Common";
                case TidelingRarity.Uncommon:
                    return "Uncommon";
                case TidelingRarity.Rare:
                    return "Rare";
                case TidelingRarity.Secret:
                    return "Secret";
                default:
                    return "Unknown";
            }
        }
    }
}
