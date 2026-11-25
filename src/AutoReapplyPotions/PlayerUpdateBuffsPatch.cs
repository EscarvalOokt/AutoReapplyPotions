using HarmonyLib;
using Terraria;

// ReSharper disable InconsistentNaming

namespace AutoReapplyPotions
{
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateBuffs))]
    internal static class PlayerUpdateBuffsPatch
    {
        [HarmonyPrefix]
        public static void Prefix(Player __instance, out bool __state)
        {
            __state = BuffReapplyDetector.BeginUpdate(__instance);
        }

        [HarmonyPostfix]
        public static void Postfix(Player __instance, bool __state)
        {
            if (__state)
                BuffReapplyDetector.ProcessAfterUpdate(__instance);
        }

        [HarmonyFinalizer]
        public static void Finalizer(bool __state)
        {
            if (__state)
                BuffReapplyDetector.EndUpdate();
        }
    }
}