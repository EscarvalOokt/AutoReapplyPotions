using Terraria;

namespace AutoReapplyPotions
{
    internal static class PotionReapplyHelper
    {
        public static bool TryReapplyBuffFromInventory(Player player, int expiredBuffId)
        {
            bool isInvalidInput = player == null || expiredBuffId <= 0;
            if (isInvalidInput)
                return false;

            int slot = FindMatchingBuffItemSlot(player, expiredBuffId);
            if (slot < 0)
                return false;

            Item item = player.inventory[slot];
            bool isMissingItem = item == null || item.IsAir;
            if (isMissingItem)
                return false;

            if (!CanUseForAutoReapply(item, expiredBuffId))
                return false;

            int buffTime = item.buffTime;
            if (buffTime <= 0)
                buffTime = 3600;

            player.AddBuff(item.buffType, buffTime);

            if (item.consumable)
            {
                ConsumeOneItem(player, slot);
            }

            return true;
        }

        private static int FindMatchingBuffItemSlot(Player player, int buffId)
        {
            for (var i = 0; i < player.inventory.Length; i++)
            {
                Item item = player.inventory[i];

                bool isMissingItem = item == null || item.IsAir;
                if (isMissingItem)
                    continue;

                if (!CanUseForAutoReapply(item, buffId))
                    continue;

                return i;
            }

            return -1;
        }

        private static bool CanUseForAutoReapply(Item item, int buffId)
        {
            return AutoReapplyRules.IsSupportedItem(item, buffId);
        }

        private static void ConsumeOneItem(Player player, int slot)
        {
            Item item = player.inventory[slot];
            bool isMissingItem = item == null || item.IsAir;
            if (isMissingItem)
                return;

            item.stack--;

            if (item.stack <= 0)
            {
                item.TurnToAir();
            }
        }
    }
}