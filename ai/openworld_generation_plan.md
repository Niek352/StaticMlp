# OpenWorldGeneration Roadmap

Дата статуса: 2026-05-14

## Текущий этап

Мы находимся на этапе **Phase 3: LayerProcGen Backend Adapter**.

Статус: Phase 3 implementation завершена: LayerProcGen adapter изолирован в `Runtime/Logic/LayerProcGen`, Unity compile/Edit Mode/playmode verification пройдены.

Verification note 2026-05-14:

- [x] `git status --short` выполняется; прежний `fatal: index file corrupt` не воспроизводится.
- [x] Static guard check: `Runevision.LayerProcGen` не найден в OpenWorldGeneration runtime contracts/logic/presentation, кроме строки guard-test.
- [x] Static guard check: другие features, включая `Frontier`, не ссылаются на `OpenWorldGeneration`.
- [x] Static guard check: в OpenWorldGeneration/test scope не найдены `.prefab` и `.Generated.cs`.
- [x] Unity Editor compile verification.
- [x] Unity Editor Edit Mode tests verification.
- [x] Manual playmode terrain verification.

Сейчас в проекте есть:

- [x] отдельная feature `StaticMlp.Features.OpenWorldGeneration`;
- [x] контракты генерации мира без зависимости от LayerProcGen;
- [x] finite chunk bounds `X/Z = -8..7`;
- [x] deterministic simple/noise backend;
- [x] mesh terrain builder с LOD0-LOD3;
- [x] client-only terrain streaming runtime;
- [x] runtime terrain chunks как Unity `GameObject`, не ECS entities;
- [x] tests для determinism, borders, LOD vertex counts, bounds и architecture guards;
- [x] Unity compile проверка;
- [x] Unity Edit Mode tests;
- [x] ручная scene/playmode проверка отображения terrain вокруг player.

`Frontier` не является owner этой системы. `Frontier` остается owner Stage 1 expedition/threat/raid flow и может позже только читать public contracts/events OpenWorldGeneration, если экспедициям понадобится world data.

## Phase 0: Architecture And Boundary

- [x] Зафиксировать, что OpenWorldGeneration является отдельной feature.
- [x] Не добавлять OpenWorldGeneration внутрь `Frontier`.
- [x] Не добавлять gameplay placements в v1.
- [x] Не создавать terrain ECS entities.
- [x] Не ссылаться на `Runevision.LayerProcGen` из contracts/gameplay/presentation.
- [x] Использовать finite chunk bounds вместо бесконечной карты.
- [x] Оставить LayerProcGen как будущий backend adapter.

## Phase 1: Core + Visual Terrain MVP

- [x] Создать `Runtime/Contracts` asmdef.
- [x] Добавить `WorldChunkId`.
- [x] Добавить `WorldChunkBounds`.
- [x] Добавить `WorldGenerationSeed`.
- [x] Добавить `WorldGenerationRequest`.
- [x] Добавить `SurfaceSample`.
- [x] Добавить `TerrainMeshData`.
- [x] Добавить `GeneratedChunkData`.
- [x] Добавить `IWorldGenerationService`.
- [x] Добавить `IHeightSampler`.
- [x] Добавить `ISurfaceSampler`.
- [x] Создать `Runtime/Logic` asmdef.
- [x] Добавить `TerrainMeshBuildRequest`.
- [x] Добавить `TerrainMeshBuilder`.
- [x] Поддержать LOD0 `65x65`.
- [x] Поддержать LOD1 `33x33`.
- [x] Поддержать LOD2 `17x17`.
- [x] Поддержать LOD3 `9x9`.
- [x] Строить vertices в local chunk space.
- [x] Брать высоты из world coordinates.
- [x] Добавить optional skirts.
- [x] Добавить `SimpleWorldGenerationService`.
- [x] Добавить deterministic `SimpleSurfaceSampler`.
- [x] `GenerateChunk` throws для chunks вне `WorldChunkBounds`.
- [x] Создать `Runtime/Presentation` asmdef.
- [x] Добавить `OpenWorldGenerationPresentationFeature`.
- [x] Добавить client terrain streaming system.
- [x] Добавить runtime terrain root/resource.
- [x] Добавить `TerrainChunkViewFactory`.
- [x] Добавить `TerrainChunkView`.
- [x] Добавлять `MeshCollider` только для near chunks.
- [x] Выгружать chunks вне view radius.
- [x] Добавить edit-mode tests.
- [x] Добавить architecture tests.
- [x] Запустить Unity compile.
- [x] Запустить `StaticMlp.Tests.OpenWorldGeneration`.
- [x] Запустить `StaticMlp.Tests.Architecture`.
- [x] Проверить в playmode, что terrain появляется вокруг local player.
- [x] Проверить, что дальние chunks без collider.
- [x] Проверить, что движение player обновляет loaded chunks.

## Phase 2: Authoring And Tuning

