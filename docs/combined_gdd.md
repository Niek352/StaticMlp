# Combined Game Design Document – Attack‑to‑Gather Base‑Building Roguelite

## Overview

This design document synthesizes the core ideas from three related proposals (``Project Anomaly``, ``BondBase`` and ``Wildforge Settlement``) into a single source of truth.  All three concepts share a unique loop: players **attack the world to gather resources**, build and automate a living **settlement**, recruit **companions** that function as both workers and fighters, and assemble a **limited item build** to defeat guardians and unlock new biomes.  The overarching vision is that every strike of the weapon has purpose – it simultaneously defeats enemies and harvests materials, feeding back into a progression spiral where the base enables new tools and those tools transform how the player fights and gathers.

The following chapters are organized around the four high‑level themes requested by the team – **Player**, **NPC**, **Economy** and **Combat** – with sub‑sections that explain what to build and how to build it.  Where appropriate, tables are used only for discrete data (e.g., slots or traits); longer explanations remain in prose as per guidelines.

---

## 1 Player

### 1.1 Movement & Targeting

The game uses a third‑person or isometric camera with active aiming.  The player is always in motion: you move through procedural environments while targeting specific enemies or resource nodes.  **Resource nodes** – trees, ore veins, spore pods, nests, ruins – are treated like enemies: they have hit points, armour types, weaknesses and sometimes counter‑attacks.  For example, in the Wildforge design a tree node has medium HP, drops wood and resin, and may lash out with falling branches; an ore vein has high armour, drops ore and crystal, and can unleash shockwaves when struck.  Because there is no separate harvesting tool, the player attacks these targets using the same arsenal used against monsters.  This “attack‑to‑gather” system blends combat and resource collection, removing downtime and making gathering inherently skill‑based.

Players can prioritise targets such as the nearest enemy, a marked resource, an elite monster or a boss weak point.  This targeting system allows you to focus damage on a resource you are farming without interrupting the fight.  Because resources spawn hazards – poison clouds from spore pods, explosions from ore veins – gathering becomes a mini tactical puzzle rather than a repetitive click.  Dropped resources use a **magnetic pickup**, flying toward the player within a small radius to keep screens readable.

### 1.2 Build & Equipment System

The player’s power comes from a **limited build** rather than a giant inventory.  Across all three documents, the consensus is that hard slot limits force meaningful choices and encourage emergent synergies.  A typical build includes:

| Slot             | Description | Examples |
|------------------|------------|---------|
| **Weapon**       | Determines your primary attack pattern.  One weapon is equipped at a time, but advanced players may unlock a second or third slot. | Bouncing Blade (projectiles ricochet); Axe Orbit (rotating axe good at clearing trees); Spark Coil (chain lightning on metal targets). |
| **Passive**      | Provides continuous stat bonuses or proc effects.  Players start with four slots and can expand to eight. | Splinter Heart (tree destruction releases damaging splinters); Glass Carapace (+40 % damage but double damage taken); Miner’s Knuckle (bonus stagger damage on armoured targets). |
| **Relic**        | A single powerful item that dramatically changes the rules.  Relics often come from bosses or rare events. | Blood Pact (+60 % damage but prevents natural HP regen); Heart of the Grove (trees become friendly root turrets after breaking). |
| **Charm/Trinket**| Small modifiers that add niche effects.  Players usually have one slot early on, expanding to two later. | Lucky Nail (chance for extra materials), Red Thread (strengthens a specific companion bond). |
| **Companion Bond** | Chooses 1–2 recruited NPCs whose bond grants a passive or active combat effect even when they stay at the base. | Forager bond occasionally spawns healing seeds; Ash Hunter bond applies bleed and critical bonuses. |

Items are divided into quality tiers (common, uncommon, rare, epic and legendary) with drop rates scaling by biome difficulty.  Synergies occur when specific items or tags are combined.  For instance, equipping Living Bark armour and the “Nature’s Wrath” weapon activates the **Forest King** synergy, granting regeneration and bonus damage when standing near trees.  Tag‑based systems also create emergent interactions: combining fire and harvest tags might cause burning trees to drop charcoal instead of wood, while crit and bleed tags can create blood‑burst explosions.  The discovery of these synergies is intentionally opaque; players are expected to experiment and listen to NPC hints.

