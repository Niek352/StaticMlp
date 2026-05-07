using System;
using System.Linq;
using System.Reflection;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public sealed class AiNavigationRuntime : IResource, IDisposable
    {
        public enum BackendType
        {
            UnityEntities = 0
        }

        private readonly IAiNavigationBackend _backend;

        public BackendType Type { get; }

        public AiNavigationRuntime(IAiNavigationBackend backend, BackendType type)
        {
            _backend = backend ?? throw new ArgumentNullException(nameof(backend));
            Type = type;
        }

        public static AiNavigationRuntime CreateDefault()
        {
            return Create(BackendType.UnityEntities);
        }

        public static AiNavigationRuntime Create(BackendType type)
        {
            return new AiNavigationRuntime(CreateBackend(type), type);
        }

        public void BeginFrame()
        {
            _backend.BeginFrame();
        }

        public void SyncAgent(in AgentInput input)
        {
            _backend.SyncAgent(in input);
        }

        public void RemoveInactiveAgents()
        {
            _backend.RemoveInactiveAgents();
        }

        public bool TryGetResult(EntityGID gid, out AgentResult result)
        {
            return _backend.TryGetResult(gid, out result);
        }

        public bool TryGetDebugState(EntityGID gid, out DebugState debugState)
        {
            return _backend.TryGetDebugState(gid, out debugState);
        }

        public void Dispose()
        {
            _backend.Dispose();
        }

        public readonly struct AgentInput
        {
            public readonly EntityGID Gid;
            public readonly Vector3 Position;
            public readonly Quaternion Rotation;
            public readonly bool HasMoveRequest;
            public readonly Vector3 Destination;
            public readonly float StopDistance;

            public AgentInput(
                EntityGID gid,
                Vector3 position,
                Quaternion rotation,
                bool hasMoveRequest,
                Vector3 destination,
                float stopDistance)
            {
                Gid = gid;
                Position = position;
                Rotation = rotation;
                HasMoveRequest = hasMoveRequest;
                Destination = destination;
                StopDistance = stopDistance;
            }
        }

        public readonly struct AgentResult
        {
            public readonly Vector3 Position;
            public readonly Vector3 Velocity;
            public readonly Quaternion Rotation;
            public readonly bool HasArrived;
            public readonly bool HasFailed;

            public AgentResult(
                Vector3 position,
                Vector3 velocity,
                Quaternion rotation,
                bool hasArrived,
                bool hasFailed)
            {
                Position = position;
                Velocity = velocity;
                Rotation = rotation;
                HasArrived = hasArrived;
                HasFailed = hasFailed;
            }
        }

        public readonly struct DebugState
        {
            public readonly Vector3 Goal;
            public readonly bool HasGoal;
            public readonly Vector3[] PathCorners;

            public DebugState(Vector3 goal, bool hasGoal, Vector3[] pathCorners)
            {
                Goal = goal;
                HasGoal = hasGoal;
                PathCorners = pathCorners;
            }
        }

        private static IAiNavigationBackend CreateBackend(BackendType type)
        {
            return type switch
            {
                BackendType.UnityEntities => CreateBackendByTypeName(
                    "StaticMlp.Features.AiBots.UnityEntitiesAiNavigationBackend",
                    "StaticMlp.Features.AiBots.NavigationBackend.Internal"),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported AI navigation backend.")
            };
        }

        private static IAiNavigationBackend CreateBackendByTypeName(string typeName, string assemblyName)
        {
            var backendType = System.Type.GetType($"{typeName}, {assemblyName}");
            if (backendType == null)
            {
                backendType = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(a => string.Equals(a.GetName().Name, assemblyName, StringComparison.Ordinal))
                    .Select(a => a.GetType(typeName, throwOnError: false))
                    .FirstOrDefault(t => t != null);
            }

            if (backendType == null)
                throw new InvalidOperationException(
                    $"AI navigation backend type '{typeName}' from assembly '{assemblyName}' was not found.");

            if (!typeof(IAiNavigationBackend).IsAssignableFrom(backendType))
                throw new InvalidOperationException(
                    $"AI navigation backend type '{backendType.FullName}' does not implement {nameof(IAiNavigationBackend)}.");

            try
            {
                return (IAiNavigationBackend)Activator.CreateInstance(backendType);
            }
            catch (TargetInvocationException e)
            {
                throw new InvalidOperationException(
                    $"Failed to create AI navigation backend '{backendType.FullName}'. See inner exception for details.",
                    e.InnerException ?? e);
            }
        }
    }
}
