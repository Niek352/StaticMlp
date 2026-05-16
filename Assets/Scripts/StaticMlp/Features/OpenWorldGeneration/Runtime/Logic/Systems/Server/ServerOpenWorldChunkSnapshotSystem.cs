using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class ServerOpenWorldChunkSnapshotSystem : ISystem
    {
        public void Update()
        {
            var state = SW.GetResource<OpenWorldChunkStreamingState>();
            for (var i = 0; i < state.PendingSnapshots.Count; i++)
            {
                var request = state.PendingSnapshots[i];
                ReplicationSnapshotBroadcaster.SendClusterSnapshot(request.Peer, request.ClusterId);
            }

            state.PendingSnapshots.Clear();
        }
    }
}
