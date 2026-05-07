using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using ProjectDawn.Navigation;
using ProjectDawn.Navigation.Hybrid;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[assembly: RegisterUnityEngineComponentType(typeof(RectTransform))]

namespace StaticMlp.Features.AiBots
{
    public sealed class UnityEntitiesAiNavigationBackend : IAiNavigationBackend
    {
        private const float DEFAULT_SPEED = 3.5f;
        private const float DEFAULT_ACCELERATION = 8f;
        private const float DEFAULT_ANGULAR_SPEED_DEGREES = 120f;
        private const float DEFAULT_AGENT_RADIUS = 0.45f;
        private const float DEFAULT_AGENT_HEIGHT = 2f;
        private const float DEFAULT_MAPPING_EXTENT = 10f;
        private const float TELEPORT_DISTANCE_SQR_THRESHOLD = 1f;
        private const float STOPPED_SYNC_POSITION_SQR_EPSILON = 0.0001f;
        private const float DESTINATION_SQR_EPSILON = 0.0001f;
        private const float STOP_DISTANCE_EPSILON = 0.001f;
        private const float ROTATION_SYNC_DOT_EPSILON = 0.9999f;

        private readonly Dictionary<EntityGID, AgentState> _agents = new();
        private readonly HashSet<EntityGID> _activeAgents = new();

        public void BeginFrame()
        {
            _activeAgents.Clear();
        }

        public void SyncAgent(in AiNavigationRuntime.AgentInput input)
        {
            _activeAgents.Add(input.Gid);

            var entityManager = RequireEntityManager();
            var agentState = GetOrCreateAgentState(in input, entityManager);

            if (!input.HasMoveRequest)
            {
                SyncStoppedTransform(agentState.NavigationEntity, entityManager, input.Position, input.Rotation);
                StopNavigationAgent(agentState.NavigationEntity, entityManager);
                agentState.HasMoveRequest = false;
                agentState.HasDestinationCommand = false;
                agentState.AwaitingDestinationApply = false;
                return;
            }

            if (TryResyncTransform(agentState.NavigationEntity, entityManager, input.Position, input.Rotation))
                ResetPath(agentState.NavigationEntity, entityManager);

            RefreshPendingDestinationApply(agentState, entityManager, input.Destination);
            UpdateStoppingDistanceIfNeeded(agentState, entityManager, input.StopDistance);
            agentState.HasMoveRequest = true;

            if (NeedsDestinationSync(agentState, input.Destination))
            {
                SetDestinationDeferred(agentState.NavigationEntity, input.Destination);
                agentState.LastCommandedDestination = input.Destination;
                agentState.HasDestinationCommand = true;
                agentState.AwaitingDestinationApply = true;
            }
        }

        public void RemoveInactiveAgents()
        {
            if (_agents.Count == 0)
                return;

            var toRemove = new List<EntityGID>();
            foreach (var pair in _agents)
            {
                if (_activeAgents.Contains(pair.Key))
                    continue;

                DestroyNavigationEntity(pair.Value.NavigationEntity);
                toRemove.Add(pair.Key);
            }

            for (var i = 0; i < toRemove.Count; i++)
                _agents.Remove(toRemove[i]);
        }

        public bool TryGetResult(EntityGID gid, out AiNavigationRuntime.AgentResult result)
        {
            if (!_agents.TryGetValue(gid, out var agentState))
            {
                result = default;
                return false;
            }

            if (!TryGetEntityManager(out var entityManager) || !entityManager.Exists(agentState.NavigationEntity))
            {
                result = default;
                return false;
            }

            var transform = entityManager.GetComponentData<LocalTransform>(agentState.NavigationEntity);
            var body = entityManager.GetComponentData<AgentBody>(agentState.NavigationEntity);
            var path = entityManager.GetComponentData<NavMeshPath>(agentState.NavigationEntity);

            var hasFailed = agentState.HasMoveRequest && path.State == NavMeshPathState.Failed;
            var hasArrived = agentState.HasMoveRequest
                             && !agentState.AwaitingDestinationApply
                             && !hasFailed
                             && body.IsStopped;

            result = new AiNavigationRuntime.AgentResult(
                transform.Position,
                body.Velocity,
                transform.Rotation,
                hasArrived,
                hasFailed);
            return true;
        }

        public bool TryGetDebugState(EntityGID gid, out AiNavigationRuntime.DebugState debugState)
        {
            if (!_agents.TryGetValue(gid, out var agentState) || !agentState.HasMoveRequest)
            {
                debugState = default;
                return false;
            }

            if (!TryGetEntityManager(out var entityManager) || !entityManager.Exists(agentState.NavigationEntity))
            {
                debugState = default;
                return false;
            }

            debugState = new AiNavigationRuntime.DebugState(
                agentState.LastCommandedDestination,
                hasGoal: true,
                pathCorners: BuildPathCorners(entityManager, agentState.NavigationEntity, agentState.LastCommandedDestination));
            return true;
        }

