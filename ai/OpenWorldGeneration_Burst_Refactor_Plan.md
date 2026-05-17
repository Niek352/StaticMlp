# План рефакторинга: LayerProcGen → LayerProcGenLite (Option B)

**Цель:** Заменить managed `LayerProcGen` с его `Thread.Sleep`, process-wide singletons и синхронным `GenerateChunk` на узкий Burst-friendly job pipeline (`LayerProcGenLite`), полностью интегрированный в StaticEcs ECS event flow.

**Подход:** Не адаптировать существующий `LayerProcGen.Burst` proof-of-concept, а написать новый backend с нуля, заимствуя из Burst решения только `ChunkId` math, noise utilities и идею dependency graph.

---

## 1. Архитектура целевой системы

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  SERVER                                                                     │
│  ServerOpenWorldChunkInterestSystem ──► OpenWorldChunkLoadRequested        │
│                                              │                              │
│  OpenWorldChunkGenerationSystem ◄────────────┘                              │
│  (schedules Burst jobs: height → surface → placements)                     │
│       │                                                                     │
│       ▼                                                                     │
│  OpenWorldChunkGenerationCompleted (event with placements)                 │
│       │                                                                     │
│       ▼                                                                     │
│  ServerOpenWorldResourceNodeSeedSystem ──► spawns resource entities        │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│  CLIENT                                                                     │
│  ClientOpenWorldTerrainStreamingSystem ──► requests chunks                 │
│                                              │                              │
│  OpenWorldChunkGenerationSystem ◄────────────┘ (shared height/surface)     │
│       │                                                                     │
│       ▼                                                                     │
│  OpenWorldChunkGenerationCompleted (event with mesh + placements)          │
│       │                                                                     │
│       ▼                                                                     │
│  ClientOpenWorldTerrainApplySystem ──► TerrainChunkView.Apply(mesh)        │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Принципы
1. **Нет синхронного `GenerateChunk`**. Вместо него — ECS events + async job handles.
2. **Shared height/surface generation**. Оба конвейера (server placements, client mesh) используют один и тот же heightmap + surface samples.
3. **Burst everywhere**. Height formula, surface sampling, normal computation, placement generation — всё в `[BurstCompile]` jobs.
4. **Mesh build частично на main thread**. Vertex/index generation — Burst job, но `Mesh` создаётся и применяется на main thread (Unity API limitation).
5. **Нет managed Dictionary в hot path**. Chunk lookup — `NativeHashMap<WorldChunkId, ChunkState>` или managed `Dictionary` только для lifecycle tracking (не внутри jobs).

---

## 2. Новые файлы (создать)

### 2.1. Domain / Math (borrowed from Burst solution)

#### `Runtime/Logic/Domain/ChunkProcGenMath.cs`
- **Назначение:** Burst-compatible math utilities для procedural generation.
- **Что содержит:**
  - `PerlinNoise(uint seed, float2 pos)` — из `LayerProcGenMath`
  - `ValueNoise(uint seed, int2 pos)` — из `LayerProcGenMath`
  - `Bilinear(float c00, float c10, float c01, float c11, float tx, float ty)` — из `LayerProcGenMath`
  - `Fade(float t)` — из `LayerProcGenMath`
  - `Smoothstep(float edge0, float edge1, float x)` — из `LayerProcGenMath`
  - `GetPositionRandom(uint worldSeed, int2 worldPos)` — deterministic random для позиции
- **Зависимости:** `Unity.Mathematics`
- **Примечания:** Взять как есть из `LayerProcGen.Burst/Runtime/Core/LayerProcGenMath.cs`, адаптировав namespace на `StaticMlp.Features.OpenWorldGeneration.Domain`.

#### `Runtime/Logic/Domain/ChunkJobUtils.cs`
- **Назначение:** Burst utilities для chunk indexing.
- **Что содержит:**
  - `IndexToLocal2D(int index, int width) -> (int x, int y)`
  - `Local2DToIndex(int x, int y, int width) -> int`
  - `ChunkLocalToWorldFloat(WorldChunkId chunkId, int localX, int localY, float chunkWorldSize) -> float2`
  - `ChunkLocalToWorldCenter(WorldChunkId chunkId, int localX, int localY, float chunkWorldSize) -> float2`
- **Зависимости:** `Unity.Mathematics`, `StaticMlp.Features.OpenWorldGeneration` (для `WorldChunkId`)
- **Примечания:** Адаптировать из `ChunkJobUtils`, заменив `ChunkId` на `WorldChunkId`.

---

### 2.2. Domain / Deterministic Hash

