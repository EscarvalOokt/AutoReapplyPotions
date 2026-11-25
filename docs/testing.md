# Auto Reapply Potions testing policy

This document defines the automated testing and runtime acceptance boundaries for Auto Reapply Potions.

## Testing principle

Automated tests should protect meaningful project-owned contracts and realistic regressions. Test count and broad
method-level coverage are not goals by themselves.

## Automated test boundary

The `AutoReapplyPotions.Tests` NUnit project covers four areas of mod behavior with explicit inputs.

- **Buff expiry.** Timer snapshots distinguish a countdown to zero from early removal or refresh. Cases include
  simultaneous expirations, slot changes and duplicate buff IDs.
- **Filter mode parsing.** The parser accepts defined mode names and numeric values and falls back to `Disabled` for
  missing or invalid input.
- **Filter Editor results.** Searches match item names, IDs and categories. Tests cover an empty result in the chosen
  category and the categories available in the Selected view.
- **Search focus.** The click-away rule detects a new press outside the search field even when another widget consumes
  the click. Holding the button does not trigger the action again.

A separate smoke test checks test discovery and execution. The [development guide](development.md#tests) gives the
command for running the suite.

## Runtime acceptance boundary

Use the installed mod to check interactions with Terraria, TerrariaModder, XNA and Harmony. Launch through
`TerrariaInjector.exe`, enter a world and open the mod's settings with **F6**. Change the filter mode, restart the
game and confirm that the saved mode is restored. Enable automatic reapplication and use a filter that allows the
test potion for the following gameplay checks.

1. **Inventory priority.** Start with an active buff and a matching potion in both the inventory and an accessible
   Void Bag. Note both stack counts and let the buff expire. The buff returns and the inventory stack decreases by
   one. The Void Bag stack keeps its original count.
2. **Void Bag fallback.** Repeat with the matching potion available only in the Void Bag. The buff returns and the
   bag stack decreases by one. Repeat with the bag locked at expiry, then unlock it. The buff stays expired and
   the potion stays in the bag.
3. **Early removal.** With matching potions in both locations, right-click the active buff before its timer expires.
   The buff disappears and both stacks keep their original counts.
4. **Filter Editor.** With the game in English and Ironskin Potion unselected, open the editor with **Alt + 2**
   (default). Choose **Food** in the **Catalog** tab and search for `Ironskin`.
   The list is empty. Click **Select results** and confirm that the selection count stays the same. Clear the search
   and confirm that **Food** is still selected and its items are visible again.

## Adding tests

Add tests together with project-owned behavior when they protect a meaningful contract. Keep runtime acceptance
scenarios separate when the relevant semantics are owned by Terraria or TerrariaModder rather than this project.