using Terraria;
using Terraria.ID;

namespace AutoReapplyPotions
{
    internal static class SupportedBuffItemClassifier
    {
        public static SupportedBuffItemCategory Classify(Item item)
        {
            bool isMissingItem = item == null || item.IsAir;
            if (isMissingItem)
                return SupportedBuffItemCategory.Unknown;

            if (IsFood(item))
                return SupportedBuffItemCategory.Food;

            if (IsFlask(item))
                return SupportedBuffItemCategory.Flask;

            if (AutoReapplyRules.IsSupportedItem(item))
                return SupportedBuffItemCategory.Potion;

            return SupportedBuffItemCategory.Other;
        }

        private static bool IsFood(Item item)
        {
            bool isFoodBuff = item.buffType == BuffID.WellFed || item.buffType == BuffID.WellFed2 ||
                              item.buffType == BuffID.WellFed3;
            return isFoodBuff;
        }

        private static bool IsFlask(Item item)
        {
            switch (item.buffType)
            {
                case 71:
                case 73:
                case 74:
                case 75:
                case 76:
                case 77:
                case 78:
                case 79:
                    return true;

                default:
                    return false;
            }
        }
    }
}