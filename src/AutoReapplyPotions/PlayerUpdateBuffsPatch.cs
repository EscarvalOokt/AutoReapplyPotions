using HarmonyLib;
using Terraria;

// ReSharper disable InconsistentNaming

namespace AutoReapplyPotions
{
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateBuffs))]
    internal static class PlayerUpdateBuffsPatch
    {
        [HarmonyPrefix]
        public static void Prefix(Player __instance)
        {
            BuffReapplyDetector.CaptureBeforeUpdate(__instance);
        }

        [HarmonyPostfix]
        public static void Postfix(Player __instance)
        {
            BuffReapplyDetector.ProcessAfterUpdate(__instance);
        }
    }
}