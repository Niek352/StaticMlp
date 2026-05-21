using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class ChunkNavSourceRegistry : IResource, IDisposable
    {
        private const ulong FNV64_OFFSET = 14695981039346656037ul;
        private const ulong FNV64_PRIME = 1099511628211ul;

        private readonly Dictionary<WorldChunkId, ChunkSources> _chunks = new();
        private readonly List<WorldChunkId> _chunkOrder = new();

        public ulong RegistryVersion { get; private set; }
        public int ChunkCount => _chunks.Count;

        public int SourceCount
        {
            get
            {
                var count = 0;
                foreach (var pair in _chunks)
                    count += pair.Value.Sources.Length;

                return count;
            }
        }

        public void Register(WorldChunkId chunkId, float chunkWorldSize, TerrainMeshData navMeshSourceMesh)
        {
            if (chunkWorldSize <= 0f)
                throw new ArgumentOutOfRangeException(nameof(chunkWorldSize), chunkWorldSize, "Chunk world size must be positive.");
            if (navMeshSourceMesh == null)
                throw new ArgumentNullException(nameof(navMeshSourceMesh));
            if (navMeshSourceMesh.Vertices == null)
                throw new InvalidOperationException($"Chunk {chunkId} NavMesh source mesh has no vertices.");
            if (navMeshSourceMesh.Triangles == null)
                throw new InvalidOperationException($"Chunk {chunkId} NavMesh source mesh has no triangles.");

            var version = NextVersion(RegistryVersion);
            var replacement = CreateChunkSources(chunkId, chunkWorldSize, navMeshSourceMesh, version);
            if (_chunks.TryGetValue(chunkId, out var existing))
            {
                existing.Dispose();
            }
            else
            {
                InsertChunkInStableOrder(chunkId);
            }

            RegistryVersion = version;
            _chunks[chunkId] = replacement;
        }

        public void Unregister(WorldChunkId chunkId)
        {
            if (!_chunks.TryGetValue(chunkId, out var existing))
                return;

            existing.Dispose();
            _chunks.Remove(chunkId);
            _chunkOrder.Remove(chunkId);
            RegistryVersion = NextVersion(RegistryVersion);
        }

        public bool HasChunk(WorldChunkId chunkId)
        {
            return _chunks.ContainsKey(chunkId);
        }

        public ulong CalculateSourceSetVersion(Bounds sourceCollectBounds, out int sourceCount)
        {
            var hash = FNV64_OFFSET;
            sourceCount = 0;

            for (var chunkIndex = 0; chunkIndex < _chunkOrder.Count; chunkIndex++)
            {
                var chunkId = _chunkOrder[chunkIndex];
                var chunk = _chunks[chunkId];
                if (!chunk.WorldBounds.Intersects(sourceCollectBounds))
                    continue;

                sourceCount += chunk.Sources.Length;
                hash = Mix(hash, chunkId.X);
                hash = Mix(hash, chunkId.Z);
                hash = Mix(hash, chunk.Version);
                hash = Mix(hash, chunk.GeometryHash);
                hash = Mix(hash, chunk.Sources.Length);

                for (var sourceIndex = 0; sourceIndex < chunk.Sources.Length; sourceIndex++)
                    hash = Mix(hash, chunk.Sources[sourceIndex].LocalSourceIndex);
            }

            return sourceCount == 0 ? 0ul : AvoidZero(hash);
        }

        public void CollectSources(Bounds sourceCollectBounds, List<NavMeshBuildSource> destination)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            for (var chunkIndex = 0; chunkIndex < _chunkOrder.Count; chunkIndex++)
            {
                var chunkId = _chunkOrder[chunkIndex];
                var chunk = _chunks[chunkId];
                if (!chunk.WorldBounds.Intersects(sourceCollectBounds))
                    continue;

                for (var sourceIndex = 0; sourceIndex < chunk.Sources.Length; sourceIndex++)
                {
                    var source = chunk.Sources[sourceIndex];
                    destination.Add(new NavMeshBuildSource
                    {
                        shape = NavMeshBuildSourceShape.Mesh,
                        sourceObject = source.Mesh,
                        transform = source.Transform,
                        area = 0
                    });
                }
            }
        }

        public void Dispose()
        {
            foreach (var pair in _chunks)
                pair.Value.Dispose();

            _chunks.Clear();
            _chunkOrder.Clear();
            RegistryVersion = NextVersion(RegistryVersion);
        }

        private static ChunkSources CreateChunkSources(
            WorldChunkId chunkId,
            float chunkWorldSize,
            TerrainMeshData navMeshSourceMesh,
            ulong version)
        {
            var mesh = new Mesh
            {
                name = $"NavSource_Chunk_{chunkId.X}_{chunkId.Z}_0"
            };

            if (navMeshSourceMesh.Vertices.Length > ushort.MaxValue)
                mesh.indexFormat = IndexFormat.UInt32;

            mesh.vertices = navMeshSourceMesh.Vertices;
            mesh.triangles = navMeshSourceMesh.Triangles;
            mesh.RecalculateBounds();

            var origin = chunkId.GetWorldOrigin(chunkWorldSize);
            var worldBounds = navMeshSourceMesh.Bounds;
            worldBounds.center += origin;

            return new ChunkSources(
                version,
                CalculateGeometryHash(chunkId, chunkWorldSize, navMeshSourceMesh),
                worldBounds,
                new[]
                {
                    new ChunkNavSource(
                        0,
                        mesh,
                        Matrix4x4.TRS(origin, Quaternion.identity, Vector3.one))
                });
        }

        private void InsertChunkInStableOrder(WorldChunkId chunkId)
        {
            var insertIndex = _chunkOrder.Count;
            for (var i = 0; i < _chunkOrder.Count; i++)
            {
                if (CompareChunkIds(chunkId, _chunkOrder[i]) >= 0)
                    continue;

                insertIndex = i;
                break;
            }

            _chunkOrder.Insert(insertIndex, chunkId);
        }

        private static int CompareChunkIds(WorldChunkId left, WorldChunkId right)
        {
            var x = left.X.CompareTo(right.X);
            return x != 0 ? x : left.Z.CompareTo(right.Z);
        }

        private static ulong CalculateGeometryHash(WorldChunkId chunkId, float chunkWorldSize, TerrainMeshData meshData)
        {
            var hash = FNV64_OFFSET;
            hash = Mix(hash, chunkId.X);
            hash = Mix(hash, chunkId.Z);
            hash = Mix(hash, math.asuint(chunkWorldSize));
            hash = Mix(hash, meshData.Vertices.Length);
            hash = Mix(hash, meshData.Triangles.Length);
            hash = Mix(hash, meshData.Bounds.center.x);
            hash = Mix(hash, meshData.Bounds.center.y);
            hash = Mix(hash, meshData.Bounds.center.z);
            hash = Mix(hash, meshData.Bounds.size.x);
            hash = Mix(hash, meshData.Bounds.size.y);
            hash = Mix(hash, meshData.Bounds.size.z);

            for (var i = 0; i < meshData.Vertices.Length; i++)
            {
                var vertex = meshData.Vertices[i];
                hash = Mix(hash, vertex.x);
                hash = Mix(hash, vertex.y);
                hash = Mix(hash, vertex.z);
            }

            for (var i = 0; i < meshData.Triangles.Length; i++)
                hash = Mix(hash, meshData.Triangles[i]);

            return AvoidZero(hash);
        }

        private static ulong NextVersion(ulong current)
        {
            return current == ulong.MaxValue ? 1ul : current + 1ul;
        }

        private static ulong AvoidZero(ulong hash)
        {
            return hash == 0ul ? FNV64_OFFSET : hash;
        }

        private static ulong Mix(ulong hash, float value)
        {
            return Mix(hash, math.asuint(value));
        }

        private static ulong Mix(ulong hash, int value)
        {
            return Mix(hash, unchecked((uint)value));
        }

        private static ulong Mix(ulong hash, uint value)
        {
            return Mix(hash, (ulong)value);
        }

        private static ulong Mix(ulong hash, ulong value)
        {
            unchecked
            {
                hash ^= value;
                hash *= FNV64_PRIME;
                return hash;
            }
        }

        private readonly struct ChunkNavSource
        {
            public ChunkNavSource(int localSourceIndex, Mesh mesh, Matrix4x4 transform)
            {
                LocalSourceIndex = localSourceIndex;
                Mesh = mesh;
                Transform = transform;
            }

            public readonly int LocalSourceIndex;
            public readonly Mesh Mesh;
            public readonly Matrix4x4 Transform;
        }

        private readonly struct ChunkSources
        {
            public ChunkSources(ulong version, ulong geometryHash, Bounds worldBounds, ChunkNavSource[] sources)
            {
                Version = version;
                GeometryHash = geometryHash;
                WorldBounds = worldBounds;
                Sources = sources;
            }

            public readonly ulong Version;
            public readonly ulong GeometryHash;
            public readonly Bounds WorldBounds;
            public readonly ChunkNavSource[] Sources;

            public void Dispose()
            {
                for (var i = 0; i < Sources.Length; i++)
                {
                    var mesh = Sources[i].Mesh;
                    if (mesh == null)
                        continue;

                    if (Application.isPlaying)
                        UnityEngine.Object.Destroy(mesh);
                    else
                        UnityEngine.Object.DestroyImmediate(mesh);
                }
            }
        }
    }
}