#### `Runtime/Logic/Domain/ChunkDeterministicHash.cs`
- **Назначение:** Deterministic hashing для chunk-local random и placement ID generation.
- **Что содержит:**
  - `Hash(uint seed, WorldChunkId chunkId, int stream, int index) -> uint`
  - `Hash(uint seed, WorldChunkId chunkId, int stream) -> uint`
  - `Unit(uint hash) -> float [0,1)`
  - `CreatePlacementId(uint seed, WorldChunkId chunkId, int stream, int index) -> long`
- **Зависимости:** `Unity.Mathematics`
- **Примечания:** Перенести логику из текущего `OpenWorldPlacementGenerator.Hash()`, `CreatePlacementId()`, `RotateLeft()`, `Unit()`. Заменить `int seed` на `uint seed` для Burst compatibility. Использовать `math.rol` вместо ручного `RotateLeft`.

---

### 2.3. Burst Jobs

#### `Runtime/Logic/Jobs/HeightmapGenerationJob.cs`
- **Назначение:** Генерация heightmap для одного чанка.
- **Сигнатура:**
  ```csharp
  [BurstCompile]
  public struct HeightmapGenerationJob : IJobParallelFor
  {
      public WorldChunkId ChunkId;
      public uint WorldSeed;
      public float ChunkWorldSize;
      public int Resolution; // e.g. 128 for 128x128 grid

      [WriteOnly] public NativeArray<float> Heights;

      public void Execute(int index) { ... }
  }
  ```
- **Алгоритм:**
  1. `IndexToLocal2D(index, Resolution)` → local x, z
  2. `ChunkLocalToWorldFloat(ChunkId, localX, localZ, ChunkWorldSize)` → world xz
  3. Применить формулу из `LpgHeightLayer.SampleHeight()` (текущая):
     - `low = sin((worldX + seed * 17.13) * 0.0065) * cos((worldZ - seed * 9.71) * 0.0065) * 24`
     - `mid = sin((worldX - seed * 3.37) * 0.021 + (worldZ + seed * 2.11) * 0.008) * 7`
     - `ridgeWave = sin((worldX + seed) * 0.018) * cos((worldZ - seed) * 0.015)`
     - `ridge = ridgeWave * ridgeWave * 8`
     - `height = low + mid + ridge - 9`
  4. Записать в `Heights[index]`
- **Зависимости:** `ChunkProcGenMath`, `ChunkJobUtils`, `Unity.Mathematics`, `Unity.Collections`, `Unity.Jobs`, `Unity.Burst`
- **Примечания:** `Resolution` должно быть достаточным для всех LOD (LOD0 mesh может быть 64 quads, но height sampling может быть 128x128 для качества). Или генерировать с `BaseQuadCount + 1` resolution под конкретный LOD.

#### `Runtime/Logic/Jobs/SurfaceSamplingJob.cs`
- **Назначение:** Сэмплинг surface (height, normal, biome, material, water, wetness) для каждой точки heightmap.
- **Сигнатура:**
  ```csharp
  [BurstCompile]
  public struct SurfaceSamplingJob : IJobParallelFor
  {
      public WorldChunkId ChunkId;
      public uint WorldSeed;
      public float ChunkWorldSize;
      public int Resolution;
      public float WaterLevel;

      [ReadOnly] public NativeArray<float>.ReadOnly Heights;
      [WriteOnly] public NativeArray<SurfaceSample> Surfaces;

      public void Execute(int index) { ... }
  }
  ```
- **Алгоритм:**
  1. Получить height из `Heights[index]`
  2. Вычислить normal через соседние heights (finite differences):
     - `left = Heights[clamp(x-1, z)]`, `right = Heights[clamp(x+1, z)]`
     - `down = Heights[clamp(x, z-1)]`, `up = Heights[clamp(x, z+1)]`
     - `normal = normalize(float3(left - right, 2 * step, down - up))`
  3. `waterMask = height <= WaterLevel ? 1f : 0f`
  4. `biomeId = SelectBiome(...)` — из `LpgBiomeLayer` (если есть) или заглушка
  5. `materialId = SelectMaterialId(height, waterMask)` — из `LpgSurfaceLayer.SelectMaterialId()`
  6. `wetness = saturate((WaterLevel + 3f - height) / 6f)`
  7. Записать `SurfaceSample` в `Surfaces[index]`
- **Зависимости:** `SurfaceSample`, `ChunkJobUtils`, `Unity.Mathematics`, `Unity.Collections`, `Unity.Jobs`, `Unity.Burst`
- **Примечания:** `SurfaceSample` сейчас содержит `Vector3 Normal` (UnityEngine). Для Burst нужно либо сделать версию с `float3`, либо Burst умеет работать с `Vector3` (managed). Лучше создать `NativeSurfaceSample` с `float3` и конвертировать при apply.

