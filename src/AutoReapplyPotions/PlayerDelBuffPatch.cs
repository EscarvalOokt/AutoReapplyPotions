using HarmonyLib;
using Terraria;

namespace AutoReapplyPotions
{
    [HarmonyPatch(typeof(Player), nameof(Player.DelBuff))]
    internal static class PlayerDelBuffPatch
    {
        [HarmonyPrefix]
        // ReSharper disable once InconsistentNaming
        public static void Prefix(Player __instance)
        {
            BuffReapplyDetector.BeforeRemoval(__instance);
        }
    }
}