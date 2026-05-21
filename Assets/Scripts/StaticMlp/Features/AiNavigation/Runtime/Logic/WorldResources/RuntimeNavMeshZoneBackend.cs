using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using UnityEngine;
using UnityEngine.AI;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class RuntimeNavMeshZoneBackend : IResource, IDisposable
    {
        private const int AGENT_TYPE_ID = 0;

        private readonly Dictionary<EntityGID, ZoneEntry> _zones = new();
        private readonly List<EntityGID> _zonesToRemove = new();

        public int ActiveZoneCount => _zones.Count;
        public int TotalBuildStarts { get; private set; }

        public bool IsBuilding(EntityGID zoneId)
        {
            return _zones.TryGetValue(zoneId, out var entry)
                   && entry.Operation != null
                   && !entry.Operation.isDone;
        }

        public void StartBuild(
            EntityGID zoneId,
            in BuildInput input,
            ChunkNavSourceRegistry sourceRegistry)
        {
            if (sourceRegistry == null)
                throw new ArgumentNullException(nameof(sourceRegistry));
            if (input.SourceSetVersion == 0ul)
                throw new InvalidOperationException("Runtime NavMesh build requires a non-empty source set.");

            var entry = GetOrCreateZone(zoneId);
            if (entry.Operation != null && !entry.Operation.isDone)
                throw new InvalidOperationException($"Runtime NavMesh zone {zoneId} already has an active build.");

            var sources = new List<NavMeshBuildSource>(64);
            sourceRegistry.CollectSources(input.SourceCollectBounds, sources);
            if (sources.Count == 0)
                throw new InvalidOperationException($"Runtime NavMesh zone {zoneId} has no source geometry for requested build.");

            CompleteTrackedNavMeshJobs();
            entry.Operation = NavMeshBuilder.UpdateNavMeshDataAsync(
                entry.NavMeshData,
                NavMesh.GetSettingsByID(AGENT_TYPE_ID),
                sources,
                input.BuildBounds);
            entry.TargetNavVersion = input.NavVersion;
            entry.TargetSourceSetVersion = input.SourceSetVersion;
            entry.RemoveWhenBuildCompletes = false;
            TotalBuildStarts++;
        }

        public void CollectCompletedBuilds(List<BuildResult> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            _zonesToRemove.Clear();

            foreach (var pair in _zones)
            {
                var entry = pair.Value;
                if (entry.Operation == null || !entry.Operation.isDone)
                    continue;

                entry.Operation = null;
                if (entry.RemoveWhenBuildCompletes)
                {
                    _zonesToRemove.Add(pair.Key);
                    continue;
                }

                results.Add(new BuildResult(
                    pair.Key,
                    entry.TargetNavVersion,
                    entry.TargetSourceSetVersion));
            }

            for (var i = 0; i < _zonesToRemove.Count; i++)
                RemoveNow(_zonesToRemove[i]);

            _zonesToRemove.Clear();
        }

        public void Remove(EntityGID zoneId)
        {
            if (!_zones.TryGetValue(zoneId, out var entry))
                return;

            if (entry.Operation != null && !entry.Operation.isDone)
            {
                entry.RemoveWhenBuildCompletes = true;
                return;
            }

            RemoveNow(zoneId);
        }

        public void Dispose()
        {
            _zonesToRemove.Clear();
            foreach (var pair in _zones)
                _zonesToRemove.Add(pair.Key);

            for (var i = 0; i < _zonesToRemove.Count; i++)
                RemoveNow(_zonesToRemove[i]);

            _zonesToRemove.Clear();
        }

        private ZoneEntry GetOrCreateZone(EntityGID zoneId)
        {
            if (_zones.TryGetValue(zoneId, out var entry))
                return entry;

            CompleteTrackedNavMeshJobs();
            var navMeshData = new NavMeshData(AGENT_TYPE_ID);
            var instance = NavMesh.AddNavMeshData(navMeshData, Vector3.zero, Quaternion.identity);
            entry = new ZoneEntry(navMeshData, instance);
            _zones.Add(zoneId, entry);
            return entry;
        }

        private void RemoveNow(EntityGID zoneId)
        {
            if (!_zones.TryGetValue(zoneId, out var entry))
                return;

            CompleteTrackedNavMeshJobs();
            entry.Instance.Remove();

            if (entry.NavMeshData != null)
            {
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(entry.NavMeshData);
                else
                    UnityEngine.Object.DestroyImmediate(entry.NavMeshData);
            }

            _zones.Remove(zoneId);
        }

        private static void CompleteTrackedNavMeshJobs()
        {
            var world = Unity.Entities.World.DefaultGameObjectInjectionWorld;
            if (world != null)
                world.EntityManager.CompleteAllTrackedJobs();
        }

        public readonly struct BuildInput
        {
            public BuildInput(
                Bounds buildBounds,
                Bounds sourceCollectBounds,
                int navVersion,
                ulong sourceSetVersion)
            {
                BuildBounds = buildBounds;
                SourceCollectBounds = sourceCollectBounds;
                NavVersion = navVersion;
                SourceSetVersion = sourceSetVersion;
            }

            public readonly Bounds BuildBounds;
            public readonly Bounds SourceCollectBounds;
            public readonly int NavVersion;
            public readonly ulong SourceSetVersion;
        }

        public readonly struct BuildResult
        {
            public BuildResult(EntityGID zoneId, int navVersion, ulong sourceSetVersion)
            {
                ZoneId = zoneId;
                NavVersion = navVersion;
                SourceSetVersion = sourceSetVersion;
            }

            public readonly EntityGID ZoneId;
            public readonly int NavVersion;
            public readonly ulong SourceSetVersion;
        }

        private sealed class ZoneEntry
        {
            public ZoneEntry(NavMeshData navMeshData, NavMeshDataInstance instance)
            {
                NavMeshData = navMeshData;
                Instance = instance;
            }

            public readonly NavMeshData NavMeshData;
            public readonly NavMeshDataInstance Instance;
            public AsyncOperation Operation;
            public int TargetNavVersion;
            public ulong TargetSourceSetVersion;
            public bool RemoveWhenBuildCompletes;
        }
    }
}
