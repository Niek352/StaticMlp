# Roadmap: Settlement + NPC Economy Improvements

## Overview

This roadmap upgrades the current camp repair prototype into a Stage1 settlement economy slice. The sequence is contract-first: resources and building metadata, then construction/content, then operation state, worker jobs, Stage1 progression, and finally UX.

## Phases

- [x] **Phase 1: Settlement Foundation** - Expand resource/storage and building metadata contracts without changing gameplay flow yet.
- [ ] **Phase 2: Building Catalog And Construction** - Add Stage1 buildings and connect them to construction/network/presentation catalogs.
- [ ] **Phase 3: Storage And Operations** - Add stockpile, shelter, and workbench operation contracts that buildings can own after construction.
- [ ] **Phase 4: NPC Worker Economy** - Expand worker roles and make NPC workers discover gather, haul, and process demands.
- [ ] **Phase 5: Stage1 Progression And Loadout** - Require the first real settlement economy chain before loadout preparation.
- [ ] **Phase 6: UX And Presentation** - Replace debug-like interaction with raycast-first focus and generic building action/details presentation.

## Phase Details

### Phase 1: Settlement Foundation

**Goal**: Establish typed resource, storage, construction, and building definition contracts for Stage1 settlement growth.
**Depends on**: Nothing.
**Plans**: 3 plans.

Plans:

- [x] 01-01: Extend resource catalog and settlement shared storage beyond wood/stone.
- [x] 01-02: Add building metadata for categories, capabilities, interactions, NPC profile, and operation profile.
- [x] 01-03: Generalize construction resource ledgers and deposit rules over the Stage1 resource contract.

### Phase 2: Building Catalog And Construction

**Goal**: Add the initial Stage1 settlement building set through catalog data and existing construction pipelines.
**Depends on**: Phase 1.
**Plans**: 3 plans.

Plans:

- [x] 02-01: Add Camp Core, Stockpile, Bedroll Shelter, Lumber Camp, Stone Mine, and Workbench catalog definitions.
- [x] 02-02: Register network and presentation catalog entries for the new buildings without prefab asset generation.
- [ ] 02-03: Emit construction completion facts that operation owners can consume.

### Phase 3: Storage And Operations

**Goal**: Give finished buildings typed operation state so Stage1 has real storage, service, and production surfaces.
**Depends on**: Phase 2.
**Plans**: 3 plans.

Plans:

- [ ] 03-01: Implement Stockpile capacity and settlement storage contribution state.
- [ ] 03-02: Implement Bedroll Shelter bed slot/service state.
- [ ] 03-03: Implement Workbench input/output/recipe queue state.

### Phase 4: NPC Worker Economy

**Goal**: Expand worker roles and task selection from camp builder repair work into an early gather/haul/process loop.
**Depends on**: Phase 3.
**Plans**: 3 plans.

Plans:

- [ ] 04-01: Add worker roles and NPC/runtime mappings for Builder, Gatherer, Hauler, Processor, and Guard.
- [ ] 04-02: Add typed demand discovery for gather, haul, and process tasks.
- [ ] 04-03: Connect Lumber Camp and Stone Mine extraction buffers to Stockpile through worker tasks.

### Phase 5: Stage1 Progression And Loadout

**Goal**: Make Stage1 require a functioning settlement economy chain before loadout preparation.
**Depends on**: Phase 4.
**Plans**: 3 plans.

Plans:

- [ ] 05-01: Extend Stage1 progress stages with StockpilePlaced, ShelterPlaced, ExtractionOnline, and WorkbenchOnline.
- [ ] 05-02: Update Stage1 objectives, hints, flow view state, and tests around the new economy gates.
- [ ] 05-03: Add settlement-aware loadout module definitions for utility, build signal, and base infrastructure slots.

### Phase 6: UX And Presentation

**Goal**: Replace prototype settlement interaction with typed, scalable player-facing UX.
**Depends on**: Phase 5.
**Plans**: 3 plans.

Plans:

- [ ] 06-01: Change Stage1 context focus to raycast-first with proximity fallback.
- [ ] 06-02: Add generic building action/details presentation state.
- [ ] 06-03: Expand build menu overlay categories/cards and document required Unity view wiring.

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Settlement Foundation | 3/3 | Complete | 2026-05-20 |
| 2. Building Catalog And Construction | 2/3 | In progress | - |
| 3. Storage And Operations | 0/3 | Not started | - |
| 4. NPC Worker Economy | 0/3 | Not started | - |
| 5. Stage1 Progression And Loadout | 0/3 | Not started | - |
| 6. UX And Presentation | 0/3 | Not started | - |
