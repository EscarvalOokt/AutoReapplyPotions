# Auto Reapply Potions

![Auto Reapply Potions preview](assets/nexus/Preview.png)

**Keep your buffs active without constantly reusing consumables.**

Auto Reapply Potions is a quality-of-life mod for Terraria that automatically reapplies supported consumable buffs when they expire.

When a supported timed buff ends, the mod first looks for an allowed consumable in the local player's inventory. If none is found, it can use a matching item from an accessible Void Bag. Each reapplication consumes one item from the selected stack.

It supports common buff sources such as potions, food, and weapon flasks, and includes configurable whitelist and blacklist filtering for controlling which items may be used automatically.

## Features

- Automatically reapply supported consumable buffs after their current effect fully expires.
- Use matching items from the inventory first, then from an accessible Void Bag when needed.
- Support common buff sources including potions, food, and weapon flasks.
- Choose between Disabled, Blacklist, and Whitelist filtering modes.
- Manage allowed and blocked items through the in-game Filter Editor.
- Toggle automatic reapplication with a configurable keybind.
- Enable optional debug tools for testing buff expiration.

## How it works

Auto Reapply Potions tracks active timed buffs on the local player.

When a supported buff reaches the end of its timer:

1. The mod searches the player's inventory for the first supported item allowed by the current filter.
2. If no allowed inventory item matches, it checks an accessible Void Bag for an item allowed by the same filter.
3. The buff is reapplied using the selected item's buff duration, and one item is consumed from that inventory or Void Bag stack.

Removing a buff early or displacing it when buff slots are full leaves the consumable stack untouched. While the player is cursed, crowd-controlled, spectating, or waiting for an inventory operation, the mod skips that reapplication.

A Void Bag must be available and unlocked in the player's inventory for its stored items to be used. The mod does not access it during pending inventory operations or queue a later reapplication if it is unavailable when a buff expires.

The mod does not automatically reapply:

- Debuffs.
- Potion Sickness.
- Mana Sickness.
- Consumables that also restore health or mana, including Bottled Honey.
- Permanent buffs whose remaining duration does not decrease.
- Items that do not use Terraria's supported consumable buff-item behavior.

## Item filtering

The built-in Filter Editor lets you control which supported items may be consumed automatically.

Open the editor with the **Open Filter UI** keybind and choose a filter mode.

- **Disabled** allows all supported items.
- **Blacklist** excludes selected items from automatic use.
- **Whitelist** allows only selected items. An empty whitelist prevents automatic reapplication.

The Filter Editor includes the following tools.

- Search by item name, ID, or category within the selected category. An empty search result keeps that category selected.
- Catalog and Selected views, with available Potions, Food, Flasks, and Other categories.
- **Select results** adds every matching item to the selection. **Clear results** removes every matching item from it. Both actions include rows outside the visible scroll area. **Clear all** clears the entire selection.
- Item previews and tooltips.
- A compact, draggable window with **Pin/Unpin** controls.

Pin prevents Escape from closing the editor. Opening the game's options menu or leaving the world still closes it.

Filter settings are saved through the TerrariaModder configuration system.

## Controls

Default keybinds:

- **Alt + 1** — Toggle Auto Reapply Potions.
- **Alt + 2** — Open Filter UI.
- **Alt + 3** — Reduce all active timed buffs with more than 5 seconds remaining to 5 seconds when Debug Mode is enabled.
- **Alt + 4** — Reduce one randomly chosen active timed buff with more than 5 seconds remaining to 5 seconds when Debug Mode is enabled.

Keybinds can be configured through TerrariaModder.

## Settings

Press **F6** and select **Auto Reapply Potions** to open its settings.

- **Enabled** turns automatic reapplication on or off. It is on by default.
- **Debug Mode** enables the two buff timer shortcuts listed under [Controls](#controls). It is off by default.
- **Item Filter Mode** defaults to **Disabled**. Use the [Filter Editor](#item-filtering) to choose a mode and manage the **Filtered Item IDs** list, which starts empty.

## Requirements

- **Terraria 1.4.6** for Windows.
- **TerrariaModder Core 0.4.3 or newer.** Choose a compatible Core release from [Nexus Mods](https://www.nexusmods.com/terraria/mods/135?tab=files) or [GitHub Releases](https://github.com/Inidar1/terraria-modder/releases).
- [.NET Framework 4.8 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48).

## Installation

1. Close Terraria and install any missing dependencies listed under [Requirements](#requirements). For a manual TerrariaModder Core installation, extract the Core archive into the folder containing `Terraria.exe`.
2. Download the Auto Reapply Potions archive for your Terraria version.

   - On [Nexus Mods](https://www.nexusmods.com/terraria/mods/169?tab=files), select **Manual download** for the main file.
   - On the [latest GitHub release](https://github.com/EscarvalOokt/AutoReapplyPotions/releases/latest), download the packaged mod from **Assets**.

3. In your Terraria installation folder, open `TerrariaModder\mods\`. Create the `mods` folder if needed.
4. Open the mod archive and copy its `auto-reapply-potions` folder into `mods`. When updating, replace the existing `AutoReapplyPotions.dll` and `manifest.json` with the copies from the new archive.
5. Run `TerrariaInjector.exe` from the folder containing `Terraria.exe`, or launch the modded game from TerrariaModder Vault.
6. Enter a world and press **Alt + 2** to open the Filter Editor.

After installation, the mod files should be in the following locations.

```text
Terraria\
├── Terraria.exe
├── TerrariaInjector.exe
└── TerrariaModder\
    └── mods\
        └── auto-reapply-potions\
            ├── AutoReapplyPotions.dll
            └── manifest.json
```

## Compatibility

### Existing characters and worlds

You can use Auto Reapply Potions with an existing character and world.

### Removing the mod

Close Terraria and remove the `auto-reapply-potions` folder from `TerrariaModder\mods\`. You can continue playing with the same character and world without save cleanup.

### Multiplayer

Automatic reapplication runs for the local player. Multiplayer synchronization has not been separately verified.

### Saved progress

The mod tracks no separate progression. Each automatic reapplication consumes one item from the character's existing inventory or Void Bag stack. TerrariaModder saves the mod settings, including the item filter.

### Other mods

Auto Reapply Potions supports items that use Terraria's standard consumable buff behavior. Items that apply buffs through custom systems or scripted effects may not be recognized.

## Technical documentation

- [Development guide](docs/development.md)
- [Architecture](docs/architecture.md)
- [Testing policy](docs/testing.md)