namespace AutoReapplyPotions
{
    internal static class ModLog
    {
        public static void Info(string message)
        {
            AutoReapplyPotionsMod.Context?.Logger?.Info($"[Auto Reapply Potions] {message}");
        }

        public static void Warn(string message)
        {
            AutoReapplyPotionsMod.Context?.Logger?.Warn($"[Auto Reapply Potions] {message}");
        }

        public static void Error(string message)
        {
            AutoReapplyPotionsMod.Context?.Logger?.Error($"[Auto Reapply Potions] {message}");
        }
    }
}