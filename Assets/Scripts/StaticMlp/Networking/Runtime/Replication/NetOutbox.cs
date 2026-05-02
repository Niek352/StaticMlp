using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public sealed class NetOutbox : IResource {
        public readonly List<OutgoingPacket> Packets = new();
        private readonly List<PendingComponentBatch> _componentBatches = new();

        public void Enqueue(NetworkPeerId peer, byte[] payload, NetDelivery delivery) {
            Packets.Add(new OutgoingPacket {
                Peer = peer,
                Payload = payload,
                Delivery = delivery
            });
        }

        public void EnqueueComponentDelta(NetworkPeerId peer, ComponentDelta delta, NetDelivery delivery) {
            var batch = GetOrCreateComponentBatch(peer, delivery);
            batch.Batch.Deltas.Add(delta);
        }

        public void FlushComponentBatches() {
            foreach (var pending in _componentBatches)
                Enqueue(pending.Peer, PacketCodec.EncodeComponentBatch(pending.Batch), pending.Delivery);

            _componentBatches.Clear();
        }

        public void Clear() {
            Packets.Clear();
            _componentBatches.Clear();
        }

        private PendingComponentBatch GetOrCreateComponentBatch(NetworkPeerId peer, NetDelivery delivery) {
            foreach (var pending in _componentBatches) {
                if (pending.Peer == peer && pending.Delivery == delivery)
                    return pending;
            }

            var created = new PendingComponentBatch(peer, delivery, new ComponentBatch {
                SourcePeer = NetworkRuntime.LocalPeerId
            });
            _componentBatches.Add(created);
            return created;
        }

        private sealed class PendingComponentBatch {
            public readonly NetworkPeerId Peer;
            public readonly NetDelivery Delivery;
            public readonly ComponentBatch Batch;

            public PendingComponentBatch(NetworkPeerId peer, NetDelivery delivery, ComponentBatch batch) {
                Peer = peer;
                Delivery = delivery;
                Batch = batch;
            }
        }
    }
}
