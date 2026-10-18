using Terraria;

namespace AutoReapplyPotions
{
    internal static class PotionReapplyHelper
    {
        public static bool TryReapplyBuff(Player player, int expiredBuffId)
        {
            bool isInvalidInput = player == null || expiredBuffId <= 0;
            if (isInvalidInput)
                return false;

            int slot = FindMatchingBuffItemSlot(player, expiredBuffId);
            Item item = slot >= 0 ? player.inventory[slot] : FindMatchingBuffItemInVoidBag(player, expiredBuffId);

            bool isMissingItem = item == null || item.IsAir || item.stack <= 0;
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
                ConsumeOneItem(item);
            }

            return true;
        }

        private static int FindMatchingBuffItemSlot(Player player, int buffId)
        {
            for (var i = 0; i < 58 && i < player.inventory.Length; i++)
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

        private static Item FindMatchingBuffItemInVoidBag(Player player, int buffId)
        {
            if (!player.useVoidBag() || Main.LocalPlayerHasPendingInventoryActions())
                return null;

            Chest bank4 = player.bank4;
            if (bank4?.item == null)
                return null;

            for (var i = 0; i < bank4.maxItems && i < bank4.item.Length; i++)
            {
                Item item = bank4.item[i];

                bool isMissingItem = item == null || item.IsAir || item.stack <= 0;
                if (isMissingItem)
                    continue;

                if (!CanUseForAutoReapply(item, buffId))
                    continue;

                return item;
            }

            return null;
        }

        private static bool CanUseForAutoReapply(Item item, int buffId)
        {
            return AutoReapplyRules.IsSupportedItem(item, buffId);
        }

        private static void ConsumeOneItem(Item item)
        {
            bool isMissingItem = item == null || item.IsAir || item.stack <= 0;
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