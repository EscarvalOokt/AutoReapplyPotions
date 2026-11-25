# Auto Reapply Potions architecture

This document defines the accepted architecture and implementation boundaries of Auto Reapply Potions.
It describes the current production baseline; it is not a Roadmap or an implementation guide.

## Sources of truth

1. The installed Terraria and TerrariaModder runtime define external runtime behavior and API contracts.
2. The [manifest](../mod/manifest.json) declares the runtime requirements. The [production project](../src/AutoReapplyPotions/AutoReapplyPotions.csproj) defines the build target and references, and its source defines the mod's behavior.
3. This document records accepted project architecture and boundaries.
4. The [development guide](development.md) covers local setup and build commands. The [testing policy](testing.md) defines automated testing and in-game acceptance boundaries.

When documentation and production code disagree about current project behavior, production code is the source of truth until the discrepancy is resolved explicitly.

## Runtime baseline

- The manifest for version `1.3.0` specifies Terraria `1.4.6.1` and TerrariaModder Core `>=0.4.3`.
- The tested Windows configuration used Terraria `1.4.6.1` and Core `0.4.3`.
- .NET Framework 4.8.
- TerrariaModder provides mod lifecycle, configuration, keybind, logging, and UI infrastructure.
- Harmony is used for runtime patching of Terraria behavior.
- TerrariaWindowKit v1.1.0 is integrated source-only through `TerrariaWindowKit.Source.projitems` with Core integration enabled. It does not require a separate toolkit DLL.

## Runtime flow

The current automatic reapplication path is:

1. `PlayerUpdateBuffsPatch` starts expiration tracking in a Harmony prefix and processes candidates in its postfix. While this update is active, a `Player.DelBuff` prefix records zero timers before Terraria compacts buff slots. A finalizer releases the thread-local state even when the update throws.
2. `BuffReapplyDetector` gates detection on successful mod initialization, the enabled setting and an active local player. It captures buff IDs with one tick remaining. It confirms expiry only after observing a zero timer for that ID. A copy with time remaining at the end of the update keeps the effect active, even if Terraria moved it to a different slot. The detector resolves all expired IDs before reapplying any buff because reapplying food can shift the slots again. The common path with no expiring buffs requires no snapshot arrays or sets.
3. `PotionReapplyHelper` skips reapplication while the player is cursed, crowd-controlled, spectating or has a pending inventory operation. It searches inventory slots `0–57` first and then `player.bank4.item` when `player.useVoidBag()` allows access.
4. Both sources use `AutoReapplyRules.IsSupportedItem(item, expiredBuffId)`, which excludes consumables that also heal life or restore mana and applies the configured item filter.
5. The helper reapplies the expired buff through `Player.AddBuff`, using the item's buff duration or a 3600-tick fallback, and consumes one item directly from the selected source stack. An exhausted stack is cleared with `TurnToAir()`.

The detector processes the local active player, and the existing reentrancy guard prevents nested reapplication. It does not queue retries for a Void Bag that is unavailable when a buff expires.

## Filter Editor and window integration

- `FilterUiController` displays the Catalog and Selected views, handles search and selection, and saves the filter mode and selected item IDs through TerrariaModder configuration. It uses TerrariaModder.Core `TextInput`, `TabBar`, `ScrollView`, `Button`, `Checkbox`, renderer, colors, and tooltips.
- On first opening the editor, the UI thread creates a `CoreWindowBackend` connected through `SharedCoordinator.Connect()` and a `WindowHost` with identity `auto-reapply-potions:filter`.
- The window is draggable and pinnable but not manually resizable. The compact layout uses a base size of 520 x 420 UI units, with width adjustment for readable mode labels.
- `WindowHost` and `CoreWindowBackend` coordinate pointer, wheel, text focus, Escape, and Pin behavior with other WindowKit consumers. `CorePinButtonRenderer` draws the shared Pin/Unpin control; the mod does not implement a second window coordinator.
- `FilterEditorClickAwayPolicy` handles the project-owned Search click-away decision, including a physical mouse press edge when the normal click signal has already been consumed by another widget.
- When the mod unloads, it closes the lifecycle gate and disposes the window host before its backend. On world load it invalidates the item catalog and refreshes the editor. Expiration tracking is scoped to the current player update.

## Supporting subsystems

- `AutoReapplyPotionsMod` is the TerrariaModder entry point and owns lifecycle/config/keybind setup.
- `ItemFilterSettings` is the shared runtime source for disabled/blacklist/whitelist filtering.
- `SupportedBuffItemsCatalog` builds and caches metadata for supported items.
- `FilterEditorResults` derives the Catalog and Selected lists from the catalog, search text, active category, and item selection. It also computes which categories are available on the active tab.
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