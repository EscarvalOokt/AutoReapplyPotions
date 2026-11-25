using System;
using Terraria;

namespace AutoReapplyPotions
{
    internal static class BuffReapplyDetector
    {
        [ThreadStatic] private static UpdateState _current;

        public static bool BeginUpdate(Player player)
        {
            if (_current.Player != null || !ShouldProcessPlayer(player))
                return false;

            var candidates = BuffSnapshot.Capture(player.buffType, player.buffTime);
            if (candidates.Count == 0)
                return false;

            _current = new UpdateState { Player = player, Candidates = candidates };
            return true;
        }

        public static void BeforeRemoval(Player player)
        {
            if (_current.Player != player)
                return;

            _current.ObservedAtZero = _current.Candidates.FindExpired(
                player.buffType,
                player.buffTime,
                _current.ObservedAtZero);
        }

        public static void ProcessAfterUpdate(Player player)
        {
            if (!ShouldProcessPlayer(player))
                return;

            // Resolve all expirations before reapplying food can compact the slots again.
            BuffSnapshot expired = _current.Candidates.FindExpired(
                player.buffType,
                player.buffTime,
                _current.ObservedAtZero);
            for (var i = 0; i < expired.Count; i++)
            {
                int buffId = expired[i];
                if (AutoReapplyRules.IsSupportedBuff(buffId) && PotionReapplyHelper.TryReapplyBuff(player, buffId))
                    ModLog.Info($"Reapplied buff {buffId}.");
            }
        }

        public static void EndUpdate()
        {
            _current = default;
        }

        private static bool ShouldProcessPlayer(Player player)
        {
            return AutoReapplyPotionsMod.CanAutoReapply &&
                   player is { active: true, dead: false } &&
                   !Main.gameMenu &&
                   player.whoAmI == Main.myPlayer;
        }

        private struct UpdateState
        {
            public Player Player;
            public BuffSnapshot Candidates;
            public BuffSnapshot ObservedAtZero;
        }
    }
}