### 1.3 Player Progression & Roles

Progression is both vertical (improving health, stamina, settlement level, item slots and station tiers) and horizontal (unlocking new item pools, NPC roles, weapon types and synergies).  Each biome acts as a contained arc: you arrive with basic gear, gather resources through combat, return to build a home, recruit a few companions, craft your first items, then challenge the biome’s guardian boss.  Victory unlocks the next biome, new NPC types and resources, higher‑tier items and additional threats.  This spiral repeats, with later biomes demanding specialized builds and deeper base automation.

In co‑op, players can take on complementary roles.  The design encourages builds such as **Harvester** (focus on object damage and gathering), **Defender** (tank with auras and base defence), **Boss Killer** (single‑target burst), **Swarm Clear** (area‑of‑effect control), **Support** (healing, buffs, companion synergy) and **Scout** (mobility and event discovery).  These roles interact via shared debuffs and combo effects; for example, one player roots enemies while another ignites them to create a burning grove explosion.

### 1.4 User Experience & Guidance

User interfaces must guide the player without overwhelming them.  Wildforge proposes a **Biome Board** – a quest board that lists objectives for the current biome (e.g., build a house, recruit a Forager, craft the first forest item, defeat the mini‑boss, build a Resin Forge, prepare the Root Stag lure) so players always understand what they should be doing next.  On the base, screens are provided for build/equipment, base management, NPC roster, production queues, biome progress and boss preparation.  Players should always know which resource to gather, which NPC to recruit next, which building unlocks the next item pool, and which boss they are preparing to fight.

---

## 2 NPC

NPC companions are the heart of the settlement.  They are not passive villagers but multi‑role entities that function as **workers**, **combat allies** and **progression keys**.  Each NPC has attributes (strength, dexterity, constitution and wisdom), innate traits and a unique acquisition story.  They require housing and food, can be assigned to workstations or accompany players on expeditions, and often unlock new buildings or item pools.

### 2.1 Recruitment & Acquisition

Acquiring companions should feel like completing a small quest rather than simply capturing an entity.  The documents outline several recruitment methods:

1. **Rescue** – NPCs are found imprisoned, injured or besieged.  Players must defeat surrounding enemies and then build appropriate housing to convince them to join.  For example, rescuing a wounded Herbalist in a poisoned forest requires destroying three poison pods, bringing clean water and constructing a Herbal Hut.  BondBase adds that rescued NPCs start with a positive trait such as **Grateful** (+20 % work speed for three days) or **Warrior** (+10 Strength).
2. **Duel / Trial** – Some NPCs test the player’s skill before joining.  The Ash Hunter ambushes players; they must defeat him without using fire items.  Upon earning his respect he offers a contract and unlocks the Hunter Lodge.
3. **Contract Object** – NPCs bound to cursed objects can only be freed by destroying the object while surviving waves of guardians.  The Bound Miner stands next to a runic stone; when the stone is broken and golem waves are defeated, he becomes available and unlocks the Quarry Workbench.
4. **Lure / Comfort** – Certain NPCs will only join a comfortable settlement.  For instance, a Cook appears near the campfire at night but will only stay if there is a Kitchen, sufficient beds, food reserves and a Comfort Level of 2.
5. **Tame Through Work** – Players can attract specialist NPCs by building the right production chain.  Constructing a Silk Garden from rare resources spawns the Moth Weaver after a few days; she opens access to dodge/evasion items and cloth armour.
6. **Convince** – In BondBase, some NPCs wander neutral camps and can be persuaded through dialogue choices that match their personalities.  Failure can be mitigated with gifts.
7. **Craft** – At higher settlement tiers, players unlock a **Soul Forge** to build artificial companions (Constructs) such as Worker, Guardian or Harvester constructs.  These have predictable stats and fill workforce gaps.
8. **Boss Reward** – Defeating a biome guardian spawns two to three NPCs unique to that biome, some with rare traits like Fireheart (immune to fire, +25 % fire damage) or Gemvein (bonus gem drops), providing strong incentives to progress.

This variety keeps recruitment fresh and ties it back to core systems: combat proficiency, base building, comfort management and production planning.

### 2.2 Roles & Traits