#### `Runtime/Logic/Jobs/ResourcePlacementGenerationJob.cs`
- **Назначение:** Deterministic generation of `ResourcePlacement` + `SpawnPlacement` для чанка.
- **Сигнатура:**
  ```csharp
  [BurstCompile]
  public struct ResourcePlacementGenerationJob : IJob
  {
      public WorldChunkId ChunkId;
      public uint WorldSeed;
      public float ChunkWorldSize;
      public float WaterLevel;

      [ReadOnly] public NativeArray<float>.ReadOnly Heights;
      [ReadOnly] public NativeArray<NativeSurfaceSample>.ReadOnly Surfaces;

      public NativeArray<ResourcePlacement> ResourcePlacements;
      public NativeArray<int> ResourcePlacementCount;
      public NativeArray<SpawnPlacement> SpawnPlacements;
      public NativeArray<int> SpawnPlacementCount;

      public void Execute() { ... }
  }
  ```
- **Алгоритм:**
  1. `RESOURCE_CANDIDATES_PER_CHUNK = 24`, `SPAWN_CANDIDATES_PER_CHUNK = 8`
  2. Для каждого candidate:
     - `hash = ChunkDeterministicHash.Hash(WorldSeed, ChunkId, stream, index)`
     - `unit = ChunkDeterministicHash.Unit(hash)`
     - Вычислить x, z внутри чанка с margin
     - Найти ближайший height/surface из `Heights`/`Surfaces` (bilinear или nearest)
     - Проверить `waterMask <= MAX_WATER_MASK && normal.y >= MIN_NORMAL_Y`
     - Если прошёл — записать в `ResourcePlacements[written++]`
  3. Fallback grid scan если 0 placements
  4. Записать counts
- **Зависимости:** `ResourcePlacement`, `SpawnPlacement`, `ChunkDeterministicHash`, `ChunkJobUtils`, `Unity.Mathematics`, `Unity.Collections`, `Unity.Jobs`, `Unity.Burst`
- **Примечания:** Surface sampling для placements может быть на lower resolution чем heightmap. Для простоты — использовать тот же `Heights`/`Surfaces` и `SampleBilinear`.

#### `Runtime/Logic/Jobs/TerrainMeshGenerationJob.cs`
- **Назначение:** Генерация mesh vertices, indices, bounds.
- **Сигнатура:**
  ```csharp
  [BurstCompile]
  public struct TerrainMeshGenerationJob : IJob
  {
      public WorldChunkId ChunkId;
      public float ChunkWorldSize;
      public int Quads; // = BaseQuadCount >> Lod
      public bool AddSkirts;
      public float SkirtDepth;

      [ReadOnly] public NativeArray<float>.ReadOnly Heights;
      [ReadOnly] public NativeArray<NativeSurfaceSample>.ReadOnly Surfaces;

      [WriteOnly] public NativeArray<float3> Vertices;
      [WriteOnly] public NativeArray<float3> Normals;
      [WriteOnly] public NativeArray<float4> Tangents;
      [WriteOnly] public NativeArray<float2> Uvs;
      [WriteOnly] public NativeArray<Color32> Colors;
      [WriteOnly] public NativeArray<int> Triangles;
      public NativeArray<float> OutMinY;
      public NativeArray<float> OutMaxY;

      public void Execute() { ... }
  }
  ```
- **Алгореритм:**
  1. Grid `(Quads + 1) x (Quads + 1)` vertices
  2. Для каждого vertex: `sample = BilinearSample(Heights, Surfaces, ...)`, записать position, normal, color, uv
  3. Для каждого quad: 2 triangles
  4. Если `AddSkirts`:
     - Добавить vertices по периметру с `y -= SkirtDepth`
     - Добавить triangles для skirts
  5. Записать `minY`, `maxY`
- **Зависимости:** `TerrainMeshBuildRequest`, `ChunkJobUtils`, `Unity.Mathematics`, `Unity.Collections`, `Unity.Jobs`, `Unity.Burst`
- **Примечания:** `Color32` — UnityEngine struct, Burst-friendly (unmanaged). `float3`, `float4`, `float2` из `Unity.Mathematics`. Output arrays должны быть предварительно выделены с правильным размером.

---

### 2.4. ECS Events (Contracts)

#### `Runtime/Contracts/Events/OpenWorldChunkGenerationRequested.cs`
- **Назначение:** Запрос на генерацию чанка. Публикуется streaming/interest systems.
- **Сигнатура:**
  ```csharp
  public readonly struct OpenWorldChunkGenerationRequested : IEvent
  {
      public readonly WorldChunkId ChunkId;
      public readonly WorldGenerationRequest Request;
      public readonly GenerationOutputMask Outputs; // what to generate: Mesh, Placements, or both

      public OpenWorldChunkGenerationRequested(WorldChunkId chunkId, WorldGenerationRequest request, GenerationOutputMask outputs)
      { ... }
  }
  ```
- **Зависимости:** `WorldChunkId`, `WorldGenerationRequest`

