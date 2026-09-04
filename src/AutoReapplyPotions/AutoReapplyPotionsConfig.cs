using TerrariaModder.Core.Config;

namespace AutoReapplyPotions
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class AutoReapplyPotionsConfig : ModConfig
    {
        public override int Version => 1;

        [Client]
        [Label("Enabled")]
        [Description("Enables or disables automatic buff item reapplication.")]
        public bool ModEnabled { get; set; } = true;

        [Client]
        [Label("Debug Mode")]
        [Description("Enables debug-only buff timer tools.")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool DebugModeEnabled { get; set; }

        [Client]
        [Label("Item Filter Mode")]
        [Description("Controls how the selected item list is interpreted.")]
        [Options("Disabled", "Blacklist", "Whitelist")]
        public string ItemFilterMode { get; set; } = "Disabled";

        [Client]
        [Label("Filtered Item IDs")]
        [Description("Item IDs managed by the Auto Reapply Potions filter editor.")]
        public string FilteredItemIds { get; set; } = string.Empty;
    }
}