Once recruited, an NPC should always serve multiple purposes.  The Wildforge document defines three simultaneous roles:

1. **Base Worker** – NPCs operate production buildings, refine materials, gather resources off‑line and speed up crafting.  Each worker needs a bed and a steady food supply, and their job assignment and priority can be managed via a RimWorld‑style interface.
2. **Combat Companion** – Selected NPCs occupy the player’s **Companion Bond** slot, granting passive or active abilities during expeditions: the Forager occasionally drops a healing seed; the Ash Hunter marks elite enemies with bleed; the Spark Tinker fires chain‑lightning gadgets.  Only a limited number of companions can be bonded at once, making the choice meaningful.
3. **Progression Key** – Many NPCs unlock new buildings, item pools or biome mechanics.  The Forager opens the Herbal Hut and healing recipes; the Woodbound Carpenter reduces wood construction costs and enables the Resin Forge; the Bound Miner unlocks the Quarry; the Moth Weaver grants access to cloth gear.

NPCs have **innate traits** that modify their behavior.  BondBase categorizes traits as positive (Hard Worker: +25 % work speed; Battle Trained: +20 % combat damage), neutral (Moody: fluctuating work speed; Perfectionist: slower crafting but increased output) or negative (Lazy: −30 % work speed; Fragile: −30 % max HP; Picky Eater: requires premium food).  NPCs gain new traits by performing jobs or fighting; for example, a Carpenter crafting furniture for ten in‑game days unlocks **Master Carpenter** (+15 % crafting speed, unlocks fine furniture).

### 2.3 Behaviour in the Settlement

The base functions as a living settlement with several parameters: **Settlement Level**, **Comfort**, **Capacity** (NPC limit), **Workforce** (available labour hours), **Food Stability**, **Defense**, **Production Power**, **Biome Attunement** and **Threat**.  Upgrading the **Core Heart** expands the build radius and raises the maximum number of active NPCs.  Each NPC requires a bed and consumes one unit of food per day; housing and food quality directly affect comfort and therefore recruitment of picky NPCs.  Production buildings (Lumber Yard, Quarry, Forge, Alchemy Table, Loom, Relic Press, Tinker Station) convert raw materials into refined goods and items.  NPC‑specific buildings (Herbal Hut, Hunter Lodge, Miner Dorm, Workshop Cabin, Shrine Room, Training Yard) enable specialised recipes and companion bonds.

Job assignment is handled via a drag‑and‑drop priority board reminiscent of RimWorld.  NPCs can be set to craft, farm, haul, patrol or explore; idle labour is wasted, so players must balance workforce between base upkeep and expeditions.  There is also a **Reserve Pool** for additional NPCs who are fed but not assigned to jobs, allowing players to recruit ahead of upgrades or keep replacements ready.

### 2.4 Enemy Behaviour & Raids

Over‑exploiting the environment increases **Threat**.  Project Anomaly notes that a **Threat Budget** system reacts to player activity: intensive resource gathering provokes raids on the settlement that must be defended.  In Wildforge, base events include raids, NPC sickness, food shortages, accidents, companion conflicts, rare visitors, biome corruption and boss retaliation.  The aim is to ensure the settlement is dynamic and occasionally under pressure without feeling punitive.  Players must invest in defensive structures, assign NPCs to patrols and occasionally return from expeditions to repel attacks.  Boss fights also interact with the base: defeating the Root Stag unlocks a new raid type (beast packs).

---

## 3 Economy

### 3.1 Resource Flow & Scarcity

All resources are obtained by attacking the environment; there is no traditional gathering action.  Nodes have HP and armour values that determine how long they take to harvest and which attack types are most efficient.  For example, trees are weak to axes or orbiting blades, ores to blunt and lightning damage, spore pods to fire, and ancient chests to rune or projectile weapons.  When destroyed, nodes drop raw materials and sometimes spawn hazards (poison clouds, shard explosions) that must be dodged.

The economy follows a **three‑tier conversion chain**: **Raw → Refined → Product**.  Raw resources (wood, stone, iron ore, berries) come directly from the world.  Refined materials (planks, bricks, ingots, thread) are created at crafting stations.  Products (weapons, armour, furniture, food) are final goods used for equipment, base upgrades and NPC maintenance.  Special materials such as **Heart Cores**, **Boss Scales** and rare gems drop from guardians and rare events and are used exclusively for settlement upgrades and legendary crafting.  There is **no currency**; barter with merchants or factions uses resources as payment.  Even basic resources remain valuable late game because they are required in bulk for high‑tier buildings or as fuel for furnaces.