#### `Runtime/Contracts/Definitions/GenerationOutputMask.cs`
- **Назначение:** Флаги, что именно нужно сгенерировать для чанка.
- **Сигнатура:**
  ```csharp
  [Flags]
  public enum GenerationOutputMask : byte
  {
      None = 0,
      Placements = 1 << 0, // server needs this
      Mesh = 1 << 1,       // client needs this
      All = Placements | Mesh
  }
  ```

#### `Runtime/Contracts/Events/OpenWorldChunkGenerationCompleted.cs`
- **Назначение:** Результат генерации чанка. Публикуется после завершения job pipeline.
- **Сигнатура:**
  ```csharp
  public readonly struct OpenWorldChunkGenerationCompleted : IEvent
  {
      public readonly WorldChunkId ChunkId;
      public readonly int Lod;
      public readonly TerrainMeshData TerrainMesh; // null if Mesh was not requested
      public readonly ResourcePlacement[] ResourcePlacements; // empty if Placements was not requested
      public readonly SpawnPlacement[] SpawnPlacements; // empty if Placements was not requested
  }
  ```
- **Примечания:** `TerrainMeshData` содержит managed arrays (`Vector3[]`, `int[]`), поэтому этот event может быть создан только на main thread после `JobHandle.Complete()`.

---

### 2.5. ECS Systems

#### `Runtime/Logic/Systems/Shared/OpenWorldChunkGenerationSystem.cs`
- **Назначение:** Central scheduler. Принимает `OpenWorldChunkGenerationRequested`, schedule jobs, track handles, publish `OpenWorldChunkGenerationCompleted` когда jobs завершены.
- **Сигнатура:**
  ```csharp
  public sealed class OpenWorldChunkGenerationSystem : ISystem
  {
      private EventReceiver<OpenWorldChunkGenerationRequested> _requests;
      private NativeHashMap<WorldChunkId, PendingChunkGeneration> _pending;
      private List<CompletedChunkGeneration> _completed;

      public void Init() { ... }
      public void Update() { ... }
      public void Destroy() { ... }
  }
  ```
- **Логика `Update`:**
  1. Для каждого нового `_requests`:
     - Проверить, нет ли уже pending generation для этого `ChunkId + Lod`
     - Создать `NativeArray<float>` для heightmap (resolution = `request.BaseQuadCount + 1` или фиксированный 128)
     - Schedule `HeightmapGenerationJob`
     - Schedule `SurfaceSamplingJob` с dependency от height job
     - Если `outputs` содержит `Placements`:
       - Schedule `ResourcePlacementGenerationJob` с dependency от surface job
     - Если `outputs` содержит `Mesh`:
       - Выделить output arrays для mesh (vertices, triangles, etc.)
       - Schedule `TerrainMeshGenerationJob` с dependency от surface job
     - Сохранить `JobHandle`(ы) в `_pending[chunkId]`
  2. Для каждого pending generation:
     - Проверить `JobHandle.IsCompleted`
     - Если все handles completed:
       - Вызвать `Complete()` для всех handles
       - Сконструировать `TerrainMeshData` из NativeArrays (main thread only)
       - Скопировать `ResourcePlacement`/`SpawnPlacement` в managed arrays
       - Dispose всех NativeArrays
       - Отправить `OpenWorldChunkGenerationCompleted`
       - Удалить из `_pending`
- **Зависимости:** Все job types, `OpenWorldChunkGenerationRequested`, `OpenWorldChunkGenerationCompleted`
- **Примечания:** Этот system должен работать и на сервере, и на клиенте (shared logic). На сервере `OpenWorldChunkLoadRequested` → маппится в `OpenWorldChunkGenerationRequested` с `GenerationOutputMask.Placements`. На клиенте `ClientOpenWorldTerrainStreamingSystem` → `OpenWorldChunkGenerationRequested` с `GenerationOutputMask.Mesh`.

#### `Runtime/Logic/Systems/Server/ServerOpenWorldChunkGenerationBridgeSystem.cs`
- **Назначение:** Bridge между существующим `OpenWorldChunkLoadRequested` и новым `OpenWorldChunkGenerationRequested`.
- **Сигнатура:**
  ```csharp
  public sealed class ServerOpenWorldChunkGenerationBridgeSystem : ISystem
  {
      private EventReceiver<ServerWT, OpenWorldChunkLoadRequested> _loadRequests;
      public void Init() { _loadRequests = SW.RegisterEventReceiver<OpenWorldChunkLoadRequested>(); }
      public void Update()
      {
          var runtime = SW.GetResource<OpenWorldGenerationServerRuntime>();
          foreach (var request in _loadRequests)
          {
              SW.SendEvent(new OpenWorldChunkGenerationRequested(
                  request.Value.ChunkId,
                  runtime.DefaultRequest,
                  GenerationOutputMask.Placements));
          }
      }
  }
  ```
