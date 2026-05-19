# AiBots: AiNavigation

Документ описывает текущую интеграцию `StaticMlp.Features.AiBots` с локальным пакетом `com.projectdawn.navigation`.

## Архитектурная граница

`AiBots` не использует ProjectDawn API напрямую из gameplay-систем. Пакет закрыт за ресурсом `AiNavigationRuntime` и интерфейсом `IAiNavigationBackend`.

Допустимые точки взаимодействия:

- gameplay/action/executor код задает движение через `AiMoveRequest`;
- `ServerAiNavigationSystem` единственный читает `AiMoveRequest` и применяет результат навигации в `CharacterNetState`;
- `AiNavigationRuntime` владеет жизненным циклом backend-а;
- `UnityEntitiesAiNavigationBackend` единственный код AiBots, который создает и мутирует `Unity.Entities` сущности ProjectDawn.

Нельзя:

- добавлять `ProjectDawn.Navigation` ссылки в ordinary gameplay/action systems;
- двигать бота прямой записью в `CharacterNetState` из executor-а, если движение должно идти по навигации;
- хранить `Unity.Entities.Entity` или `SW.Entity` между кадрами;
- создавать альтернативный navigation runtime рядом с `AiNavigationRuntime`;
- переносить navigation ownership в presentation или MonoBehaviour view parts.

## Текущий data flow

```text
Ai task/executor/system
    -> AiMoveRequest
ServerAiNavigationSystem
    -> AiNavigationRuntime.AgentInput
AiNavigationRuntime
    -> IAiNavigationBackend
UnityEntitiesAiNavigationBackend
    -> ProjectDawn ECS entity
ProjectDawn navigation systems
    -> LocalTransform / AgentBody / NavMeshPath
UnityEntitiesAiNavigationBackend
    -> AiNavigationRuntime.AgentResult
ServerAiNavigationSystem
    -> ReplicationMut.Mut<CharacterNetState>
    -> delete AiMoveRequest on arrival
Replication
    -> clients
```

`AiMoveRequest` is durable ECS state for the current movement intent:

```csharp
public struct AiMoveRequest : IComponent
{
    public Vector3 Destination;
    public float StopDistance;
}
```

If an entity has no `AiMoveRequest`, `ServerAiNavigationSystem` syncs the backend agent to the current `CharacterNetState` position and stops ProjectDawn movement.

## Server Initialization

`ServerAiRuntimeInitSystem.Init()` initializes navigation during server setup:

1. Registers `AiActionCatalog`.
2. Calls `ValidateNavigationEnvironment()`.
3. Requires a baked Unity NavMesh through `NavMesh.CalculateTriangulation()`.
4. Creates `AiNavigationRuntime.CreateDefault()`.

Missing baked NavMesh is a configuration error and throws. Do not replace this with a silent fallback; server AI navigation cannot operate without valid NavMesh data.

`StaticMlpMultiplayerBootstrap` disposes `AiNavigationRuntime` during shutdown before destroying the StaticEcs world. The backend then destroys its ProjectDawn mirror entities.

## AiNavigationRuntime API

`AiNavigationRuntime` is a StaticEcs `IResource`.

- `CreateDefault()` creates the `UnityEntities` backend.
- `BeginFrame()` clears backend per-frame active-agent tracking.
- `SyncAgent(in AgentInput input)` creates or updates the backend mirror agent for one bot.
- `RemoveInactiveAgents()` destroys backend agents that were not synced this frame.
- `TryGetResult(EntityGID gid, out AgentResult result)` reads navigation output for gameplay replication.
- `TryGetDebugState(EntityGID gid, out DebugState debugState)` exposes goal and path corners for debug visualization.
- `Dispose()` releases backend-owned ProjectDawn entities.

`AgentInput` carries the StaticMlp side of the contract:

- `Gid`: stable StaticEcs `EntityGID` used as backend lookup key.
- `Position`: current replicated/gameplay position from `CharacterNetState`.
- `Rotation`: current replicated/gameplay rotation from `CharacterNetState`.
- `HasMoveRequest`: whether navigation should move or stop.
- `Destination`: movement target if `HasMoveRequest` is true.
- `StopDistance`: accepted arrival distance.

