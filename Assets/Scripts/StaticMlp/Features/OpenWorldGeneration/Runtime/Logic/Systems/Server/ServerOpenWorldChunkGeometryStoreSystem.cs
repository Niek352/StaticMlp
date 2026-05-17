using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class ServerOpenWorldChunkGeometryStoreSystem : ISystem
    {
        private EventReceiver<ServerWT, OpenWorldChunkGenerationCompleted> _completed;

        public void Init()
        {
            _completed = SW.RegisterEventReceiver<OpenWorldChunkGenerationCompleted>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _completed);
        }

        public void Update()
        {
            var runtime = SW.GetResource<OpenWorldServerChunkGeometryRuntime>();

            foreach (var evt in _completed)
            {
                if (evt.Value.PhysicsMesh == null && evt.Value.NavMeshSourceMesh == null)
                    continue;

                runtime.Set(
                    evt.Value.ChunkId,
                    evt.Value.Lod,
                    evt.Value.PhysicsMesh,
                    evt.Value.NavMeshSourceMesh);
            }
        }
    }
}
