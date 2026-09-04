# Auto Reapply Potions architecture

This document defines the accepted architecture and implementation boundaries of Auto Reapply Potions.
It describes the current production baseline; it is not a RoadMap or an implementation guide.

## Sources of truth

1. The installed Terraria and TerrariaModder runtime define external runtime behavior and API contracts.
2. The current production implementation defines behavior actually present in the mod.
3. This document records accepted project architecture and boundaries.
4. `docs/testing.md` defines automated testing and runtime acceptance boundaries.

When documentation and production code disagree about current project behavior, production code is the source of truth until the discrepancy is resolved explicitly.

## Runtime baseline

- Terraria 1.4.5.8 target environment.
- TerrariaModder 0.4.0 or newer.
- .NET Framework 4.8.
- TerrariaModder provides mod lifecycle, configuration, keybind, logging, and UI infrastructure.
- Harmony is used for runtime patching of Terraria behavior.

## Runtime flow

The current automatic reapplication path is:

1. `PlayerUpdateBuffsPatch` wraps `Player.UpdateBuffs` with Harmony prefix/postfix hooks.
2. `BuffReapplyDetector` captures a `BuffSnapshot` before the update and compares it with the player's buffs afterward.
3. Expired buff IDs are checked through `AutoReapplyRules`.
4. `PotionReapplyHelper` scans inventory for the first matching supported item allowed by `ItemFilterSettings`.
5. The helper reapplies the buff and consumes one item from the selected stack.

## Supporting subsystems

- `AutoReapplyPotionsMod` is the TerrariaModder entry point and owns lifecycle/config/keybind setup.
- `ItemFilterSettings` is the shared runtime source for disabled/blacklist/whitelist filtering.
- `SupportedBuffItemsCatalog` builds and caches the supported item catalog used by the filter UI.
- `SupportedBuffItemClassifier` provides catalog categories.
- `FilterUiController` is the in-game filter editor and reuses TerrariaModder.Core UI widgets.
- `DebugBuffTools` provides debug-only buff timer helpers.
- `ModLog` is the common logging adapter.

## Project boundaries

- Production source remains in a single project until implementation complexity justifies additional projects or internal folders.
- Conceptual subsystems should not be turned into directory/layer abstractions without a concrete implementation need.
- Shared eligibility rules belong in `AutoReapplyRules`; filtering rules belong in `ItemFilterSettings` rather than being duplicated at call sites.
- Existing TerrariaModder.Core UI widgets and styling should be reused instead of introducing a parallel UI framework.
- Machine-specific game/framework paths belong only in `src/AutoReapplyPotions.Local.props` and must not be committed.