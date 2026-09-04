using System;
using System.Collections.Generic;
using Terraria;
using TerrariaModder.Core;

namespace AutoReapplyPotions
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class AutoReapplyPotionsMod : IMod, IModLifecycle
    {
        private FilterUiController _filterUi;
        internal static ModContext Context { get; private set; }
        internal static AutoReapplyPotionsConfig Config { get; private set; }

        internal static bool IsEnabled { get; private set; } = true;

        public bool DebugModeEnabled { get; private set; }

        public string Id => "auto-reapply-potions";
        public string Name => "Auto Reapply Potions";
        public string Version => "1.1.0";

        public void Initialize(ModContext context)
        {
            Context = context;
            Config = context.GetConfig<AutoReapplyPotionsConfig>() ??
                     throw new InvalidOperationException("AutoReapplyPotionsConfig was not loaded by TerrariaModder.");

            BuffReapplyDetector.Reset();
            LoadConfig();

            _filterUi = new FilterUiController();
            _filterUi.Initialize(context);

            context.RegisterKeybind(
                "toggle-mod-enabled",
                "Toggle Auto Reapply Potions",
                "Enables or disables automatic buff item reapplication.",
                "ALT + 1",
                (Action)OnToggleModKeyPressed);

            context.RegisterKeybind(
                "open-filter-ui",
                "Open Filter UI",
                "Opens the in-game filter editor for supported buff items.",
                "ALT + 2",
                (Action)OnOpenFilterUiKeyPressed);

            context.RegisterKeybind(
                "debug-reduce-all-buffs",
                "[Debug] Reduce All Buffs To 5 Seconds",
                "Sets all active timed buffs to 5 seconds for debugging.",
                "ALT + 3",
                (Action)OnReduceAllBuffsKeyPressed);

            context.RegisterKeybind(
                "debug-reduce-random-buff",
                "[Debug] Reduce Random Buff To 5 Seconds",
                "Sets one random active timed buff to 5 seconds for debugging.",
                "ALT + 4",
                (Action)OnReduceRandomBuffKeyPressed);

            ModLog.Info($"Initialized. Enabled={IsEnabled}, Debug={DebugModeEnabled}");
        }

        public void Unload()
        {
            BuffReapplyDetector.Reset();

            _filterUi?.Unload();
            _filterUi = null;

            IsEnabled = true;
            Config = null;
            Context = null;
        }

        public void OnContentReady(ModContext _)
        {
            // No content-ready initialization is required.
        }

        public void OnWorldLoad()
        {
            BuffReapplyDetector.Reset();
            SupportedBuffItemsCatalog.Invalidate();
            _filterUi?.RefreshFromConfig();
            ModLog.Info("World loaded.");
        }

        public void OnWorldUnload()
        {
            BuffReapplyDetector.Reset();
            ModLog.Info("World unloaded.");
        }

        public void OnConfigChanged()
        {
            LoadConfig();
            _filterUi?.RefreshFromConfig();
            ModLog.Info(
                $"Config reloaded. Enabled={IsEnabled}, Debug={DebugModeEnabled}, FilterMode={ItemFilterSettings.Mode}, FilterCount={ItemFilterSettings.FilteredItemIds.Count}");
        }

        private void LoadConfig()
        {
            if (Config == null)
                return;

            IsEnabled = Config.ModEnabled;
            DebugModeEnabled = Config.DebugModeEnabled;

            ItemFilterMode filterMode = ParseItemFilterMode(Config.ItemFilterMode);

            List<int> parsedIds = ItemFilterSettings.ParseItemIds(Config.FilteredItemIds);
            ItemFilterSettings.Update(filterMode, parsedIds);

            ModLog.Info(
                $"Config loaded. Enabled={IsEnabled}, Debug={DebugModeEnabled}, FilterMode={filterMode}, AcceptedIds={ItemFilterSettings.FilteredItemIds.Count}");

            if (DebugModeEnabled)
            {
                ModLog.Info($"[Debug] Raw item filter mode: '{Config.ItemFilterMode}'");
                ModLog.Info($"[Debug] Parsed IDs count: {parsedIds.Count}");
                ModLog.Info($"[Debug] Serialized filtered IDs: '{ItemFilterSettings.SerializeToConfigString()}'");
            }
        }

        private void SaveEnabledState()
        {
            try
            {
                if (Config == null)
                    return;

                Config.ModEnabled = IsEnabled;
                Config.Save();
            }
            catch (Exception ex)
            {
                ModLog.Error($"Failed to save mod enabled state: {ex}");
            }
        }

        private void OnToggleModKeyPressed()
        {
            IsEnabled = !IsEnabled;
            SaveEnabledState();
            ModLog.Info($"Auto reapply is now {(IsEnabled ? "enabled" : "disabled")}.");
        }

        private void OnReduceAllBuffsKeyPressed()
        {
            if (!DebugModeEnabled)
            {
                ModLog.Info("Debug mode is disabled.");
                return;
            }

            Player player = Main.LocalPlayer;
            DebugBuffTools.ReduceAllTimedBuffsToFiveSeconds(player);
        }

        private void OnReduceRandomBuffKeyPressed()
        {
            if (!DebugModeEnabled)
            {
                ModLog.Info("Debug mode is disabled.");
                return;
            }

            Player player = Main.LocalPlayer;
            DebugBuffTools.ReduceRandomTimedBuffToFiveSeconds(player);
        }

        private void OnOpenFilterUiKeyPressed()
        {
            _filterUi?.Toggle();
        }

        private static ItemFilterMode ParseItemFilterMode(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return ItemFilterMode.Disabled;

            if (Enum.TryParse(raw, ignoreCase: true, out ItemFilterMode result))
                return result;

            return ItemFilterMode.Disabled;
        }
    }
}