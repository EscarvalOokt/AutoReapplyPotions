# Auto Reapply Potions architecture

This document defines the accepted architecture and implementation boundaries of Auto Reapply Potions.
It describes the current production baseline; it is not a Roadmap or an implementation guide.

## Sources of truth

1. The installed Terraria and TerrariaModder runtime define external runtime behavior and API contracts.
2. The current production implementation defines behavior actually present in the mod.
3. This document records accepted project architecture and boundaries.
4. `docs/testing.md` defines automated testing and runtime acceptance boundaries.

When documentation and production code disagree about current project behavior, production code is the source of truth until the discrepancy is resolved explicitly.

## Runtime baseline

- Terraria 1.4.5.8 target environment.
- TerrariaModder Core `>=0.4.1` declared minimum; Core 0.4.2 is the primary intended runtime validation configuration. Compatibility with Core 0.4.1 has not been separately verified.
- .NET Framework 4.8.
- TerrariaModder provides mod lifecycle, configuration, keybind, logging, and UI infrastructure.
- Harmony is used for runtime patching of Terraria behavior.
- TerrariaWindowKit v1.1.0 is integrated source-only through `TerrariaWindowKit.Source.projitems` with Core integration enabled. It does not require a separate toolkit DLL.

## Runtime flow

The current automatic reapplication path is:

1. `PlayerUpdateBuffsPatch` wraps `Player.UpdateBuffs` with Harmony prefix/postfix hooks.
2. `BuffReapplyDetector` captures a `BuffSnapshot` before the update and compares it with the local player's buffs afterward, processing supported expired buff IDs.
3. `PotionReapplyHelper` searches slots `0–57` of `player.inventory` first, excluding the temporary cursor slot `58`. If no eligible item is found, it searches `player.bank4.item` only when `player.useVoidBag()` succeeds and `Main.LocalPlayerHasPendingInventoryActions()` is false.
4. Both sources use `AutoReapplyRules.IsSupportedItem(item, expiredBuffId)`, including the shared `ItemFilterSettings` Disabled/Blacklist/Whitelist rule for the specific item ID.
5. The helper reapplies the expired buff through `Player.AddBuff`, using the item's buff duration or a 3600-tick fallback, and consumes one item directly from the selected source stack. An exhausted stack is cleared with `TurnToAir()`.

The detector processes the local active player, and the existing reentrancy guard prevents nested reapplication. It does not queue retries for a Void Bag that is unavailable when a buff expires.

## Filter Editor and window integration

- `FilterUiController` owns the mod-specific catalog, item filtering actions, Search, and selected-item views. It uses TerrariaModder.Core `TextInput`, `TabBar`, `ScrollView`, `Button`, `Checkbox`, renderer, colors, and tooltips.
- On first opening the editor, the UI thread creates a `CoreWindowBackend` connected through `SharedCoordinator.Connect()` and a `WindowHost` with identity `auto-reapply-potions:filter`.
- The window is draggable and pinnable but not manually resizable. The compact layout uses a base size of 520 x 420 UI units, with width adjustment for readable mode labels.
- `WindowHost` and `CoreWindowBackend` coordinate pointer, wheel, text focus, Escape, and Pin behavior with other WindowKit consumers. `CorePinButtonRenderer` draws the shared Pin/Unpin control; the mod does not implement a second window coordinator.
- `FilterEditorClickAwayPolicy` handles the project-owned Search click-away decision, including a physical mouse press edge when the normal click signal has already been consumed by another widget.
- When the mod unloads, `FilterUiController` disposes the window host before its backend. `OnWorldLoad` resets `BuffReapplyDetector`, invalidates `SupportedBuffItemsCatalog`, and refreshes the existing filter UI from configuration. `OnWorldUnload` resets `BuffReapplyDetector`.

## Supporting subsystems

- `AutoReapplyPotionsMod` is the TerrariaModder entry point and owns lifecycle/config/keybind setup.
- `ItemFilterSettings` is the shared runtime source for disabled/blacklist/whitelist filtering.
- `SupportedBuffItemsCatalog` builds and caches the supported item catalog used by the filter UI.
- `SupportedBuffItemClassifier` provides catalog categories.
- `DebugBuffTools` provides debug-only buff timer helpers.
- `ModLog` is the common logging adapter.

## Project boundaries

- Production source remains in one .NET Framework 4.8 project; WindowKit sources are imported rather than deployed as a separate library.
- Conceptual subsystems should not become additional layers or projects without a concrete implementation need.
- Shared eligibility rules belong in `AutoReapplyRules`; filtering rules belong in `ItemFilterSettings` rather than being duplicated at call sites.
- TerrariaModder.Core and TerrariaWindowKit supply UI widgets, styling, window lifecycle, and input coordination rather than a parallel UI framework.
- The Void Bag extension adds no new configuration field, independent item storage, save format, or custom multiplayer synchronization. Native persistence and network code paths are not evidence that mod-specific multiplayer synchronization has been verified.
- Machine-specific game/framework paths belong only in `src/AutoReapplyPotions.Local.props` and must not be committed.