using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Diagnostics;
using UnityEngine;

namespace StaticMlp.Networking.Replication {
    public sealed class NetOutbox : IResource {
        private const int MAX_ENTITY_SNAPSHOT_BATCH_BYTES = 1200;
        private const int ENTITY_SNAPSHOT_BATCH_HEADER_BYTES = 5;
        private const int ENTITY_SNAPSHOT_ENTRY_HEADER_BYTES = 4;
        private const int MAX_NETWORK_EVENT_BATCH_BYTES = 1200;
        private const int NETWORK_EVENT_BATCH_HEADER_BYTES = 3;
        private const int NETWORK_EVENT_ENTRY_HEADER_BYTES = 6;

        public readonly List<OutgoingPacket> Packets = new();
        private readonly List<PendingEntitySnapshotBatch> _entitySnapshotBatches = new();
        private readonly List<PendingNetworkEventBatch> _networkEventBatches = new();

        public void Enqueue(NetworkPeerId peer, byte[] payload, NetDelivery delivery) {
            NetworkTrafficProfiler.RecordOutgoingPacket(peer, delivery, payload);
            Packets.Add(new OutgoingPacket {
                Peer = peer,
                Payload = payload,
                Delivery = delivery
            });
        }

        public void EnqueueEntitySnapshot(NetworkPeerId peer, byte[] payload, NetDelivery delivery) {
            var payloadSize = payload?.Length ?? 0;
            Debug.Log($"[NetOutbox] EnqueueEntitySnapshot peer={peer.Value} size={payloadSize} delivery={delivery}");
            var batch = GetOrCreateEntitySnapshotBatch(peer, delivery, ENTITY_SNAPSHOT_ENTRY_HEADER_BYTES + payloadSize);
            batch.Batch.Payloads.Add(payload);
            batch.EncodedSize += ENTITY_SNAPSHOT_ENTRY_HEADER_BYTES + payloadSize;
        }

        internal void EnqueueNetworkEvent(in NetworkEventPacket packet) {
            Debug.Log($"[NetOutbox] EnqueueNetworkEvent typeId={packet.EventTypeId} target={packet.TargetPeer.Value} delivery={packet.Delivery}");
            var batch = GetOrCreateNetworkEventBatch(
                packet.TargetPeer,
                packet.Delivery,
                NETWORK_EVENT_ENTRY_HEADER_BYTES + packet.EstimatedPayloadSize);
            batch.Batch.Events.Add(packet);
            batch.EncodedSize += NETWORK_EVENT_ENTRY_HEADER_BYTES + packet.EstimatedPayloadSize;
        }

        public void FlushEntitySnapshotBatches() {
            foreach (var pending in _entitySnapshotBatches)
                Enqueue(pending.Peer, PacketCodec.EncodeEntitySnapshotBatch(pending.Batch), pending.Delivery);

            _entitySnapshotBatches.Clear();
        }

        public void FlushNetworkEventBatches() {
            foreach (var pending in _networkEventBatches)
                Enqueue(pending.Peer, PacketCodec.EncodeNetworkEventBatch(pending.Batch), pending.Delivery);

            _networkEventBatches.Clear();
        }

        public void Clear() {
            Packets.Clear();
            _entitySnapshotBatches.Clear();
            _networkEventBatches.Clear();
        }

        private PendingEntitySnapshotBatch GetOrCreateEntitySnapshotBatch(NetworkPeerId peer, NetDelivery delivery, int nextPayloadSize) {
            foreach (var pending in _entitySnapshotBatches) {
                if (pending.Peer == peer && pending.Delivery == delivery &&
                    pending.EncodedSize + nextPayloadSize <= MAX_ENTITY_SNAPSHOT_BATCH_BYTES)
                    return pending;
            }

            var created = new PendingEntitySnapshotBatch(peer, delivery, new EntitySnapshotBatch {
                SourcePeer = NetworkRuntime.LocalPeerId,
                Payloads = new List<byte[]>(),
            }, ENTITY_SNAPSHOT_BATCH_HEADER_BYTES);
            _entitySnapshotBatches.Add(created);
            return created;
        }

        private PendingNetworkEventBatch GetOrCreateNetworkEventBatch(NetworkPeerId peer, NetDelivery delivery, int nextPayloadSize) {
            foreach (var pending in _networkEventBatches) {
                if (pending.Peer == peer && pending.Delivery == delivery &&
                    pending.EncodedSize + nextPayloadSize <= MAX_NETWORK_EVENT_BATCH_BYTES)
                    return pending;
            }

            var created = new PendingNetworkEventBatch(peer, delivery, new NetworkEventBatch(), NETWORK_EVENT_BATCH_HEADER_BYTES);
            _networkEventBatches.Add(created);
            return created;
        }

        private sealed class PendingEntitySnapshotBatch {
            public readonly NetworkPeerId Peer;
            public readonly NetDelivery Delivery;
            public readonly EntitySnapshotBatch Batch;
            public int EncodedSize;

            public PendingEntitySnapshotBatch(NetworkPeerId peer, NetDelivery delivery, EntitySnapshotBatch batch, int encodedSize) {
                Peer = peer;
                Delivery = delivery;
                Batch = batch;
                EncodedSize = encodedSize;
            }
        }

        private sealed class PendingNetworkEventBatch {
            public readonly NetworkPeerId Peer;
            public readonly NetDelivery Delivery;
            public readonly NetworkEventBatch Batch;
            public int EncodedSize;

            public PendingNetworkEventBatch(NetworkPeerId peer, NetDelivery delivery, NetworkEventBatch batch, int encodedSize) {
                Peer = peer;
                Delivery = delivery;
                Batch = batch;
                EncodedSize = encodedSize;
            }
        }
    }
}
