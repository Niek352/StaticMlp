# Attack‑to‑Gather Base‑Building Roguelite – Implementation Roadmap

This roadmap translates the combined game design document (GDD) into a sequence of implementation stages. Each section summarises what needs to be built, proposes an order of work and notes what is already completed according to the current StaticMlp repository and UX documentation. Where appropriate, citations from the GDD highlight the intended behaviour.

## Foundation and Project Setup

| Task | Description | Status |
| --- | --- | --- |
| **Design documentation & core loop definition** | Finalise the attack‑to‑gather loop: players attack the world to harvest resources, build and upgrade a settlement, recruit companions, assemble a limited item build, defeat biome bosses and unlock the next biome. | **Completed** – the combined GDD defines the loop and core principles. |
| **ECS architecture & networking layer** | Set up a server‑authoritative Entity Component System (ECS) architecture with client‑only presentation. This underpins deterministic open‑world placement, resource overlays and future NPC/settlement systems. | **Completed** – the repository contains foundational assemblies (StaticMlp.Features.\*), networking and ECS infrastructure. |
| **Open‑world resource placement** | Implement deterministic generation of resource nodes (trees, ores, spore pods, chests) and an overlay system to track their mutable state. Avoid spawning every node as a network entity; instead index placements and send overlay updates to clients[\[1\]](https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/Assets/Scripts/StaticMlp/Features/OpenWorldResources/AGENTS.md#L5-L48). | **Completed** – the OpenWorldResources feature registers placement indexes and overlay stores and provides client proxies[\[2\]](https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/Assets/Scripts/StaticMlp/Features/OpenWorldResources/AGENTS.md#L26-L58). |
| **NPC identity foundation** | Provide basic NPC contracts: NpcClass (Companion, Specialist), NpcRoleFlags (Gatherer, Hauler, Processor, Guard, Builder, Researcher), acquisition path identifiers and a replicated NpcIdentity component[\[3\]](https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/Assets/Scripts/StaticMlp/Features/Npc/AGENTS.md#L11-L34). | **Completed** – implemented in StaticMlp.Features.Npc with NpcGameplayFeature as the entry point[\[3\]](https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/Assets/Scripts/StaticMlp/Features/Npc/AGENTS.md#L11-L34). |
| **Settlement UX vertical slice** | Build the first‑pass settlement UI: interaction prompts when focusing on buildings, a building management panel for construction/deposit/operation, a building menu listing available structures and a placement preview with confirmation/cancel input[\[4\]](https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/docs/ux/settlement-stage1.md#L22-L46)[\[5\]](https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/docs/ux/settlement-stage1.md#L93-L105). This enables players to place camp core, stockpile, bedroll shelter, lumber camp, stone mine and workbench. | **Completed (Unity verification pending)** – code paths exist for prompts, panels and menus; Unity UX still needs manual testing[\[6\]](https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/docs/ux/settlement-stage1.md#L22-L48)[\[5\]](https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/docs/ux/settlement-stage1.md#L93-L105). |

## Phase 1 – Core Combat and Gathering

1.  **Movement & targeting**: Implement third‑person or isometric movement with active aiming. The player should be able to focus damage on enemies or resource nodes without exiting combat.
2.  **Attack‑to‑gather mechanic**: Treat resource nodes as enemies with HP and armour, letting every strike harvest materials and deal damage. Include hazards when nodes are destroyed (falling branches, explosions, poison clouds etc.).
3.  **Magnetic pickup & inventory**: Implement a small raw‑material inventory (~20 slots) and a magnetic pickup radius so dropped resources fly toward the player. This encourages returning to base frequently.
4.  **Enemy AI basics**: Develop simple enemy behaviours (swarm creatures, elites) and integrate them with resource nodes.

_Status:_ **Completed (Unity verification pending)** - the Phase 1 source path now supports active combat targeting, attack-to-gather resource overlay damage, carried raw inventory, magnetic pickups, depletion hazards/feedback, and tuned swarm/elite enemy pressure through existing AI and Combat systems. Unity compile and host/client play verification are still required before this phase is fully closed.

## Phase 2 – Resource Flow and Economy

1.  **Resource node types**: Define data and prefabs for trees, ores, spore pods and chests with HP, armour and weaknesses. Tag weapons/passives for optimal harvesting (axes for trees, blunt/lightning for ores, fire for pods, rune/projectile for chests).
2.  **Three‑tier conversion chain**: Build crafting stations that convert raw materials into refined goods and products. Examples include sawmills for planks, smelters for ingots and cooking stations for food.
3.  **Resource limits and sinks**: Enforce the small raw‑inventory, require periodic off‑loading and implement sinks such as construction, station fuel, item crafting, NPC upkeep, station upgrades and raid repairs.
4.  **Building unlocks**: Ensure each new building expands item pools or automation. For example, the Resin Forge unlocked by rescuing the Carpenter enables resin processing.
5.  **Trading & barter**: Design merchants and factions that exchange items for resources rather than currency.

_Status:_ Resource node generation, harvest profiles, depletion hazards and client visuals now cover trees, ores, spore pods and chests. Building interaction UX, resource overlays and carried-inventory deposit into settlement stockpiles exist. Settlement now has generalized production station contracts, current Workbench recipes/state, server-authoritative recipe processing with fuel requirements and explicit blocked-state reporting, and output claiming into shared storage. Additional station buildings, bartering and broader economy sinks remain to be implemented.

_Sink boundaries:_ NPC upkeep (`Food`, `Medicine` `Upkeep` flags) is deferred to Phase 3 because the NPC task/comfort system is not yet built. Raid repairs (`Stone`, `SimpleParts`, `Ingots` `Repair` flags) are deferred to Phase 8 because base-defence encounters do not yet exist. Phase 2 active sinks are limited to construction, station fuel, and recipe crafting, all owned by existing Settlement systems.

_Ownership note:_ `Settlement.Contracts owns resource ids` and storage contracts; `OpenWorldResources` owns node placement, overlay state and harvest metadata; `ResourcesInventoryMinimal` owns carried raw inventory. Economy cross-feature mutations must go through typed events or requests.

## Phase 3 – Settlement & NPC Systems

1.  **Recruitment & acquisition**: Implement varied ways to recruit NPCs – rescue missions, duels/trials, destroying cursed objects, comfort‑based lures, production‑based spawning, persuasion, artificial companions and boss rewards.
2.  **Housing & comfort**: Add beds, kitchens and comfort levels; NPCs will only join or stay if their needs are met.
3.  **NPC roles & traits**: Provide base worker roles (operate stations, refine materials, gather offline) and combat companion roles. Tie each NPC to progression unlocks (e.g., Forager unlocks Herbal Hut, Carpenter unlocks Resin Forge).
4.  **NPC economy & task system**: Build a RimWorld‑style priority board for tasks such as gathering, hauling, processing, guarding, building, research, repair and feeding. Implement leaks (food, fuel, durability) and systems for task generation, scoring, assignment and execution (see the Codex NPC economy prototype for details). Ensure server‑authoritative updates and ECS compatibility.
5.  **NPC progression & traits**: Implement innate and earned traits affecting work speed, combat performance and preferences.
6.  **Raids & base defence**: Increase settlement threat when resources are over‑harvested and trigger raids. Provide defensive structures and assign NPC guards.

_Status:_ Only the basic NPC identity and roles are implemented. Recruitment, comfort, task systems and raids have not yet been built.

## Phase 4 – Equipment and Build System

1.  **Limited build slots**: Create systems for equipping a weapon, several passives, one relic, trinkets and one or two companion bonds.
2.  **Item tiers & drops**: Implement loot tables with common through legendary tiers and scale drop rates by biome difficulty.
3.  **Synergy tags**: Design tag‑based interactions (e.g., Fire + Harvest causes charcoal drops, Forest King synergy from Living Bark + Nature’s Wrath).
4.  **Discovery & opacity**: Ensure synergies are not obvious; rely on experimentation and NPC hints.

_Status:_ The repository does not yet implement item slots, tags or synergies. This phase remains unstarted.

## Phase 5 – Player Progression & Co‑op Roles

1.  **Vertical progression**: Implement upgrades to health, stamina, settlement level, item slots and station tiers.
2.  **Horizontal progression**: Unlock new item pools, NPC roles, weapon types and synergies per biome.
3.  **Co‑op roles**: Allow players to specialise into Harvester, Defender, Boss Killer, Swarm Clearer, Support and Scout builds. Synergistic effects should encourage cooperation (e.g., root + ignite for burning grove explosions).
4.  **Guidance systems**: Develop a biome board and base screens so players always know which resource to gather, which NPC to recruit, which building to construct and which boss to prepare for.

_Status:_ Progression systems and co‑op roles are not yet developed.

## Phase 6 – Combat Dynamics and Weapons

1.  **Weapon variety**: Implement a roster of weapons with different attack patterns (continuous firing, orbiting axes, chain lightning, area effects).
2.  **Passive procs & relics**: Add passives that trigger effects like splinters or poison clouds and relics that alter rules (e.g., Cursed Bell raises drop rates but increases threat). Balance them for synergy with resource nodes and enemies.
3.  **Targeting**: Allow players to prioritise enemies, elites, resource nodes or boss weak points.

_Status:_ Weapon mechanics and targeting logic have not yet been implemented in code.

## Phase 7 – Bosses & Biomes

1.  **First biome**: Create the Forest biome with the Root Stag guardian. Design multi‑phase fights where harvested trees heal players and later phases require destroying root anchors.
2.  **Subsequent biomes**: Add Storm Colossus (lightning armour), Thorn Titan (thorn walls), Storm Serpent (wind zones) and others. Each boss should test mastery of that biome’s mechanics.
3.  **Biome unlocks**: Defeating a boss grants unique loot, a Heart Core for upgrading the Core Heart and spawns NPCs with rare traits.
4.  **Environment hazards & resource variety**: Introduce biome‑specific enemies, hazards (toxic mist, heat waves, falling platforms) and resource nodes (spore pods, cursed contracts, rune chests).

_Status:_ No biome or boss gameplay is currently implemented.

## Phase 8 – Co‑operative Play & Base Defence

1.  **Multiplayer architecture**: Support 1–4 players sharing the same base and NPC roster. Ensure item drops are instanced per player but crafting and upgrades are shared.
2.  **Scaling & co‑op mechanics**: Scale enemy HP and spawn rates with player count. Add co‑op‑only mechanics such as Thorn Titan’s walls and Storm Serpent’s wind zones.
3.  **Base defence events**: Trigger raids when threat budgets are exceeded; require players to return to defend, build turrets/traps and assign NPC patrols.
4.  **Social dynamics**: Design systems that encourage cooperation and discourage selfish play (shared resources but personal builds, competition for NPC bonds etc.).

_Status:_ Multiplayer and base defence systems are not yet implemented.

## Phase 9 – Expansion and Long‑Term Vision

1.  **Additional biomes & NPC types**: After the first release, expand with new biomes, dozens of NPC types and hundreds of items.
2.  **Advanced automation & relationships**: Add complex production chains, advanced automation (e.g., conveyors, constructs), NPC relationships and narratives.
3.  **Dedicated server co‑op**: Move to a dedicated server architecture to support persistent worlds and large co‑op groups.

_Status:_ Long‑term features are aspirational and not in the current scope.

## Summary of Completed Work

- **Open‑world resource placement & overlay:** implemented in the StaticMlp.Features.OpenWorldResources feature, which registers placement indexes and overlay stores and spawns client‑only proxies[\[2\]](https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/Assets/Scripts/StaticMlp/Features/OpenWorldResources/AGENTS.md#L26-L58).
- **Settlement UI & building interactions:** the settlement vertical slice provides interaction prompts, a building management panel, building menu and placement preview; these code paths exist and only need Unity verification[\[6\]](https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/docs/ux/settlement-stage1.md#L22-L48)[\[5\]](https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/docs/ux/settlement-stage1.md#L93-L105).
- **NPC identity foundation:** basic NPC contracts and the NpcGameplayFeature are in place, defining classes, acquisition paths and role flags[\[3\]](https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/Assets/Scripts/StaticMlp/Features/Npc/AGENTS.md#L11-L34).
- **Phase 1 combat/gathering source path:** active targeting, static resource damage, pickup inventory, depletion hazards, and Combat Director enemy swarm/elite pressure are source-complete; Unity verification remains pending.

Remaining later-phase features described in the GDD still need development. The roadmap above outlines a suggested order for building them and highlights dependencies between systems.

[\[1\]](https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/Assets/Scripts/StaticMlp/Features/OpenWorldResources/AGENTS.md#L5-L48) [\[2\]](https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/Assets/Scripts/StaticMlp/Features/OpenWorldResources/AGENTS.md#L26-L58) AGENTS.md

https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/Assets/Scripts/StaticMlp/Features/OpenWorldResources/AGENTS.md

[\[3\]](https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/Assets/Scripts/StaticMlp/Features/Npc/AGENTS.md#L11-L34) AGENTS.md

https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/Assets/Scripts/StaticMlp/Features/Npc/AGENTS.md

[\[4\]](https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/docs/ux/settlement-stage1.md#L22-L46) [\[5\]](https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/docs/ux/settlement-stage1.md#L93-L105) [\[6\]](https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/docs/ux/settlement-stage1.md#L22-L48) settlement-stage1.md

https://github.com/Niek352/StaticMlp/blob/c06f6b2bcd7a8ef446bf0f0f6441eb67a8c6e28e/docs/ux/settlement-stage1.md
