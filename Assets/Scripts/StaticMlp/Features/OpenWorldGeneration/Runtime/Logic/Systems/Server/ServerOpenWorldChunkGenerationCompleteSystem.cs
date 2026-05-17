using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class ServerOpenWorldChunkGenerationCompleteSystem : ISystem
    {
        private readonly List<NetworkPeerId> _peers = new();
        private EventReceiver<ServerWT, OpenWorldChunkGenerationCompleted> _completed;

        public void Init()
        {
            _completed = SW.RegisterEventReceiver<OpenWorldChunkGenerationCompleted>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _completed);
            _peers.Clear();
        }

        public void Update()
        {
            var state = SW.GetResource<OpenWorldChunkStreamingState>();
            var bounds = SW.GetResource<OpenWorldGenerationServerRuntime>().DefaultRequest.Bounds;

            foreach (var evt in _completed)
            {
                var chunkId = evt.Value.ChunkId;
                state.MarkServerLoaded(chunkId);

                _peers.Clear();
                state.CopyPeersWithChunk(chunkId, _peers);
                var clusterId = OpenWorldSpatialClusterIds.ToClusterId(chunkId, bounds);
                for (var i = 0; i < _peers.Count; i++)
                    state.QueueSnapshot(_peers[i], chunkId, clusterId);
            }

            _peers.Clear();
        }
    }
}
