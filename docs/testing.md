# Auto Reapply Potions testing policy

This document defines the automated testing and runtime acceptance boundaries for Auto Reapply Potions.

## Testing principle

Automated tests should protect meaningful project-owned contracts and realistic regressions. Test count and broad method-level coverage are not goals by themselves.

## Automated test boundary

The NUnit-based `AutoReapplyPotions.Tests` project is reserved for deterministic behavior owned by Auto Reapply Potions.

The initial smoke test validates that the test project is discoverable and executable. It is infrastructure validation, not product behavior coverage.

Good candidates for deterministic tests include project-owned parsing, serialization, filtering, ordering, and rule behavior when they can be exercised without reproducing Terraria runtime semantics.

## Runtime acceptance boundary

Behavior that depends on the complete Terraria, TerrariaModder, XNA, or Harmony runtime should be validated separately in game rather than reproduced through fragile simulations.

This includes, in particular:

- whether the mod is discovered and loaded by the current TerrariaModder version;
- lifecycle and config integration;
- Harmony patch application to the current Terraria build;
- actual buff expiration/reapplication behavior;
- inventory mutation and item consumption in the live game;
- TerrariaModder.Core in-game UI behavior and input handling.

## Adding tests

Add tests together with project-owned behavior when they protect a meaningful contract. Keep runtime acceptance scenarios separate when the relevant semantics are owned by Terraria or TerrariaModder rather than this project.