- **Заменяет:** прямой вызов `runtime.GenerationService.GenerateChunk()` в `ServerOpenWorldResourceNodeSeedSystem`.

#### `Runtime/Logic/Systems/Client/ClientOpenWorldTerrainApplySystem.cs`
- **Назначение:** Применяет сгенерированный mesh к `TerrainChunkView`.
- **Сигнатура:**
  ```csharp
  public sealed class ClientOpenWorldTerrainApplySystem : ISystem
  {
      private EventReceiver<ClientWT, OpenWorldChunkGenerationCompleted> _completed;
      private OpenWorldTerrainRuntime _runtime;

      public void Init() { _completed = CW.RegisterEventReceiver<OpenWorldChunkGenerationCompleted>(); _runtime = CW.GetResource<OpenWorldTerrainRuntime>(); }
      public void Update()
      {
          foreach (var evt in _completed)
          {
              if (evt.Value.TerrainMesh != null)
                  _runtime.ApplyChunkMesh(evt.Value.ChunkId, evt.Value.Lod, evt.Value.TerrainMesh);
          }
      }
  }
  ```
- **Зависимости:** `OpenWorldChunkGenerationCompleted`, `OpenWorldTerrainRuntime`
- **Примечания:** Требует добавления метода `ApplyChunkMesh` в `OpenWorldTerrainRuntime`.

---

### 2.6. WorldResources / Runtime

#### `Runtime/Logic/WorldResources/OpenWorldChunkGenerationRuntime.cs`
- **Назначение:** Resource для хранения глобального состояния generation runtime (seed, bounds, chunk size, water level).
- **Сигнатура:**
  ```csharp
  public sealed class OpenWorldChunkGenerationRuntime : IResource
  {
      public readonly WorldGenerationSeed Seed;
      public readonly WorldChunkBounds Bounds;
      public readonly float ChunkWorldSize;
      public readonly float WaterLevel;
      public readonly int BaseQuadCount;
      public readonly bool AddSkirts;
      public readonly float SkirtDepth;

      public static OpenWorldChunkGenerationRuntime CreateDefault()
      {
          return new OpenWorldChunkGenerationRuntime(
              new WorldGenerationSeed(12345),
              WorldChunkBounds.Default,
              128f,
              -7f,
              64,
              true,
              6f);
      }
  }
  ```
- **Заменяет:** `OpenWorldGenerationServerRuntime` (partially — server-specific settings остаются в `OpenWorldGenerationServerRuntime`).
- **Примечания:** `WaterLevel` раньше был `const` в `LpgHeightLayer`. Теперь он должен быть конфигурируемым. `OpenWorldChunkGenerationSystem` читает этот resource для параметров генерации.

---

### 2.7. Native structs for Burst

#### `Runtime/Logic/Domain/NativeSurfaceSample.cs`
- **Назначение:** Burst-compatible версия `SurfaceSample`.
- **Сигнатура:**
  ```csharp
  public struct NativeSurfaceSample
  {
      public float Height;
      public float3 Normal;
      public byte BiomeId;
      public byte PrimaryMaterialId;
      public float RoadMask;
      public float WaterMask;
      public float Wetness;
  }
  ```
- **Примечания:** Конвертация `NativeSurfaceSample → SurfaceSample` при необходимости на main thread.

---

## 3. Файлы для изменения (modify)

### 3.1. `Runtime/Logic/Systems/Server/ServerOpenWorldResourceNodeSeedSystem.cs`
**Что менять:**
- Убрать прямой вызов `runtime.GenerationService.GenerateChunk(request.Value.ChunkId, runtime.DefaultRequest)`.
- Вместо этого подписаться на `OpenWorldChunkGenerationCompleted`.
- Фильтровать только события, где `ResourcePlacements.Length > 0`.
- Остальная логика `SpawnChunkResourceNodes` остаётся без изменений.

**Почему:** Server больше не должен синхронно генерировать чанки. Generation — async через `OpenWorldChunkGenerationSystem`.

### 3.2. `Runtime/Presentation/WorldResources/OpenWorldTerrainRuntime.cs`
**Что менять:**
- Убрать `IWorldGenerationService _generationService`.
- Убрать `LayerProcGenWorldGenerationService.AcquireShared()` в `Create()`.
- Добавить метод:
  ```csharp
  public void ApplyChunkMesh(WorldChunkId chunkId, int lod, TerrainMeshData mesh)
  {
      if (_loadedChunks.TryGetValue(chunkId, out var loaded)
          && loaded.Lod == lod)
      {
          loaded.View.Apply(chunkId, lod, mesh, loaded.Collision);
          return;
      }

      // Chunk was requested but not yet in _loadedChunks — queue for apply
  }
  ```
