# Codex Feature Research Scopes — Index

Этот пакет разбивает исходный Deep Research на отдельные research/implementation документы для Codex.

## Файлы

1. `01-design-lock.md`
   - NPC classes.
   - Resource families.
   - NPC acquisition paths.
   - Build/Equipment slot rules.
   - ECS contracts.

2. `02-combat-director.md`
   - Combat cells.
   - Threat budget.
   - Spawn sources.
   - Enemy roles.
   - Server-authoritative enemy spawn.

3. `03-npc-economy.md`
   - Priority board.
   - NPC work roles.
   - Food/Fuel/Wear leaks.
   - Specialist-gated recipes.
   - ECS economy task pipeline.

4. `04-build-equipment-modules.md`
   - Module library.
   - Slot-limited loadout.
   - Combat/Utility/BuildSignal/BaseInfrastructure modules.
   - Module effect pipeline.

5. `05-vertical-slice.md`
   - One biome.
   - One complete gameplay loop.
   - NPC acquisition.
   - Station unlock.
   - Mini-boss and boss.
   - Integration checklist.

6. `06-production-ready-direction.md`
   - Biome taxes.
   - Raids.
   - Caravans.
   - Outpost network.
   - Simulation layers.

## Как использовать в Codex

Рекомендуемый порядок:

1. Сначала дать Codex `01-design-lock.md`.
2. После фиксации contracts — `02-combat-director.md` и `03-npc-economy.md`.
3. Затем `04-build-equipment-modules.md`.
4. После этого собрать `05-vertical-slice.md`.
5. `06-production-ready-direction.md` использовать только после успешного vertical slice.

## Общая установка

Не реализовывать gameplay через MonoBehaviour state. Runtime truth — ECS components/systems/resources/events. MonoBehaviour допустим для composition, authoring, view binding, UI и client-only presentation.
