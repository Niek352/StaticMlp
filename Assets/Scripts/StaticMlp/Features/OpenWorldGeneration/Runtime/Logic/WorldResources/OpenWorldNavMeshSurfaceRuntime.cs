using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldNavMeshSurfaceRuntime : IResource, IDisposable
    {
        private const int AGENT_TYPE_ID = 0;
        private const float NAV_MESH_BOUNDS_Y = 1000f;

        private readonly Dictionary<WorldChunkId, SurfaceState> _surfaces = new();

        public void BuildAndAdd(WorldChunkId chunkId, TerrainMeshData meshData, float chunkWorldSize)
        {
            CompleteTrackedNavMeshJobs();

            if (_surfaces.ContainsKey(chunkId))
                Remove(chunkId);

            var navMeshData = BuildNavMeshData(meshData, chunkId, chunkWorldSize);
            var position = new Vector3(chunkId.X * chunkWorldSize, 0f, chunkId.Z * chunkWorldSize);
            var instance = NavMesh.AddNavMeshData(navMeshData, position, Quaternion.identity);
            
            _surfaces[chunkId] = new SurfaceState(navMeshData, instance);
        }

        public void Remove(WorldChunkId chunkId)
        {
            CompleteTrackedNavMeshJobs();

            if (!_surfaces.TryGetValue(chunkId, out var state))
                return;

            state.Instance.Remove();

            if (state.NavMeshData != null)
                UnityEngine.Object.Destroy(state.NavMeshData);

            _surfaces.Remove(chunkId);
        }

        public void Dispose()
        {
            CompleteTrackedNavMeshJobs();

            foreach (var pair in _surfaces)
            {
                pair.Value.Instance.Remove();

                if (pair.Value.NavMeshData != null)
                    UnityEngine.Object.Destroy(pair.Value.NavMeshData);
            }

            _surfaces.Clear();
        }

        private static NavMeshData BuildNavMeshData(TerrainMeshData meshData, WorldChunkId chunkId, float chunkWorldSize)
        {
            var mesh = new Mesh
            {
                vertices = meshData.Vertices,
                triangles = meshData.Triangles,
            };
            mesh.RecalculateBounds();

            var sources = new List<NavMeshBuildSource>(1)
            {
                new()
                {
                    shape = NavMeshBuildSourceShape.Mesh,
                    sourceObject = mesh,
                    transform = Matrix4x4.identity,
                    area = 0,
                }
            };

            var bounds = meshData.Bounds;
            bounds.Expand(new Vector3(0f, 1f, 0f));

            var navMeshData = NavMeshBuilder.BuildNavMeshData(
                NavMesh.GetSettingsByID(AGENT_TYPE_ID),
                sources,
                bounds,
                Vector3.zero,
                Quaternion.identity);

            UnityEngine.Object.Destroy(mesh);

            if (navMeshData == null)
                throw new InvalidOperationException($"Failed to build NavMeshData for chunk {chunkId}.");

            return navMeshData;
        }

        private static void CompleteTrackedNavMeshJobs()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
                throw new InvalidOperationException("Default Unity.Entities world is required before mutating runtime NavMesh surfaces.");

            world.EntityManager.CompleteAllTrackedJobs();
        }

        private readonly struct SurfaceState
        {
            public readonly NavMeshData NavMeshData;
            public readonly NavMeshDataInstance Instance;

            public SurfaceState(NavMeshData navMeshData, NavMeshDataInstance instance)
            {
                NavMeshData = navMeshData;
                Instance = instance;
            }
        }
    }
}
