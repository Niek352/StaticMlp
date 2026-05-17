using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class ServerOpenWorldResourcePlacementIndexSystem : ISystem
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
            var placementIndex = SW.GetResource<OpenWorldPlacementIndexStore>();
            var overlayStore = SW.GetResource<OpenWorldChunkOverlayStore>();

            foreach (var evt in _completed)
            {
                var chunk = evt.Value;
                if (chunk.ResourcePlacements.Length == 0)
                    continue;

                placementIndex.RegisterChunkPlacements(chunk.ChunkId, chunk.ResourcePlacements);
                for (var i = 0; i < chunk.ResourcePlacements.Length; i++)
                    overlayStore.RegisterPlacement(chunk.ChunkId, chunk.ResourcePlacements[i].PlacementId);
            }
        }
    }
}
