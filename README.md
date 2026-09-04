# Auto Reapply Potions

*Keep your buffs active without constantly reusing consumables.*

Auto Reapply Potions is a quality-of-life mod for Terraria that automatically reapplies supported consumable buffs when they expire.

When a supported timed buff ends, the mod looks for a matching consumable item in the local player's inventory, reapplies the buff, and consumes one item from the matching stack.

It supports common buff sources such as potions, food, and weapon flasks, and includes configurable whitelist and blacklist filtering for controlling which items may be used automatically.

## Features

- Automatically reapply supported consumable buffs after their current effect fully expires.
- Use matching items directly from the player's inventory.
- Support common buff sources including potions, food, and weapon flasks.
- Choose between Disabled, Blacklist, and Whitelist filtering modes.
- Manage filtered items through an in-game Filter Editor.
- Search the supported-item catalog by name, item ID, or category.
- Filter catalog entries by Potions, Food, Flasks, and other available categories.
- Browse currently selected items separately from the full catalog.
- Select or clear multiple visible items at once.
- Preview Terraria items directly in the Filter Editor.
- Toggle automatic reapplication with a configurable keybind.
- Enable optional debug tools for testing buff expiration.

## How it works

Auto Reapply Potions tracks active timed buffs on the local player.

When a supported buff disappears:

1. The mod checks whether a matching supported consumable exists in the player's inventory.
2. The current item filter is applied.
3. If an allowed item is found, the buff is reapplied using that item's buff duration.
4. One item is consumed from the matching inventory stack.

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

Only selected items may be used automatically.

The Filter Editor includes:

- A searchable supported-item catalog.
- Category tabs for available item groups.
- A separate list of currently selected items.
- Bulk selection and clearing actions.
- In-game item previews.

Filter settings are saved through the TerrariaModder configuration system.

## Controls

Default keybinds:

- **Alt + 1** — Toggle Auto Reapply Potions.
- **Alt + 2** — Open Filter UI.
- **Alt + 3** — Reduce all active timed buffs to 5 seconds when Debug Mode is enabled.
- **Alt + 4** — Reduce one random active timed buff to 5 seconds when Debug Mode is enabled.

Keybinds can be configured through TerrariaModder.

## Requirements

- Terraria 1.4.5.8
- TerrariaModder 0.4.0 or newer

A .NET Framework 4.8 development environment is additionally required when building the project from source.

## Compatibility

Auto Reapply Potions is designed primarily around standard Terraria consumable buff items.

The mod can be added to or removed from an existing character or world without save cleanup. It does not add persistent world objects, custom items, or other save-dependent content.

Consumables added or heavily modified by other mods are not guaranteed to work. Items that use custom buff systems, scripted effects, or non-standard consumable behavior may not be recognized correctly.

Automatically used items are consumed from their existing inventory stacks. Disabling or removing the mod does not restore items that were already consumed.

## Installation

1. Install and configure TerrariaModder.
2. Launch Terraria through `TerrariaInjector.exe` at least once so TerrariaModder can create its `mods` directory.
3. Copy the `auto-reapply-potions` folder into:

```text
Terraria/TerrariaModder/mods/
````

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

The current automated suite includes infrastructure validation for the test pipeline. Behavior that depends on the complete Terraria, TerrariaModder, XNA, or Harmony runtime is validated separately in game.

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