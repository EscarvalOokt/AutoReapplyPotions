using Terraria;
using Terraria.ID;

namespace AutoReapplyPotions
{
    internal static class SupportedBuffItemClassifier
    {
        public static SupportedBuffItemCategory Classify(Item item)
        {
            if (item == null || item.IsAir || item.buffType <= 0 || item.buffType >= BuffID.Count)
                return SupportedBuffItemCategory.Unknown;

            if (BuffID.Sets.IsWellFed[item.buffType])
                return SupportedBuffItemCategory.Food;

            if (BuffID.Sets.IsAFlaskBuff[item.buffType])
                return SupportedBuffItemCategory.Flask;

            if (item.useStyle == ItemUseStyleID.DrinkLiquid)
                return SupportedBuffItemCategory.Potion;

            return SupportedBuffItemCategory.Other;
        }
    }
}