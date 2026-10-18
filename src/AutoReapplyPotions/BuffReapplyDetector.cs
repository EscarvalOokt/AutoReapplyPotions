using System.Collections.Generic;
using Terraria;

namespace AutoReapplyPotions
{
    internal static class BuffReapplyDetector
    {
        private static readonly Dictionary<int, BuffSnapshot> _beforeUpdateSnapshots =
            new Dictionary<int, BuffSnapshot>();

        private static readonly HashSet<int> _reapplyingPlayers = new HashSet<int>();

        public static void Reset()
        {
            _beforeUpdateSnapshots.Clear();
            _reapplyingPlayers.Clear();
        }

        public static void CaptureBeforeUpdate(Player player)
        {
            if (!ShouldProcessPlayer(player))
                return;

            var snapshot = new BuffSnapshot(player);
            _beforeUpdateSnapshots[player.whoAmI] = snapshot;
        }

        public static void ProcessAfterUpdate(Player player)
        {
            if (!ShouldProcessPlayer(player))
                return;

            if (!AutoReapplyPotionsMod.IsEnabled)
                return;

            int playerId = player.whoAmI;

            if (!_beforeUpdateSnapshots.TryGetValue(playerId, out BuffSnapshot snapshot))
                return;

            if (_reapplyingPlayers.Contains(playerId))
                return;

            try
            {
                _reapplyingPlayers.Add(playerId);

                HashSet<int> expiredBuffs = FindExpiredBuffs(player, snapshot);

                foreach (int buffId in expiredBuffs)
                {
                    if (!ShouldAutoReapplyBuff(buffId))
                        continue;

                    if (PotionReapplyHelper.TryReapplyBuff(player, buffId))
                        ModLog.Info($"Reapplied buff {buffId}.");
                }
            }
            finally
            {
                _beforeUpdateSnapshots.Remove(playerId);
                _reapplyingPlayers.Remove(playerId);
            }
        }

        private static bool ShouldProcessPlayer(Player player)
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

        private static HashSet<int> FindExpiredBuffs(Player player, BuffSnapshot snapshot)
        {
            HashSet<int> before = CollectActiveTimedBuffs(snapshot.BuffTypes, snapshot.BuffTimes);
            HashSet<int> after = CollectActiveTimedBuffs(player.buffType, player.buffTime);

            before.ExceptWith(after);
            return before;
        }

        private static HashSet<int> CollectActiveTimedBuffs(int[] buffTypes, int[] buffTimes)
        {
            var result = new HashSet<int>();

            int count = buffTypes.Length < buffTimes.Length ? buffTypes.Length : buffTimes.Length;

            for (var i = 0; i < count; i++)
            {
                int buffType = buffTypes[i];
                int buffTime = buffTimes[i];

                if (buffType <= 0)
                    continue;

                if (buffTime <= 0)
                    continue;

                result.Add(buffType);
            }

            return result;
        }

        private static bool ShouldAutoReapplyBuff(int buffId)
        {
            return AutoReapplyRules.IsSupportedBuff(buffId);
        }
    }
}