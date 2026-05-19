using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class ServerOpenWorldNavMeshSurfaceSystem : ISystem
    {
        private EventReceiver<ServerWT, OpenWorldChunkGenerationCompleted> _completed;
        private readonly Queue<PendingBuild> _pendingBuilds = new();

        public void Init()
        {
            _completed = SW.RegisterEventReceiver<OpenWorldChunkGenerationCompleted>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _completed);
            _pendingBuilds.Clear();
        }

        public void Update()
        {
            var runtime = SW.GetResource<OpenWorldNavMeshSurfaceRuntime>();
            var generationRuntime = SW.GetResource<OpenWorldChunkGenerationRuntime>();
            var serverRuntime = SW.GetResource<OpenWorldGenerationServerRuntime>();

            foreach (var evt in _completed)
            {
                if (evt.Value.NavMeshSourceMesh == null)
                    continue;

                _pendingBuilds.Enqueue(new PendingBuild(
                    evt.Value.ChunkId,
                    evt.Value.NavMeshSourceMesh));
            }

            var geometryRuntime = SW.GetResource<OpenWorldServerChunkGeometryRuntime>();
            var budget = serverRuntime.MaxNavMeshBuildsPerFrame;
            for (var i = 0; i < budget && _pendingBuilds.Count > 0; i++)
            {
                var pending = _pendingBuilds.Dequeue();
                if (!geometryRuntime.TryGet(pending.ChunkId, out _))
                    continue;

                runtime.BuildAndAdd(
                    pending.ChunkId,
                    pending.MeshData,
                    generationRuntime.ChunkWorldSize);
            }
        }

        private readonly struct PendingBuild
        {
            public readonly WorldChunkId ChunkId;
            public readonly TerrainMeshData MeshData;

            public PendingBuild(WorldChunkId chunkId, TerrainMeshData meshData)
            {
                ChunkId = chunkId;
                MeshData = meshData;
            }
        }
    }
}
