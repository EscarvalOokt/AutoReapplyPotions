# Auto Reapply Potions development guide

## Prerequisites

- A .NET SDK and the .NET Framework 4.8 development tools for building the `net48` solution on Windows.
- A local installation of [Terraria and TerrariaModder Core](../README.md#requirements). The project references `Terraria.exe`, `TerrariaModder.Core.dll`, and the `0Harmony.dll` distributed with Core.
- The XNA Framework assemblies `Microsoft.Xna.Framework.dll` and `Microsoft.Xna.Framework.Game.dll`.
- A [TerrariaWindowKit](https://github.com/EscarvalOokt/TerrariaWindowKit) v1.1.0 checkout in a folder named `TerrariaWindowKit` alongside `AutoReapplyPotions`.

The project imports `../TerrariaWindowKit/src/TerrariaWindowKit.Source.projitems` relative to the AutoReapplyPotions repository root. It compiles the toolkit directly into `AutoReapplyPotions.dll`.

## Local configuration

Copy [AutoReapplyPotions.Local.props.example](AutoReapplyPotions.Local.props.example) to `src/AutoReapplyPotions.Local.props` and set the paths for your installation. Git ignores the local file.

| Property | Purpose |
| --- | --- |
| `TerrariaDirectory` | Path to the folder containing `Terraria.exe` and `TerrariaModder`. |
| `XnaFrameworkDll` | Path to `Microsoft.Xna.Framework.dll` for the WindowKit Core integration. |
| `XnaFrameworkGameDll` | Path to `Microsoft.Xna.Framework.Game.dll` for compiling against Terraria. |
| `DeployToGame` | Set to `true` to copy the mod into the game after a successful build. Defaults to `false`. |
| `TerrariaModDirectory` | Deployment destination. Defaults to `TerrariaModder/mods/auto-reapply-potions` under `TerrariaDirectory`. |

## Build

From the repository root, build the solution in Release configuration with deployment disabled.

```powershell
dotnet build src/AutoReapplyPotions.sln -c Release -p:DeployToGame=false
```

To deploy to your local game, close Terraria and run the same command with `-p:DeployToGame=true`. After a successful build, the project copies `AutoReapplyPotions.dll` and `mod/manifest.json` to `TerrariaModDirectory`.

## Tests

From the repository root, run the NUnit test project. This command also builds the mod with deployment disabled, even if deployment is enabled in your local configuration.

```powershell
dotnet test tests/AutoReapplyPotions.Tests/AutoReapplyPotions.Tests.csproj -c Release -p:DeployToGame=false
```

The [testing policy](testing.md) describes what the NUnit suite covers and what to check in game.