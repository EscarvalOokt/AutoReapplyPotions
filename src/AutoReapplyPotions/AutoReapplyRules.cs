using Terraria;
using Terraria.ID;

namespace AutoReapplyPotions
{
    internal static class AutoReapplyRules
    {
        public static bool IsSupportedBuff(int buffId)
        {
            if (buffId <= 0)
                return false;

            if (buffId >= BuffID.Count)
                return false;

            if (Main.debuff[buffId])
                return false;

            if (BuffID.Sets.TimeLeftDoesNotDecrease[buffId])
                return false;

            bool isBlockedBuff = buffId == BuffID.PotionSickness || buffId == BuffID.ManaSickness;
            if (isBlockedBuff)
                return false;

            return true;
        }

        public static bool IsSupportedItem(Item item)
        {
            bool isMissingItem = item == null || item.IsAir;
            if (isMissingItem)
                return false;

            if (!item.consumable)
                return false;

            if (item.buffType <= 0)
                return false;

            if (item.useStyle <= 0)
                return false;

            if (!IsSupportedBuff(item.buffType))
                return false;

            return true;
        }

        public static bool IsSupportedItem(Item item, int expectedBuffId)
        {
            if (!IsSupportedItem(item))
                return false;

            if (item.buffType != expectedBuffId)
                return false;

            if (!ItemFilterSettings.IsItemAllowed(item.type))
                return false;

            return true;
        }
    }
}