- [x] Решить, нужен ли inspector-facing config asset или текущего runtime config достаточно: используем текущий runtime ECS config, без ScriptableObject asset.
- [x] Настроить seed, bounds, view radius, collider radius и material color под текущую сцену: текущие defaults оставлены без изменения.
- [x] Добавить debug gizmos для chunk bounds.
- [x] Добавить debug overlay/лог active chunks, LOD и seed: добавлен editor-only throttled debug log без MVC/Canvas overlay.
- [x] Проверить визуальные cracks на границах chunks.
- [x] Проверить LOD transitions со skirts.
- [x] Добавить cleanup проверки на отсутствие leaked GameObjects/Meshes при disable/shutdown.

## Phase 3: LayerProcGen Backend Adapter

- [x] Решить package strategy для LayerProcGen: оставить local package, vendor в `Assets/ThirdParty`, или заменить на стабильный package source.
- [x] Добавить `ThirdPartyNotices.txt` для LayerProcGen MPL 2.0, если LPG становится частью shared repo/distribution: не требуется, LPG остается package dependency и не vendored.
- [x] Создать отдельный adapter слой в `Runtime/Logic/LayerProcGen`.
- [x] Добавить LPG-backed implementation of `IWorldGenerationService`.
- [x] Не менять исходники LPG без отдельного архитектурного решения.
- [x] Реализовать initial layers: region, biome, height, final surface, terrain mesh data.
- [x] Проверить deterministic same seed + chunk id.
- [x] Проверить border matching между соседними chunks.
- [x] Проверить, что contracts/presentation не импортируют `Runevision.LayerProcGen`.
- [x] Обновить architecture guard, разрешив LPG references только adapter assembly/folder.

## Phase 4: Server Gameplay Placements

- [ ] Спроектировать public placement contracts без Unity objects и без LPG types.
- [ ] Добавить `ResourcePlacement` только как generated input, не как replicated truth.
- [ ] Добавить `SpawnPlacement` только как generated input, не как replicated truth.
- [ ] На сервере создавать authoritative gameplay entities через owner features/factories.
- [ ] Не позволять client terrain generation создавать authoritative resources/NPCs.
- [ ] Связать spawned gameplay entities с `WorldChunkId`, если это нужно для persistence/unload.
- [ ] Сохранять только deltas: removed resources, mined rocks, chopped trees, built structures, ownership/world state.
- [ ] Не сохранять generated mesh/heightmap без отдельной причины.

## Phase 5: Frontier Integration If Needed

- [ ] Определить, действительно ли expedition/threat flow нуждается в OpenWorld data.
- [ ] Если нужно, добавить read-only dependency из `Frontier` на OpenWorld contracts.
- [ ] Не переносить OpenWorld owner logic в `Frontier`.
- [ ] Не давать `Frontier` мутировать OpenWorld-owned state напрямую.
- [ ] Связывать через events/contracts, если появятся cross-feature gameplay effects.

## Verification Checklist

- [x] Unity project compiles.
- [x] `StaticMlp.Tests.OpenWorldGeneration` passes.
- [x] `StaticMlp.Tests.Architecture` passes.
- [x] Нет `Runevision.LayerProcGen` references вне будущего LPG adapter.
- [x] Нет direct OpenWorldGeneration references из `Frontier` в v1.
- [x] Нет generated `.prefab` assets.
- [x] Нет manually edited `.Generated.cs`.
- [x] Terrain chunks are visual-only Unity runtime objects.
- [x] Gameplay entities remain server-authoritative.

## Known Risks

- [x] Git index был поврежден при прежней проверке (`fatal: index file corrupt`), но `git status --short` снова выполняется; перед commit diff/status проверены.
- [ ] Текущий LayerProcGen package path указывает на `file:Z:/_UnityProjects/LayerProcGen`, это не portable setup для команды/CI.
- [ ] Simple/noise backend нужен только для validation visual pipeline; он не заменяет final LPG generation.
- [ ] Runtime terrain пока генерируется синхронно на client system update, большие radii могут давать hitching.

Phase 3 implementation note 2026-05-14:

- [x] Seam hotfix: removed chunk-local LPG height offset that created near-border terrain steps while exact edge tests still passed.
- [x] Seam regression: LPG tests now check near-border continuity across same-LOD neighboring chunks, including negative coordinates.
- [x] Interior crease hotfix: replaced hard absolute LPG ridge with a differentiable squared ridge to avoid normal/shading creases inside chunks.
- [x] Interior crease regression: LPG tests now scan adjacent `LOD0` mesh normals inside representative chunks.

- [x] Package strategy оставлена `file:Z:/_UnityProjects/LayerProcGen`.
- [x] LPG source не изменялся и не vendored в `Assets/ThirdParty`.
- [x] Adapter assembly добавлен под `Runtime/Logic/LayerProcGen`.
- [x] Runtime terrain default service переключен на `LayerProcGenWorldGenerationService`.
- [x] `Runevision.LayerProcGen` references ограничены adapter folder.
- [x] Startup hang fix: LPG top dependencies кэшируются до dispose, terrain streaming ограничен `MaxChunkLoadsPerFrame`.
- [x] Unity Editor compile после Phase 3.
- [x] Unity Editor `StaticMlp.Tests.OpenWorldGeneration` после Phase 3.
- [x] Unity Editor `StaticMlp.Tests.Architecture` после Phase 3.
