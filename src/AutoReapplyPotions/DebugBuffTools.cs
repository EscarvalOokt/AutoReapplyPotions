using System;
using System.Collections.Generic;
using Terraria;

namespace AutoReapplyPotions
{
    internal static class DebugBuffTools
    {
        private const int FiveSecondsInTicks = 60 * 5;
        private static readonly Random _random = new Random();

        public static int ReduceAllTimedBuffsToFiveSeconds(Player player)
        {
            if (!CanProcess(player))
                return 0;

            var changedCount = 0;

            for (var i = 0; i < player.buffType.Length && i < player.buffTime.Length; i++)
            {
                if (!CanReduceBuff(player, i))
                    continue;

                player.buffTime[i] = FiveSecondsInTicks;
                changedCount++;
            }

            ModLog.Info(
                changedCount > 0
                    ? $"Debug: reduced {changedCount} timed buff(s) to 5 seconds."
                    : "Debug: no timed buffs found to reduce.");

            return changedCount;
        }

        public static bool ReduceRandomTimedBuffToFiveSeconds(Player player)
        {
            if (!CanProcess(player))
                return false;

            var validIndices = new List<int>();

            for (var i = 0; i < player.buffType.Length && i < player.buffTime.Length; i++)
            {
                if (CanReduceBuff(player, i))
                    validIndices.Add(i);
            }

            if (validIndices.Count == 0)
            {
                ModLog.Info("Debug: no timed buffs found to reduce.");
                return false;
            }

            int randomIndex = _random.Next(validIndices.Count);
            int selectedIndex = validIndices[randomIndex];
            int buffType = player.buffType[selectedIndex];
            int oldTime = player.buffTime[selectedIndex];

            player.buffTime[selectedIndex] = FiveSecondsInTicks;

            ModLog.Info($"Debug: reduced random timed buff {buffType} from {oldTime} ticks to 5 seconds.");
            return true;
        }

        private static bool CanProcess(Player player)
        {
            if (player == null)
                return false;

            if (Main.gameMenu)
                return false;

            bool isInactiveOrDead = !player.active || player.dead;
            if (isInactiveOrDead)
                return false;

            return player.whoAmI == Main.myPlayer;
        }

        private static bool CanReduceBuff(Player player, int index)
        {
            int buffType = player.buffType[index];
            int buffTime = player.buffTime[index];

            if (buffType <= 0)
                return false;

            if (buffTime <= 0)
                return false;

            if (buffTime <= FiveSecondsInTicks)
                return false;

            return true;
        }
    }
}