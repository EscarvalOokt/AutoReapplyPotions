# Auto Reapply Potions architecture

This document describes the runtime behavior of Auto Reapply Potions and the responsibilities of its components.

## Sources of truth

- The [manifest](../mod/manifest.json) declares runtime requirements. The [production project](../src/AutoReapplyPotions/AutoReapplyPotions.csproj) defines the build target and references.
- Terraria and TerrariaModder define the external APIs. The production source defines how the mod uses them, and the [configuration model](../src/AutoReapplyPotions/AutoReapplyPotionsConfig.cs) defines its settings.
- The [development guide](development.md) covers local setup, build, and test commands. The [testing policy](testing.md) describes automated coverage and in-game checks.

## Runtime baseline

- The manifest for version `1.3.0` specifies Terraria `1.4.6` and TerrariaModder Core `>=0.4.3`.
- The tested Windows configuration used Terraria `1.4.6.1` and Core `0.4.3`.
- The production project targets .NET Framework 4.8.
- TerrariaModder Core provides lifecycle callbacks, configuration, logging, and UI services. Harmony provides runtime method patching.
- The production project imports TerrariaWindowKit sources with Core integration enabled. The build uses the revision present in the sibling checkout and compiles it into `AutoReapplyPotions.dll`.

## Runtime flow

The current automatic reapplication path is:

1. `PlayerUpdateBuffsPatch` starts expiration tracking in a Harmony prefix and processes candidates in its postfix. While this update is active, a `Player.DelBuff` prefix records zero timers before Terraria compacts buff slots. A finalizer releases the thread-local state even when the update throws.
2. `BuffReapplyDetector` gates detection on successful mod initialization, the enabled setting and an active local player. It captures buff IDs with one tick remaining. It confirms expiry only after observing a zero timer for that ID. A copy with time remaining at the end of the update keeps the effect active, even if Terraria moved it to a different slot. The detector resolves all expired IDs before reapplying any buff because reapplying food can shift the slots again. The common path with no expiring buffs requires no snapshot arrays or sets.
3. `PotionReapplyHelper` skips reapplication while the player is cursed, crowd-controlled, spectating or has a pending inventory operation. It searches inventory slots `0–57` first and then `player.bank4.item` when `player.useVoidBag()` allows access.
4. Both sources use `AutoReapplyRules.IsSupportedItem(item, expiredBuffId)`, which excludes consumables that also heal life or restore mana and applies the configured item filter.
5. The helper reapplies the expired buff through `Player.AddBuff`, using the item's buff duration or a 3600-tick fallback, and consumes one item directly from the selected source stack. An exhausted stack is cleared with `TurnToAir()`.

The detector processes the local active player, and the existing reentrancy guard prevents nested reapplication. It does not queue retries for a Void Bag that is unavailable when a buff expires.

## Filter Editor and window integration

- `FilterUiController` displays the Catalog and Selected views, handles search and selection, and saves the filter mode and selected item IDs through TerrariaModder configuration. TerrariaModder Core provides its widgets, rendering, and tooltips.
- On first opening the editor, the UI thread creates a `CoreWindowBackend` connected through `SharedCoordinator.Connect()` and a `WindowHost` with identity `auto-reapply-potions:filter`.
- The window is draggable and pinnable but not manually resizable. The compact layout uses a base size of 520 x 420 UI units, with width adjustment for readable mode labels.
- `WindowHost` and `CoreWindowBackend` coordinate pointer, wheel, text focus, Escape, and Pin behavior with other WindowKit consumers. `CorePinButtonRenderer` draws the shared Pin/Unpin control.
- `FilterEditorClickAwayPolicy` handles the project-owned Search click-away decision, including a physical mouse press edge when the normal click signal has already been consumed by another widget.
- When the mod unloads, it closes the lifecycle gate and disposes the window host before its backend. On world load it invalidates the item catalog and refreshes the editor.

## Supporting subsystems

- `AutoReapplyPotionsMod` is the TerrariaModder entry point and owns lifecycle/config/keybind setup.
- `ItemFilterSettings` is the shared runtime source for disabled/blacklist/whitelist filtering.
- `SupportedBuffItemsCatalog` builds and caches metadata for supported items.
- `FilterEditorResults` derives the Catalog and Selected lists from the catalog, search text, active category, and item selection. It also computes which categories are available on the active tab.
- `SupportedBuffItemClassifier` provides catalog categories.
- `DebugBuffTools` provides debug-only buff timer helpers.
- `ModLog` is the common logging adapter.

## Project boundaries

- `AutoReapplyRules` determines which buffs and items qualify. `ItemFilterSettings` applies the configured filter to both inventory sources.
- The inventory and Void Bag paths use Terraria's existing item stacks and the same filter settings. Terraria owns storage, persistence, and networking. Multiplayer synchronization has not been separately verified.