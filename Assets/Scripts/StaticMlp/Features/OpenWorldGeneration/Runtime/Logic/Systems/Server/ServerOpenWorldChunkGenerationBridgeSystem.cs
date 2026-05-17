using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldGeneration
{
    /// <summary>
    /// Bridge between existing OpenWorldChunkLoadRequested and the new async OpenWorldChunkGenerationRequested.
    /// </summary>
    public sealed class ServerOpenWorldChunkGenerationBridgeSystem : ISystem
    {
        private EventReceiver<ServerWT, OpenWorldChunkLoadRequested> _loadRequests;

        public void Init()
        {
            _loadRequests = SW.RegisterEventReceiver<OpenWorldChunkLoadRequested>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _loadRequests);
        }

        public void Update()
        {
            var runtime = SW.GetResource<OpenWorldGenerationServerRuntime>();
            foreach (var request in _loadRequests)
            {
                SW.SendEvent(new OpenWorldChunkGenerationRequested(
                    request.Value.ChunkId,
                    runtime.CreateServerGeometryRequest(),
                    GenerationOutputMask.Placements | GenerationOutputMask.PhysicsMesh));
            }
        }
    }
}