- В `StreamAround()`:
  - Вместо `_generationService.GenerateChunk()` → отправить `OpenWorldChunkGenerationRequested` event через `CW.SendEvent()` с `GenerationOutputMask.Mesh`.
  - `LoadOrUpdateChunk()` должен либо применять уже готовый mesh (если он есть), либо инициировать generation и вернуть `false` (без mesh).
- Убрать `IDisposable` от `_generationService`.

**Почему:** Presentation больше не зависит от generation backend. Она только инициирует requests и применяет результаты.

### 3.3. `Runtime/Presentation/Systems/Client/ClientOpenWorldTerrainStreamingSystem.cs`
**Что менять:**
- Добавить `OpenWorldChunkGenerationRequested` event sending в `_runtime.StreamAround()` flow.
- Или перенести эту логику в `OpenWorldTerrainRuntime`.

**Почему:** Streaming system должна инициировать async generation, а не ждать синхронного ответа.

### 3.4. `Runtime/Logic/WorldResources/OpenWorldGenerationServerRuntime.cs`
**Что менять:**
- Убрать `IWorldGenerationService` полностью.
- Оставить только `DefaultRequest`, `MaxChunkGenerationsPerFrame`, `StaticStreamingRadiusInChunks`.
- Убрать `Func<IWorldGenerationService> _generationServiceFactory`.

**Почему:** Server больше не использует `IWorldGenerationService`. Generation — через `OpenWorldChunkGenerationSystem`.

### 3.5. `Runtime/Logic/LayerProcGen/OpenWorldGenerationLayerProcGenFeature.cs`
**Что менять:**
- Убрать `LayerProcGenWorldGenerationService.AcquireShared` из `RegisterServerResources()`.
- Вместо этого установить `OpenWorldChunkGenerationRuntime` и `OpenWorldGenerationServerRuntime` (без service factory).
- Добавить `OpenWorldChunkGenerationSystem` и `ServerOpenWorldChunkGenerationBridgeSystem` в `RegisterServerSystems()`.

**Почему:** Feature bootstrap должен регистрировать новые systems и resources.

### 3.6. `Runtime/Presentation/OpenWorldGenerationPresentationFeature.cs`
**Что менять:**
- Добавить `ClientOpenWorldTerrainApplySystem` в `RegisterClientCoreSystems()`.
- Убедиться, что `ClientOpenWorldTerrainStreamingSystem` регистрирован.

### 3.7. `Runtime/Logic/Domain/TerrainMeshBuilder.cs`
**Что менять:**
- Пометить как `[Obsolete("Use TerrainMeshGenerationJob instead")]` или удалить после того, как `TerrainMeshGenerationJob` будет готов.
- Либо оставить как fallback для сравнения/тестирования.

### 3.8. `Runtime/Logic/Domain/OpenWorldPlacementGenerator.cs`
**Что менять:**
- Пометить как `[Obsolete("Use ResourcePlacementGenerationJob instead")]` или удалить.

### 3.9. `Runtime/Logic/Domain/SimpleWorldGenerationService.cs`
**Что менять:**
- Пометить как `[Obsolete("Replaced by OpenWorldChunkGenerationSystem")]` или удалить.

### 3.10. `Runtime/Contracts/Domain/IWorldGenerationService.cs`
**Что менять:**
- Удалить. Больше не нужен.

### 3.11. `StaticMlp.Features.OpenWorldGeneration.Presentation.asmdef`
**Что менять:**
- Убрать `"StaticMlp.Features.OpenWorldGeneration.LayerProcGen"` из references.

### 3.12. `StaticMlp.Features.OpenWorldGeneration.LayerProcGen.asmdef`
**Что менять:**
- Убрать `"Runevision.LayerProcGen"`, `"Runevision.Common"`, `"Runevision.Interop"` из references.
- Этот asmdef будет удалён в Phase 4, но сначала нужно убрать зависимости.

### 3.13. `Packages/manifest.json`
**Что менять:**
- Убрать `"com.layer-proc-gen": "file:Z:/_UnityProjects/LayerProcGen"`.

---

## 4. Файлы для удаления (delete)

После того как новые systems полностью работают и проходят тестирование:

