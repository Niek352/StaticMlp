using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class ServerChunkNavSourceRegistrySystem : ISystem
    {
        private EventReceiver<ServerWT, OpenWorldChunkGenerationCompleted> _completed;
        private EventReceiver<ServerWT, OpenWorldChunkUnloadEvent> _unloaded;

        public void Init()
        {
            _completed = SW.RegisterEventReceiver<OpenWorldChunkGenerationCompleted>();
            _unloaded = SW.RegisterEventReceiver<OpenWorldChunkUnloadEvent>();
        }

        public void Update()
        {
            var registry = SW.GetResource<ChunkNavSourceRegistry>();

            foreach (var evt in _completed)
            {
                if (evt.Value.NavMeshSourceMesh == null)
                    continue;

                registry.Register(
                    evt.Value.ChunkId,
                    evt.Value.ChunkWorldSize,
                    evt.Value.NavMeshSourceMesh);
            }

            foreach (var evt in _unloaded)
                registry.Unregister(evt.Value.ChunkId);
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _completed);
            SW.DeleteEventReceiver(ref _unloaded);
        }
    }
}