To prevent hoarding and encourage cyclical play, the player’s raw‑material inventory is deliberately small – roughly twenty slots – and dropped items automatically fly toward the player within a magnetic radius【639889298044000†L107-L111】.  This encourages frequent returns to base and creates tension between staying in the field and offloading.  Scarcity is further enforced through **resource sinks**: building structures, crafting items, housing and feeding NPCs, upgrading stations, base defences, preparing for bosses, rerolling item choices and repairing raid damage【163748187169652†L256-L267】.  As a rule, the economy should never devolve into busywork; resources should be easy to understand and flows should stay short.  Designers are advised to avoid excessive intermediate materials and to keep production chains clear【163748187169652†L256-L267】.

### 3.2 Building & Item Economy

Buildings are more than storage containers; they determine the **item pools** available for the current biome.  Each new structure must answer the question “How does this expand the player’s build or base automation?”.  In Wildforge’s loop, rescuing or luring a specific NPC unlocks a production building (e.g., the Forager opens the Herbal Hut, the Carpenter unlocks the Resin Forge), which in turn enables crafting of new weapons, passives or relics.  These buildings should retain long‑term value: rather than producing a single unique item and becoming obsolete, they can generate multiple items across tiers, provide ongoing passive benefits or be upgraded to output refined materials faster.  Higher biome tiers add **biome‑specific buildings** (Spore Garden, Bark Shrine, Resin Forge, etc.) that introduce new tags and synergies.

Items themselves are part of the economy.  Quality tiers and drop rates control availability: early biomes mostly drop common and uncommon items, while late biomes reward rare, epic and legendary items.  Because players have limited slots, each item crafted or looted carries opportunity cost, creating an implicit economy of build decisions.  Roguelite mechanics – randomised drops, secret synergies and risk‑reward trade‑offs – ensure that the pursuit of better gear remains exciting.

### 3.3 Resource Nodes as Enemies

To merge combat and economy, resource nodes behave like enemies.  Trees, ore veins, spore pods and ancient chests have HP and weak points; they drop materials when destroyed and may counter‑attack with falling branches, shard explosions, poison clouds or summoned guardians.  Players can optimize harvesting by equipping weapons and passives tagged for specific node types: axes or orbiting blades for trees, blunt or lightning damage for ores, fire attacks for spore pods, rune weapons for ancient chests.  Tags also feed into synergies – for example, combining **Fire** and **Harvest** tags can cause burning trees to drop charcoal instead of wood.

### 3.4 Trading & Barter

Instead of a gold‑based economy, trade is conducted through barter.  Merchants might offer a rare weapon in exchange for planks and ingots.  Neutral factions may demand refined goods for unique items.  Without currency, every resource retains value throughout progression and inflation is avoided.

---

## 4 Combat

### 4.1 Attack‑to‑Gather Mechanic

At the heart of combat is the **attack‑to‑gather** mechanic: every strike simultaneously deals damage to enemies and harvests nearby resource nodes【526421678713442†L7-L25】.  Trees, rocks, ore veins and crystals have hit points like monsters.  This design removes the dichotomy between combat and gathering time; players no longer switch tools or enter separate “harvest mode” – they simply fight.  Because resource nodes drop materials mid‑fight, players are constantly making tactical decisions about whether to stay and harvest under pressure or retreat to safety.

Attacks can be manual or semi‑automatic.  Some weapons fire continuously in a chosen direction (e.g., Bouncing Blade, Thorn Whip), while others trigger area effects or chain reactions (Spark Coil, Rune Saw).  Passive items add procs such as splinters flying out of felled trees or poison clouds spawning from kills.  Relics introduce rule‑changing modifiers: the **Cursed Bell** increases drop rates but raises base threat, while the **Twin Moon Idol** doubles every third proc at the cost of longer weapon cooldowns.

### 4.2 Combat Dynamics & Roles

