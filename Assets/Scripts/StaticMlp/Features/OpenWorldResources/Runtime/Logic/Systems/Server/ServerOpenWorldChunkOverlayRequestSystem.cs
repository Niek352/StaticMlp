using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class ServerOpenWorldChunkOverlayRequestSystem : ISystem
    {
        private EventReceiver<ServerWT, NetworkEventFromClient<OpenWorldChunkOverlayRequest>> _requests;
        private EventReceiver<ServerWT, NetworkEventFromClient<OpenWorldChunkOverlayAck>> _acks;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<NetworkEventFromClient<OpenWorldChunkOverlayRequest>>();
            _acks = SW.RegisterEventReceiver<NetworkEventFromClient<OpenWorldChunkOverlayAck>>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _requests);
            SW.DeleteEventReceiver(ref _acks);
        }

        public void Update()
        {
            var overlayStore = SW.GetResource<OpenWorldChunkOverlayStore>();
            var peerState = SW.GetResource<OpenWorldPeerChunkOverlayState>();

            foreach (var evt in _requests)
            {
                var request = evt.Value.Value;
                var knownRevision = ClampToServerRevision(overlayStore, request.ChunkId, request.KnownRevision);
                peerState.SetAckedRevision(evt.Value.SourcePeer, request.ChunkId, knownRevision);
            }

            foreach (var evt in _acks)
            {
                var ack = evt.Value.Value;
                var ackedRevision = ClampToServerRevision(overlayStore, ack.ChunkId, ack.Revision);
                peerState.SetAckedRevision(evt.Value.SourcePeer, ack.ChunkId, ackedRevision);
            }
        }

        private static uint ClampToServerRevision(
            OpenWorldChunkOverlayStore overlayStore,
            WorldChunkId chunkId,
            uint clientRevision)
        {
            var serverRevision = overlayStore.GetRevision(chunkId);
            return clientRevision > serverRevision ? serverRevision : clientRevision;
        }
    }
}
