using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class ServerOpenWorldChunkOverlaySendSystem : ISystem
    {
        private readonly List<NetworkPeerId> _peers = new();
        private readonly List<OpenWorldResourceOverlayState> _absoluteStates = new();
        private readonly List<OpenWorldResourceOverlayDelta> _deltas = new();

        public void Destroy()
        {
            _peers.Clear();
            _absoluteStates.Clear();
            _deltas.Clear();
        }

        public void Update()
        {
            var streamingState = SW.GetResource<OpenWorldChunkStreamingState>();
            var overlayStore = SW.GetResource<OpenWorldChunkOverlayStore>();
            var peerState = SW.GetResource<OpenWorldPeerChunkOverlayState>();
            var runtime = SW.GetResource<OpenWorldGenerationServerRuntime>();
            var remainingChunks = runtime.MaxClusterSnapshotsPerFrame;

            _peers.Clear();
            streamingState.CopyTrackedPeers(_peers);

            for (var i = 0; i < _peers.Count && remainingChunks > 0; i++)
            {
                var peer = _peers[i];
                foreach (var chunkId in streamingState.LoadedChunksFor(peer))
                {
                    if (remainingChunks <= 0)
                        break;
                    if (!overlayStore.TryGet(chunkId, out var overlay) || overlay.Revision == 0)
                        continue;

                    var ackedRevision = peerState.GetAckedRevision(peer, chunkId);
                    if (ackedRevision == overlay.Revision)
                        continue;

                    if (ackedRevision == 0 || !TrySendDelta(peer, overlay, ackedRevision))
                        SendAbsolute(peer, overlay);

                    remainingChunks--;
                }
            }

            _peers.Clear();
        }

        private bool TrySendDelta(NetworkPeerId peer, OpenWorldChunkOverlay overlay, uint ackedRevision)
        {
            _deltas.Clear();
            if (!overlay.CopyResourceDeltasSince(ackedRevision, _deltas) || _deltas.Count == 0)
                return false;

            SW.SendToPeer(peer, new OpenWorldChunkOverlayDelta
            {
                ChunkId = overlay.ChunkId,
                BasisRevision = ackedRevision,
                Revision = overlay.Revision,
                ResourceDeltas = _deltas.ToArray()
            });
            _deltas.Clear();
            return true;
        }

        private void SendAbsolute(NetworkPeerId peer, OpenWorldChunkOverlay overlay)
        {
            _absoluteStates.Clear();
            overlay.CopyResourceStates(_absoluteStates);
            SW.SendToPeer(peer, new OpenWorldChunkOverlayAbsolute
            {
                ChunkId = overlay.ChunkId,
                Revision = overlay.Revision,
                ResourceStates = _absoluteStates.ToArray()
            });
            _absoluteStates.Clear();
        }
    }
}
