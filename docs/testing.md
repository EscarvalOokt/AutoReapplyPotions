# Auto Reapply Potions testing policy

This document defines the automated testing and runtime acceptance boundaries for Auto Reapply Potions.

## Testing principle

Check mod behavior with explicit inputs in automated tests. Verify interactions with Terraria and TerrariaModder in the installed game.

## Automated test boundary

The `AutoReapplyPotions.Tests` NUnit project covers four areas of mod behavior.

- **Buff expiry.** Timer snapshots distinguish a countdown to zero from early removal or refresh. Cases include simultaneous expirations, slot changes and duplicate buff IDs.
- **Filter mode parsing.** The parser accepts defined mode names and numeric values and falls back to `Disabled` for missing or invalid input.
- **Filter Editor results.** Searches match item names, IDs and categories. Tests cover an empty result in the chosen category and the categories available in the Selected view.
- **Search focus.** The focus rule detects a new press outside the search field, including when another widget has consumed the click. Holding the button does not trigger the action again.

A separate smoke test checks test discovery and execution. See the [development guide](development.md#tests) for the command to run the suite.

## Runtime acceptance boundary

Enter a world with automatic reapplication enabled. Use a supported potion with a removable timed buff, and start each expiry check with the buff active. Record stack counts after manually applying the buff.

1. **Inventory priority.** With filtering disabled, put matching potions in the inventory and an accessible Void Bag. Let the buff expire. It should return, consuming one potion from the inventory and none from the bag.
2. **Void Bag fallback.** Repeat with matching potions only in the accessible bag. The buff should return and the bag stack should decrease by one. Repeat with the bag locked at expiry, then unlock it. The buff should stay expired and the bag stack should keep its original count.
3. **Early removal.** With matching potions in both locations, right-click the buff before its timer expires. It should disappear and both stacks should keep their original counts.
4. **Whitelist.** With potions in the inventory, choose **Whitelist** and start with an empty list. When the buff expires, it should stay expired and the stack should keep its original count. Add the potion to the whitelist and restart the game. Check that the mode and selected potion were saved. Manually activate the buff again and note the new count. When it expires this time, it should return and consume one more potion.
5. **Filter Editor.** With the game in English and Ironskin Potion unselected, open the editor with **Alt + 2** (default). Choose **Food** in **Catalog** and search for `Ironskin`. The list should be empty. Click **Select results** and check that the selection count stays the same. Clear the search. **Food** should still be selected and its items should reappear.

## Adding tests

Add an automated test when explicit inputs can demonstrate a new rule or reproduce a bug. For behavior that depends on the game, document the setup, action, and expected result in the relevant in-game scenario.