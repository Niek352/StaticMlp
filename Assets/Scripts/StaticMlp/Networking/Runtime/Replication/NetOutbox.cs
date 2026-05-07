using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Diagnostics;

namespace StaticMlp.Networking.Replication {
    public sealed class NetOutbox : IResource {
        private const int MAX_COMPONENT_BATCH_BYTES = 1200;
        private const int COMPONENT_BATCH_HEADER_BYTES = 5;
        private const int COMPONENT_DELTA_HEADER_BYTES = 14;

        public readonly List<OutgoingPacket> Packets = new();
        public readonly List<OutgoingPacket> NetworkEventPackets = new();
        private readonly List<PendingComponentBatch> _componentBatches = new();

        public void Enqueue(NetworkPeerId peer, byte[] payload, NetDelivery delivery) {
            NetworkTrafficProfiler.RecordOutgoingPacket(peer, delivery, payload);
            Packets.Add(new OutgoingPacket {
                Peer = peer,
                Payload = payload,
                Delivery = delivery
            });
        }

        public void EnqueueComponentDelta(NetworkPeerId peer, ComponentDelta delta, NetDelivery delivery) {
            NetworkTrafficProfiler.RecordOutgoingComponentDelta(peer, delivery, delta, "ComponentBatch");
            var batch = GetOrCreateComponentBatch(peer, delivery, EncodedDeltaSize(delta));
            batch.Batch.Deltas.Add(delta);
            batch.EncodedSize += EncodedDeltaSize(delta);
        }

        public void EnqueueNetworkEvent(NetworkPeerId peer, ushort eventTypeId, byte[] payload, NetDelivery delivery) {
            EnqueueNetworkEventPacket(peer, PacketCodec.EncodeNetworkEvent(eventTypeId, payload), delivery);
        }

        internal void EnqueueNetworkEvent(in NetworkEventPacket packet) {
            EnqueueNetworkEventPacket(
                packet.TargetPeer,
                PacketCodec.EncodeNetworkEvent(packet.EventTypeId, packet.Payload),
                packet.Delivery);
        }

        public void FlushComponentBatches() {
            foreach (var pending in _componentBatches)
                Enqueue(pending.Peer, PacketCodec.EncodeComponentBatch(pending.Batch), pending.Delivery);

            _componentBatches.Clear();
        }

        public void Clear() {
            Packets.Clear();
            NetworkEventPackets.Clear();
            _componentBatches.Clear();
        }

        private void EnqueueNetworkEventPacket(NetworkPeerId peer, byte[] payload, NetDelivery delivery) {
            NetworkTrafficProfiler.RecordOutgoingPacket(peer, delivery, payload);
            NetworkEventPackets.Add(new OutgoingPacket {
                Peer = peer,
                Payload = payload,
                Delivery = delivery
            });
        }

        private PendingComponentBatch GetOrCreateComponentBatch(NetworkPeerId peer, NetDelivery delivery, int nextDeltaSize) {
            foreach (var pending in _componentBatches) {
                if (pending.Peer == peer && pending.Delivery == delivery &&
                    pending.EncodedSize + nextDeltaSize <= MAX_COMPONENT_BATCH_BYTES)
                    return pending;
            }

            var created = new PendingComponentBatch(peer, delivery, new ComponentBatch {
                SourcePeer = NetworkRuntime.LocalPeerId
            }, COMPONENT_BATCH_HEADER_BYTES);
            _componentBatches.Add(created);
            return created;
        }

        private static int EncodedDeltaSize(ComponentDelta delta) {
            return COMPONENT_DELTA_HEADER_BYTES + (delta.Payload?.Length ?? 0);
        }

        private sealed class PendingComponentBatch {
            public readonly NetworkPeerId Peer;
            public readonly NetDelivery Delivery;
            public readonly ComponentBatch Batch;
            public int EncodedSize;

            public PendingComponentBatch(NetworkPeerId peer, NetDelivery delivery, ComponentBatch batch, int encodedSize) {
                Peer = peer;
                Delivery = delivery;
                Batch = batch;
                EncodedSize = encodedSize;
            }
        }
    }
}
