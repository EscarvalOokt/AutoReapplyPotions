# Auto Reapply Potions

![Auto Reapply Potions preview](assets/AutoReapplyPotions_Preview.png)

**Keep your buffs active without constantly reusing consumables.**

[Download on Nexus Mods](https://www.nexusmods.com/terraria/mods/169)

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

When a supported buff disappears:

1. The mod searches the player's inventory for the first supported item allowed by the current filter.
2. If no allowed inventory item matches, it checks an accessible Void Bag for an item allowed by the same filter.
3. The buff is reapplied using the selected item's buff duration, and one item is consumed from that inventory or Void Bag stack.

Buffs are only reapplied after the previous effect has fully expired. Existing remaining duration is not overwritten.

The mod does not automatically reapply:

- Debuffs.
- Potion Sickness.
- Mana Sickness.
- Permanent buffs whose remaining duration does not decrease.
- Items that do not use Terraria's supported consumable buff-item behavior.

## Item filtering

The built-in Filter Editor lets you control which supported items may be consumed automatically.

Open the editor with the **Open Filter UI** keybind, which defaults to **Alt + 2**.

### Disabled

The filter is ignored and all supported items may be used.

### Blacklist

Selected items are excluded from automatic use.

### Whitelist

Only selected items may be used automatically. An empty whitelist prevents automatic reapplication.

The Filter Editor includes:

- Search by item name, ID, or category.
- Catalog and Selected views, with available Potions, Food, Flasks, and Other categories.
- **Select results**, **Clear results**, and **Clear all** actions. Select results and Clear results affect all matching items, including those outside the visible scroll area.
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

## Requirements

- Terraria 1.4.5.8
- TerrariaModder Core 0.4.1 or newer

## Compatibility

Auto Reapply Potions is designed primarily around standard Terraria consumable buff items.

The mod can be added to or removed from an existing character or world without save cleanup. It does not add persistent world objects, custom items, or other save-dependent content.

Consumables added or heavily modified by other mods are not guaranteed to work. Items that use custom buff systems, scripted effects, or non-standard consumable behavior may not be recognized correctly.

A Void Bag must be available and unlocked in the player's inventory for its stored items to be used. The mod does not access it during pending inventory operations or queue a later reapplication if it is unavailable when a buff expires.

Multiplayer synchronization has not been separately validated.

Automatically used items are consumed from their existing inventory or Void Bag stacks.

## Installation

1. Install and configure [TerrariaModder](https://www.nexusmods.com/terraria/mods/135) using the instructions on its download page.
2. Launch Terraria through `TerrariaInjector.exe` at least once so TerrariaModder can create its `mods` directory.
3. Copy the `auto-reapply-potions` folder into:

```text
Terraria/TerrariaModder/mods/
```

The installed mod directory should contain:

```text
auto-reapply-potions/
├── AutoReapplyPotions.dll
└── manifest.json
```

Launch the game again through `TerrariaInjector.exe`.

The mod is enabled by default.

Use **F6 → Auto Reapply Potions** to access its TerrariaModder settings, or press **Alt + 2** in game to open the Filter Editor.

## Development

Building from source requires a .NET Framework 4.8 development environment.

A [TerrariaWindowKit](https://github.com/EscarvalOokt/TerrariaWindowKit) v1.1.0 checkout is also required alongside the AutoReapplyPotions checkout. The production project imports `../TerrariaWindowKit/src/TerrariaWindowKit.Source.projitems` relative to the AutoReapplyPotions repository root, compiling the toolkit from source rather than deploying a separate toolkit DLL.

### Local configuration

Copy:

```text
docs/AutoReapplyPotions.Local.props.example
```

to:

```text
src/AutoReapplyPotions.Local.props
```

Then configure:

* `TerrariaDirectory` — root directory of the local Terraria installation.
* `XnaFrameworkDll` — local `Microsoft.Xna.Framework.dll` reference used by the WindowKit Core integration.
* `XnaFrameworkGameDll` — local `Microsoft.Xna.Framework.Game.dll` reference used to compile against Terraria.
* `DeployToGame` — set to `true` to enable post-build deployment.
* `TerrariaModDirectory` — target `TerrariaModder/mods/auto-reapply-potions` directory.

The local props file contains machine-specific paths and is intentionally excluded from Git.

### Build

Build the production solution in Release configuration:

```bash
dotnet build src/AutoReapplyPotions.sln -c Release
```

When `DeployToGame` is enabled, the production project copies `AutoReapplyPotions.dll` and `mod/manifest.json` to the configured TerrariaModder mod directory after a successful build.

### Tests

Run the NUnit test project with:

```bash
dotnet test tests/AutoReapplyPotions.Tests/AutoReapplyPotions.Tests.csproj -c Release
```

The automated suite covers test-pipeline discovery and the deterministic Search click-away input policy. Behavior that depends on the complete Terraria, TerrariaModder, XNA, or Harmony runtime requires separate in-game validation. Automated tests do not establish runtime compatibility.

## Repository structure

```text
docs/    Project documentation and development configuration examples
mod/     TerrariaModder package metadata
src/     Production source and solution
tests/   Automated tests
```

## Technical documentation

* [Architecture](docs/architecture.md)
* [Testing policy](docs/testing.md)

## Links

* [Nexus Mods](https://www.nexusmods.com/terraria/mods/169)
* [GitHub Releases](https://github.com/EscarvalOokt/AutoReapplyPotions/releases)