1. `Runtime/Logic/LayerProcGen/LayerProcGenWorldGenerationService.cs`
2. `Runtime/Logic/LayerProcGen/LayerProcGenWorldContext.cs`
3. `Runtime/Logic/LayerProcGen/LayerProcGenWorldSettings.cs`
4. `Runtime/Logic/LayerProcGen/LpgHeightLayer.cs`
5. `Runtime/Logic/LayerProcGen/LpgHeightChunk.cs`
6. `Runtime/Logic/LayerProcGen/LpgSurfaceLayer.cs`
7. `Runtime/Logic/LayerProcGen/LpgSurfaceChunk.cs`
8. `Runtime/Logic/LayerProcGen/LpgSurfaceSampler.cs`
9. `Runtime/Logic/LayerProcGen/LpgBiomeLayer.cs`
10. `Runtime/Logic/LayerProcGen/LpgBiomeChunk.cs`
11. `Runtime/Logic/LayerProcGen/LpgRegionLayer.cs`
12. `Runtime/Logic/LayerProcGen/LpgRegionChunk.cs`
13. `Runtime/Logic/LayerProcGen/LpgTerrainMeshLayer.cs`
14. `Runtime/Logic/LayerProcGen/LpgTerrainMeshChunk.cs`
15. `Runtime/Logic/LayerProcGen/OpenWorldGenerationLayerProcGenFeature.cs`
16. `StaticMlp.Features.OpenWorldGeneration.LayerProcGen.asmdef`
17. `Runtime/Logic/Domain/SimpleWorldGenerationService.cs` (если не нужен для тестов)
18. `Runtime/Contracts/Domain/IWorldGenerationService.cs`

---

## 5. Порядок реализации

### Step 1: Core types + Native structs
1. Создать `ChunkProcGenMath.cs`
2. Создать `ChunkJobUtils.cs`
3. Создать `ChunkDeterministicHash.cs`
4. Создать `NativeSurfaceSample.cs`
5. Создать `OpenWorldChunkGenerationRuntime.cs`
6. Добавить `OpenWorldChunkGenerationRuntime` в feature bootstrap

**Результат:** Можно компилировать, нет интеграции.

### Step 2: Burst jobs (по одному, с unit test mindset)
1. `HeightmapGenerationJob` — проверить, что height formula даёт те же значения, что `LpgHeightLayer.SampleHeight()`.
2. `SurfaceSamplingJob` — проверить normals, water mask, material id.
3. `ResourcePlacementGenerationJob` — проверить deterministic output, сравнить с `OpenWorldPlacementGenerator`.
4. `TerrainMeshGenerationJob` — проверить vertex count, triangle count, bounds. Сравнить с `TerrainMeshBuilder.Build()`.

**Результат:** Каждый job работает изолированно, можно schedule + complete вручную.

### Step 3: ECS events + bridge
1. Создать `GenerationOutputMask.cs`
2. Создать `OpenWorldChunkGenerationRequested.cs`
3. Создать `OpenWorldChunkGenerationCompleted.cs`
4. Создать `ServerOpenWorldChunkGenerationBridgeSystem.cs`
5. Модифицировать `ServerOpenWorldResourceNodeSeedSystem` — подписаться на `OpenWorldChunkGenerationCompleted`

**Результат:** Server pipeline работает через events (но пока без реального job scheduling).

### Step 4: Central scheduler system
1. Создать `OpenWorldChunkGenerationSystem`
2. Интегрировать все jobs в pipeline
3. Протестировать на server: `OpenWorldChunkLoadRequested` → generation → `OpenWorldChunkGenerationCompleted` → resource spawning

**Результат:** Server async generation работает.

### Step 5: Client integration
1. Модифицировать `OpenWorldTerrainRuntime` — убрать `IWorldGenerationService`, добавить `ApplyChunkMesh()`
2. Модифицировать `ClientOpenWorldTerrainStreamingSystem` — отправлять `OpenWorldChunkGenerationRequested`
3. Создать `ClientOpenWorldTerrainApplySystem`
4. Протестировать client terrain streaming

**Результат:** Client async generation + mesh apply работает.

### Step 6: Cleanup
1. Удалить все `Lpg*.cs` файлы
2. Удалить `LayerProcGenWorldGenerationService.cs`
3. Удалить `IWorldGenerationService.cs`
4. Удалить `StaticMlp.Features.OpenWorldGeneration.LayerProcGen.asmdef`
5. Убрать `com.layer-proc-gen` из `manifest.json`
6. Убрать `LayerProcGen` reference из `Presentation.asmdef`
7. Собрать и протестировать

---

## 6. Архитектурные решения

### Heightmap resolution
- **Вопрос:** Какой resolution для heightmap? Фиксированный 128×128 или `BaseQuadCount + 1` под LOD?
- **Решение:** Фиксированный `128 × 128` (или `64 × 64` для экономии). Mesh job делает `SampleBilinear` из heightmap. Это позволяет:
  - Один heightmap на все LOD (меньше работы)
  - Более плавные нормали (больше sample points)

### Chunk data caching
- **Вопрос:** Кешировать ли `NativeArray<float>` heights между кадрами?
- **Решение:** Да. `OpenWorldChunkGenerationSystem` хранит `NativeArray<float>` + `NativeArray<NativeSurfaceSample>` для каждого загруженного чанка в `NativeHashMap<WorldChunkId, ChunkCacheEntry>`. Когда чанк unloading — dispose arrays.

