using Terraria;

namespace AutoReapplyPotions
{
    internal sealed class BuffSnapshot
    {
        public BuffSnapshot(Player player)
        {
            BuffTypes = new int[player.buffType.Length];
            BuffTimes = new int[player.buffTime.Length];

            for (var i = 0; i < player.buffType.Length; i++)
            {
                BuffTypes[i] = player.buffType[i];
                BuffTimes[i] = player.buffTime[i];
            }
        }

        public int[] BuffTypes { get; }
        public int[] BuffTimes { get; }
    }
}