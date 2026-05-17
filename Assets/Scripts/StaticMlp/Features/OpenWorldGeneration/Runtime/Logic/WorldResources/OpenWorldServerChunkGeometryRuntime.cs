using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldServerChunkGeometryRuntime : IResource, IDisposable
    {
        private readonly Dictionary<WorldChunkId, ServerChunkGeometry> _chunks = new();

        public void Set(
            WorldChunkId chunkId,
            int lod,
            TerrainMeshData physicsMesh,
            TerrainMeshData navMeshSourceMesh)
        {
            if (physicsMesh == null && navMeshSourceMesh == null)
                throw new ArgumentException("Server chunk geometry must contain physics or nav mesh data.", nameof(physicsMesh));

            _chunks[chunkId] = new ServerChunkGeometry(lod, physicsMesh, navMeshSourceMesh);
        }

        public bool TryGet(WorldChunkId chunkId, out ServerChunkGeometry geometry)
        {
            return _chunks.TryGetValue(chunkId, out geometry);
        }

        public void Remove(WorldChunkId chunkId)
        {
            _chunks.Remove(chunkId);
        }

        public void Dispose()
        {
            _chunks.Clear();
        }

        public readonly struct ServerChunkGeometry
        {
            public ServerChunkGeometry(int lod, TerrainMeshData physicsMesh, TerrainMeshData navMeshSourceMesh)
            {
                Lod = lod;
                PhysicsMesh = physicsMesh;
                NavMeshSourceMesh = navMeshSourceMesh;
            }

            public readonly int Lod;
            public readonly TerrainMeshData PhysicsMesh;
            public readonly TerrainMeshData NavMeshSourceMesh;
        }
    }
}