        public void Dispose()
        {
            foreach (var pair in _agents)
                DestroyNavigationEntity(pair.Value.NavigationEntity);

            _agents.Clear();
            _activeAgents.Clear();
        }

        private static EntityManager RequireEntityManager()
        {
            if (TryGetEntityManager(out var entityManager))
                return entityManager;

            throw new InvalidOperationException(
                "Unity.Entities default world is missing. AI navigation backend can not create navigation entities.");
        }

        private static bool TryGetEntityManager(out EntityManager entityManager)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world != null)
            {
                entityManager = world.EntityManager;
                return true;
            }

            entityManager = default;
            return false;
        }

        private AgentState GetOrCreateAgentState(in AiNavigationRuntime.AgentInput input, EntityManager entityManager)
        {
            if (_agents.TryGetValue(input.Gid, out var existingState))
            {
                if (entityManager.Exists(existingState.NavigationEntity))
                    return existingState;

                existingState.NavigationEntity = CreateNavigationEntity(entityManager, input.Gid, input.Position, input.Rotation);
                existingState.HasDestinationCommand = false;
                existingState.HasMoveRequest = false;
                existingState.AwaitingDestinationApply = false;
                return existingState;
            }

            var state = new AgentState
            {
                NavigationEntity = CreateNavigationEntity(entityManager, input.Gid, input.Position, input.Rotation)
            };
            _agents.Add(input.Gid, state);
            return state;
        }

        private static Entity CreateNavigationEntity(
            EntityManager entityManager,
            EntityGID gid,
            Vector3 position,
            Quaternion rotation)
        {
            var entity = entityManager.CreateEntity();
            entityManager.SetName(entity, $"AiNavigation:{gid}");
            entityManager.AddComponentData(entity, new LocalTransform
            {
                Position = ToFloat3(position),
                Rotation = ToQuaternion(rotation),
                Scale = 1f
            });
            entityManager.AddComponentData(entity, Agent.Default);
            entityManager.AddComponentData(entity, new AgentBody
            {
                Destination = ToFloat3(position),
                IsStopped = true
            });
            entityManager.AddComponentData(entity, new AgentLocomotion
            {
                Speed = DEFAULT_SPEED,
                Acceleration = DEFAULT_ACCELERATION,
                AngularSpeed = math.radians(DEFAULT_ANGULAR_SPEED_DEGREES),
                StoppingDistance = 0.1f,
                AutoBreaking = true
            });
            entityManager.AddComponentData(entity, new AgentShape
            {
                Radius = DEFAULT_AGENT_RADIUS,
                Height = DEFAULT_AGENT_HEIGHT,
                Type = ShapeType.Cylinder
            });
            entityManager.AddComponentData(entity, AgentCollider.Default);
            entityManager.AddComponentData(entity, new NavMeshPath
            {
                State = NavMeshPath.Default.State,
                AgentTypeId = 0,
                AreaMask = -1,
                AutoRepath = true,
                Grounded = true,
                MappingExtent = new float3(DEFAULT_MAPPING_EXTENT, DEFAULT_MAPPING_EXTENT, DEFAULT_MAPPING_EXTENT)
            });
            entityManager.AddBuffer<NavMeshNode>(entity);
            return entity;
        }

        private static bool TryResyncTransform(
            Entity entity,
            EntityManager entityManager,
            Vector3 position,
            Quaternion rotation)
        {
            var transform = entityManager.GetComponentData<LocalTransform>(entity);
            var desiredPosition = ToFloat3(position);
            if (math.distancesq(transform.Position, desiredPosition) <= TELEPORT_DISTANCE_SQR_THRESHOLD)
                return false;

            transform.Position = desiredPosition;
            transform.Rotation = ToQuaternion(rotation);
            entityManager.SetComponentData(entity, transform);

            var body = entityManager.GetComponentData<AgentBody>(entity);
            body.Velocity = float3.zero;
            entityManager.SetComponentData(entity, body);
            return true;
        }

        private static void SyncStoppedTransform(
            Entity entity,
            EntityManager entityManager,
            Vector3 position,
            Quaternion rotation)
        {
            var transform = entityManager.GetComponentData<LocalTransform>(entity);
            var desiredPosition = ToFloat3(position);
            var desiredRotation = ToQuaternion(rotation);
            var hasPositionDrift = math.distancesq(transform.Position, desiredPosition) > STOPPED_SYNC_POSITION_SQR_EPSILON;
            var hasRotationDrift = math.abs(math.dot(transform.Rotation.value, desiredRotation.value)) < ROTATION_SYNC_DOT_EPSILON;
            if (!hasPositionDrift && !hasRotationDrift)
                return;

            transform.Position = desiredPosition;
            transform.Rotation = desiredRotation;
            entityManager.SetComponentData(entity, transform);
        }

        private static void ResetPath(Entity entity, EntityManager entityManager)
        {
            var path = entityManager.GetComponentData<NavMeshPath>(entity);
            path.Location = default;
            path.State = NavMeshPathState.WaitingNewPath;
            entityManager.SetComponentData(entity, path);
            entityManager.GetBuffer<NavMeshNode>(entity).Clear();
        }

        private static void StopNavigationAgent(Entity entity, EntityManager entityManager)
        {
            var body = entityManager.GetComponentData<AgentBody>(entity);
            if (body.IsStopped && math.lengthsq(body.Velocity) <= 0f)
                return;

            body.Stop();
            entityManager.SetComponentData(entity, body);
        }

        private static bool NeedsDestinationSync(AgentState agentState, Vector3 destination)
        {
            if (!agentState.HasDestinationCommand)
                return true;

            return (agentState.LastCommandedDestination - destination).sqrMagnitude > DESTINATION_SQR_EPSILON;
        }

        private static void RefreshPendingDestinationApply(
            AgentState agentState,
            EntityManager entityManager,
            Vector3 destination)
        {
            if (!agentState.AwaitingDestinationApply)
                return;

            var body = entityManager.GetComponentData<AgentBody>(agentState.NavigationEntity);
            if (!body.IsStopped)
            {
                agentState.AwaitingDestinationApply = false;
                return;
            }

            if (math.distancesq(body.Destination, ToFloat3(destination)) <= DESTINATION_SQR_EPSILON)
                agentState.AwaitingDestinationApply = false;
        }

        private static void UpdateStoppingDistanceIfNeeded(
            AgentState agentState,
            EntityManager entityManager,
            float stopDistance)
        {
            if (Mathf.Abs(agentState.LastStopDistance - stopDistance) <= STOP_DISTANCE_EPSILON)
                return;

            var locomotion = entityManager.GetComponentData<AgentLocomotion>(agentState.NavigationEntity);
            locomotion.StoppingDistance = stopDistance;
            entityManager.SetComponentData(agentState.NavigationEntity, locomotion);
            agentState.LastStopDistance = stopDistance;
        }

        private static void SetDestinationDeferred(Entity entity, Vector3 destination)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
                throw new InvalidOperationException(
                    "Unity.Entities default world is missing. AI navigation backend can not queue destinations.");

            var system = world.GetOrCreateSystem<AgentSetDestinationDeferredSystem>();
            var singleton = world.EntityManager.GetComponentData<AgentSetDestinationDeferredSystem.Singleton>(system);
            singleton.SetDestinationDeferred(entity, ToFloat3(destination));
        }

        private static void DestroyNavigationEntity(Entity entity)
        {
            if (!TryGetEntityManager(out var entityManager))
                return;

            if (entityManager.Exists(entity))
                entityManager.DestroyEntity(entity);
        }

        private static Vector3[] BuildPathCorners(EntityManager entityManager, Entity entity, Vector3 destination)
        {
            using var corners = new NavMeshCorners(16, entity, Allocator.Temp);
            if (!corners.TryGetCorners(out var locations) || locations.Length == 0)
                return Array.Empty<Vector3>();

            var result = new Vector3[locations.Length];
            for (var i = 0; i < locations.Length; i++)
                result[i] = ToVector3(locations[i].position);

            if ((result[^1] - destination).sqrMagnitude > DESTINATION_SQR_EPSILON)
            {
                Array.Resize(ref result, result.Length + 1);
                result[^1] = destination;
            }

            return result;
        }

        private static float3 ToFloat3(Vector3 value)
        {
            return new float3(value.x, value.y, value.z);
        }

        private static quaternion ToQuaternion(Quaternion value)
        {
            return new quaternion(value.x, value.y, value.z, value.w);
        }

        private static Vector3 ToVector3(float3 value)
        {
            return new Vector3(value.x, value.y, value.z);
        }

        private sealed class AgentState
        {
            public Entity NavigationEntity { get; set; }
            public Vector3 LastCommandedDestination { get; set; }
            public float LastStopDistance { get; set; }
            public bool HasDestinationCommand { get; set; }
            public bool HasMoveRequest { get; set; }
            public bool AwaitingDestinationApply { get; set; }
        }
    }
}