`AgentResult` returns:

- `Position`
- `Velocity`
- `Rotation`
- `HasArrived`
- `HasFailed`

`HasArrived` deletes `AiMoveRequest` in `ServerAiNavigationSystem`. `HasFailed` is currently reported but not consumed by the system; if failure handling becomes gameplay-visible, add an explicit owner-system policy instead of hiding it inside the backend.

## ProjectDawn Backend Mapping

`UnityEntitiesAiNavigationBackend` creates one ProjectDawn ECS entity per active StaticMlp bot and names it `AiNavigation:{gid}`.

The mirror entity has:

- `LocalTransform`
- `Agent.Default`
- `AgentBody`
- `AgentLocomotion`
- `AgentShape`
- `AgentCollider.Default`
- `NavMeshPath`
- `DynamicBuffer<NavMeshNode>`

Current fixed backend defaults:

- speed: `3.5`
- acceleration: `8`
- angular speed: `120` degrees/sec
- radius: `0.45`
- height: `2`
- NavMesh `AgentTypeId`: `0`
- NavMesh `AreaMask`: `-1`
- NavMesh `MappingExtent`: `(10, 10, 10)`
- `AutoRepath`: true
- `Grounded`: true

Destination changes go through `AgentSetDestinationDeferredSystem.Singleton.SetDestinationDeferred(...)`, so the write is applied by ProjectDawn's initialization system instead of forcing an immediate component mutation while package jobs may be running.

## Movement Semantics

Each server gameplay tick:

1. `ServerAiNavigationSystem` calls `runtime.BeginFrame()`.
2. It queries `ServerOwned + AiAgentTag + CharacterNetState`.
3. For each entity, it reads optional `AiMoveRequest`.
4. It calls `runtime.SyncAgent(...)`.
5. It reads `AgentResult` if available.
6. It mutates replicated `CharacterNetState` only when position, velocity, or rotation changed beyond local epsilons.
7. It deletes `AiMoveRequest` when the backend reports arrival.
8. It calls `runtime.RemoveInactiveAgents()`.

Position resync policy:

- If there is no move request, the backend mirror is synced exactly to `CharacterNetState` and stopped.
- If ProjectDawn position drifts from `CharacterNetState` by more than the teleport threshold, the backend transform is reset and its path is cleared.
- Small ProjectDawn movement is accepted as the navigation result and replicated through `CharacterNetState`.

## Debug Visualization

`BotNavigationGizmoPart` may read `AiNavigationRuntime.TryGetDebugState(...)` for editor/debug gizmos. This is presentation-only read access. It must not issue movement commands or mutate backend state.

Debug state contains:

- `Goal`
- `HasGoal`
- `PathCorners`

Path corners are built from ProjectDawn `NavMeshCorners` and append the commanded destination if the funnel result does not end at the destination.

## How To Add Navigation-Driven Behavior

For a new AI action that moves a bot:

1. Keep selection and target resolution in the owning AI action/task systems.
2. Write or update `AiMoveRequest` on the bot entity.
3. Let `ServerAiNavigationSystem` apply movement to `CharacterNetState`.
4. Use `AiTaskExecutionTransitions` or action-owned state to react when the request disappears after arrival.

Do not call ProjectDawn API from the action. Do not mutate `CharacterNetState` from the action just to move toward a destination.

## When To Change The Backend

Change `UnityEntitiesAiNavigationBackend` only when the ProjectDawn mapping itself changes:

- different agent dimensions or locomotion constants;
- different NavMesh `AgentTypeId`, `AreaMask`, or mapping extent;
- new ProjectDawn components such as avoidance, separation, smart stop, or link traversal;
- changed arrival/failure interpretation;
- changed debug data.

If gameplay needs per-bot navigation tuning, add an explicit StaticMlp contract first and let `ServerAiNavigationSystem` pass it through `AgentInput`. Do not read arbitrary gameplay feature state from the backend.