### Synchronous fallback
- **Вопрос:** Нужен ли синхронный путь для тестов или editor?
- **Решение:** Нет. Весь код должен работать через async pipeline. Для unit tests — schedule job + `Complete()` в том же методе.

### Water level
- **Вопрос:** Где хранить water level?
- **Решение:** В `OpenWorldChunkGenerationRuntime` (resource). Раньше был `const` в `LpgHeightLayer`.

### Biome generation
- **Вопрос:** Что делать с `LpgBiomeLayer`?
- **Решение:** В текущей формуле `LpgSurfaceLayer.Sample()` используется `LpgBiomeLayer.GetBiomeId()`, но детали biome generation не видны в текущем codebase. Для MVP — использовать simplified biome в `SurfaceSamplingJob` (например, `biomeId = height > 16 ? 3 : height > 8 ? 2 : 1`), либо оставить `BiomeId = 0` как placeholder до отдельной задачи.

---

## 7. Файлы для прикрепления к контексту (context attachments)

Для работы над этой задачей в отдельном контексте (например, другому агенту или при передаче в новый чат) необходимо приложить следующие файлы:

### Обязательные (архитектура + контракты)
1. `AGENTS.md` — правила проекта
2. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Contracts/Definitions/WorldGenerationRequest.cs`
3. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Contracts/Definitions/WorldChunkBounds.cs`
4. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Contracts/Ids/WorldChunkId.cs`
5. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Contracts/Domain/SurfaceSample.cs`
6. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Contracts/Domain/TerrainMeshData.cs`
7. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Contracts/Domain/GeneratedChunkData.cs`
8. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Contracts/Events/OpenWorldChunkLoadRequested.cs`
9. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Contracts/Events/OpenWorldChunkUnloadEvent.cs`
10. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Contracts/Definitions/ResourcePlacement.cs`
11. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Contracts/Definitions/SpawnPlacement.cs`

### Текущая реализация (что заменяем)
12. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/LayerProcGen/LayerProcGenWorldGenerationService.cs`
13. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/LayerProcGen/LpgHeightLayer.cs`
14. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/LayerProcGen/LpgSurfaceLayer.cs`
15. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/LayerProcGen/LpgTerrainMeshLayer.cs`
16. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/Domain/TerrainMeshBuilder.cs`
17. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/Domain/OpenWorldPlacementGenerator.cs`
18. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/Domain/SimpleWorldGenerationService.cs`
19. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/Domain/SimpleSurfaceSampler.cs`

### Integration points (куда встраивать)
20. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Presentation/WorldResources/OpenWorldTerrainRuntime.cs`
21. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Presentation/Systems/Client/ClientOpenWorldTerrainStreamingSystem.cs`
22. `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Logic/Systems/Server/ServerOpenWorldResourceNodeSeedSystem.cs`
23. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/Systems/Server/ServerOpenWorldChunkInterestSystem.cs`
24. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/WorldResources/OpenWorldGenerationServerRuntime.cs`
25. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/LayerProcGen/OpenWorldGenerationLayerProcGenFeature.cs`
26. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Presentation/OpenWorldGenerationPresentationFeature.cs`

### Asmdef / manifest
27. `Packages/manifest.json`
28. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Contracts/StaticMlp.Features.OpenWorldGeneration.Contracts.asmdef`
29. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/StaticMlp.Features.OpenWorldGeneration.Logic.asmdef`
30. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Presentation/StaticMlp.Features.OpenWorldGeneration.Presentation.asmdef`
31. `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/LayerProcGen/StaticMlp.Features.OpenWorldGeneration.LayerProcGen.asmdef`

### Reference (Burst solution — что заимствовать)
32. `Z:/_UnityProjects/Kimi_Agent_LayerProcGen Liteweight/LayerProcGen.Burst/Runtime/Core/LayerProcGenMath.cs`
33. `Z:/_UnityProjects/Kimi_Agent_LayerProcGen Liteweight/LayerProcGen.Burst/Runtime/Core/ChunkId.cs`
34. `Z:/_UnityProjects/Kimi_Agent_LayerProcGen Liteweight/LayerProcGen.Burst/Runtime/Jobs/ChunkGenerationJobs.cs`
35. `Z:/_UnityProjects/Kimi_Agent_LayerProcGen Liteweight/LayerProcGen.Burst/Runtime/Layers/LayerBase.cs`
36. `Z:/_UnityProjects/Kimi_Agent_LayerProcGen Liteweight/LayerProcGen.Burst/Runtime/Generation/GenerationScheduler.cs`

### Документация
37. `ai/ECS_Feature_Architecture_Layout_StaticEcs.md`
38. `ai/gameplay_systems_memo.md`
39. `ai/static_ecs_reference.md`
40. `ai/networked_feature_recipes.md`
41. `.agents/skills/staticmlp-code-writing/SKILL.md`