Combat draws inspiration from reverse bullet‑hell titles like Vampire Survivors: large enemy waves press the player constantly, but here they mix with resource nodes.  Movement and positioning are key.  Players can specialise into roles: **Harvester** builds maximise object damage and pickup radius; **Defenders** equip armour and auras to protect the team; **Boss Killers** focus on single‑target burst and debuff exploitation; **Swarm Clearers** prioritise area damage and crowd control; **Supports** provide healing, buffs and companion synergy; **Scouts** emphasise speed and lure events.  Synergistic builds encourage cooperation: one player roots enemies while another ignites them; if a Forager bond is active, the resulting explosion drops healing seeds.

Targeting allows players to prioritise specific enemies (e.g., elites, spawners) or resource nodes.  This ensures that the combat system remains responsive to the player’s goals, whether they are clearing a wave or farming materials.

### 4.3 Bosses & Biomes

Each biome culminates in a **Guardian Boss** that serves as an exam for that biome’s mechanics.  Bosses are multi‑phase encounters that leverage environmental hazards and resource interactions.  For example, the **Root Stag** charges and summons roots; standing trees in the arena can be harvested during the fight for healing and resources; later phases require destroying root anchors.  The **Storm Colossus** uses armour and lightning, demanding blunt or stagger damage and electrical builds.  Defeating a boss always grants unique loot from that biome’s item pool, a **Heart Core** for upgrading the Core Heart, and spawns NPCs with rare traits.  Bosses also unlock access to the next biome and its corresponding buildings, resources and threats.

### 4.4 Enemy & Resource Variety

Enemies range from simple swarm creatures to elite monsters with armour, shields or ranged attacks.  Each biome introduces new enemy archetypes and environmental hazards (toxic mist, darkness, heat waves, falling off sky islands).  Resource nodes likewise vary: spore pods explode into poison clouds; cursed contracts summon guardians; ancient chests have rune shields and summon protectors when broken.  As players progress, they must adapt their build to each biome’s unique challenges.

### 4.5 Co‑op Combat & Base Defence

The game is designed for 1–4 players.  In co‑op, all players share the same base, Core Heart and NPC roster.  Item drops are instanced per player, but crafting recipes and base upgrades are shared.  Enemy HP and spawn rates scale with player count, but resource drops do not, encouraging cooperation when harvesting high‑HP nodes.  Bosses gain additional mechanics in co‑op: the Thorn Titan spawns thorn walls that require coordinated roles, and the Storm Serpent creates wind zones that push players off platforms unless anchored by heavy items.

Threat generated by heavy harvesting or raids leads to **base defence** events.  Players must build defensive structures (turrets, walls, traps), assign NPCs to patrols and return from expeditions to repel attacks.  Co‑op introduces social dynamics: players may compete for the best‑equipped NPCs or coordinate builds to complement each other.  Shared resources and goals mean that selfish play can hurt the whole team; thus, cooperation is incentivized without forcing identical builds.

---

## 5 Design Rules & Long‑Term Vision

Across all documents, several design principles emerge:

1. **Combat‑compatible harvesting** – Every resource node must be a valid combat target with HP, tags and reactions.  Players should never leave combat mode to gather.
2. **Multi‑role NPCs** – Every companion must have at least three functions: a base job, a combat bond effect and a progression unlock.
3. **Buildings unlock actions** – Every structure should unlock a new item pool, production chain, NPC role or boss preparation step.
4. **Items have tag synergies** – Even simple items should participate in tag‑driven synergies.  Discovering and exploiting these interactions is a core motivator.
5. **Bosses test biome mastery** – A guardian should challenge players on the specific mechanics of its biome, not just soak damage.
6. **Co‑op amplifies, not breaks, the loop** – The shared base and personal builds must work together; progression should be communal, but item drops and builds remain personal.

The long‑term roadmap envisions additional biomes, dozens of new NPC types, hundreds of items, advanced automation, raid systems, companion relationships and dedicated server co‑op.  However, the core formula remains constant:

> **Attack the world to harvest resources → Build and upgrade a settlement → Recruit companions → Unlock items and assemble a build → Defeat the biome boss → Unlock the next biome.**

All future features should reinforce or elaborate on this cycle.  If a mechanic does not strengthen at least one link in this chain, it does not belong in the core scope.
