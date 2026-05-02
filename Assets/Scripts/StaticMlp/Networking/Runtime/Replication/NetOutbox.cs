using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public sealed class NetOutbox : IResource {
        public readonly List<OutgoingPacket> Packets = new();

        public void Enqueue(NetworkPeerId peer, byte[] payload, NetDelivery delivery) {
            Packets.Add(new OutgoingPacket {
                Peer = peer,
                Payload = payload,
                Delivery = delivery
            });
        }

        public void EnqueueComponentDelta(NetworkPeerId peer, ComponentDelta delta, NetDelivery delivery) {
            var batch = new ComponentBatch { SourcePeer = NetworkRuntime.LocalPeerId };
            batch.Deltas.Add(delta);
            Enqueue(peer, PacketCodec.EncodeComponentBatch(batch), delivery);
        }

        public void Clear() => Packets.Clear();
    }
}
