using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking.Diagnostics;
using UnityEngine;

namespace StaticMlp.Networking.Replication {
    public static class PacketCodec {
        public static byte[] EncodeHello() {
            var writer = BinaryPackWriter.CreateFromPool();
            writer.WriteByte((byte)NetPacketType.Hello);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static byte[] EncodeWelcome(NetworkPeerId peer) {
            var writer = BinaryPackWriter.CreateFromPool();
            writer.WriteByte((byte)NetPacketType.Welcome);
            writer.WriteUshort(peer.Value);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static byte[] EncodeSpawn(SpawnMessage spawn) {
            var writer = BinaryPackWriter.CreateFromPool();
            writer.WriteByte((byte)NetPacketType.Spawn);
            WriteGid(ref writer, spawn.Gid);
            writer.WriteByte(spawn.EntityType);
            writer.WriteUshort(spawn.NetworkSchemaVersion);
            writer.WriteUshort(spawn.Owner.Value);
            writer.WriteByte((byte)spawn.Authority);
            writer.WriteUshort(spawn.NetworkArchetypeId);
            WriteBytes(ref writer, spawn.SnapshotPayload);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static byte[] EncodeDespawn(DespawnMessage despawn) {
            var writer = BinaryPackWriter.CreateFromPool();
            writer.WriteByte((byte)NetPacketType.Despawn);
            WriteGid(ref writer, despawn.Gid);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static byte[] EncodeOwnershipChanged(OwnershipChangedMessage msg) {
            var writer = BinaryPackWriter.CreateFromPool();
            writer.WriteByte((byte)NetPacketType.OwnershipChanged);
            WriteGid(ref writer, msg.Gid);
            writer.WriteUshort(msg.NewOwner.Value);
            writer.WriteByte((byte)msg.Authority);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static byte[] EncodeSnapshot(ReplicationSnapshotMessage msg) {
            var writer = BinaryPackWriter.CreateFromPool();
            writer.WriteByte((byte)NetPacketType.Snapshot);
            writer.WriteByte((byte)msg.Kind);
            writer.WriteUshort(msg.ClusterId);
            writer.WriteUint(msg.ChunkIdx);
            WriteChunkIds(ref writer, msg.ChunkIds);
            writer.WriteBool(msg.Gzip);
            WriteBytes(ref writer, msg.Payload);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static byte[] EncodeChunkLease(ChunkLeaseMessage msg) {
            var writer = BinaryPackWriter.CreateFromPool();
            writer.WriteByte((byte)NetPacketType.ChunkLease);
            writer.WriteUshort((ushort)(msg.ChunkIds?.Length ?? 0));
            if (msg.ChunkIds != null) {
                for (var i = 0; i < msg.ChunkIds.Length; i++)
                    writer.WriteUint(msg.ChunkIds[i]);
            }
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static byte[] EncodeEntitySnapshotBatch(EntitySnapshotBatch batch) {
            var writer = BinaryPackWriter.CreateFromPool();
            writer.WriteByte((byte)NetPacketType.EntitySnapshotBatch);
            writer.WriteUshort(batch.SourcePeer.Value);
            writer.WriteUshort((ushort)batch.Payloads.Count);
            for (var i = 0; i < batch.Payloads.Count; i++)
                WriteBytes(ref writer, batch.Payloads[i]);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static byte[] EncodeNetworkEventBatch(NetworkEventBatch batch) {
            var writer = NetworkWriter.CreateFromPool();
            writer.WriteByte((byte)NetPacketType.NetworkEventBatch);
            writer.WriteUshort((ushort)batch.Events.Count);
            for (var i = 0; i < batch.Events.Count; i++) {
                var packet = batch.Events[i];
                writer.WriteUshort(packet.EventTypeId);
                var payloadLengthPosition = writer.Position;
                writer.WriteUint(0);
                var payloadStart = writer.Position;
                packet.Payload.Write(ref writer);
                writer.WriteUintAt(payloadLengthPosition, writer.Position - payloadStart);
            }
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static bool Decode(NetworkPeerId sourcePeer, byte[] payload, NetInbox inbox) {
            if (payload == null || payload.Length == 0)
                return false;

            try {
                NetworkTrafficProfiler.RecordIncomingPacket(sourcePeer, payload);
                var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
                var type = (NetPacketType)reader.ReadByte();

                switch (type) {
                    case NetPacketType.Welcome:
                        var welcomePeer = new NetworkPeerId(reader.ReadUshort());
                        NetworkRuntime.LocalPeerId = welcomePeer;
                        return true;
                    case NetPacketType.Spawn:
                        inbox.Spawns.Add(ReadSpawn(ref reader));
                        return true;
                    case NetPacketType.Despawn:
                        inbox.Despawns.Add(new DespawnMessage(ReadGid(ref reader)));
                        return true;
                    case NetPacketType.OwnershipChanged:
                        inbox.OwnershipChanges.Add(new OwnershipChangedMessage(
                            ReadGid(ref reader),
                            new NetworkPeerId(reader.ReadUshort()),
                            (NetworkAuthority)reader.ReadByte()
                        ));
                        return true;
                    case NetPacketType.Snapshot:
                        var snapshot = ReadSnapshot(ref reader);
                        snapshot.ReceiveOrder = inbox.NextReceiveOrder();
                        inbox.Snapshots.Add(snapshot);
                        return true;
                    case NetPacketType.ChunkLease:
                        inbox.ChunkLeases.Add(ReadChunkLease(ref reader));
                        return true;
                    case NetPacketType.EntitySnapshotBatch:
                        var batch = ReadEntitySnapshotBatch(ref reader, sourcePeer);
                        inbox.EntitySnapshotBatches.Add(batch);
                        return true;
                    case NetPacketType.NetworkEvent:
                        return TryReadNetworkEvent(ref reader, sourcePeer, inbox);
                    case NetPacketType.NetworkEventBatch:
                        return TryReadNetworkEventBatch(ref reader, sourcePeer, inbox);
                    default:
                        Debug.LogWarning($"[PacketCodec] Unknown packet type: {type}");
                        return true;
                }
            } catch (Exception ex) {
                Debug.LogError($"[PacketCodec] Decode exception");
                Debug.LogException(ex);
                return false;
            }
        }

        private static SpawnMessage ReadSpawn(ref BinaryPackReader reader) {
            return new SpawnMessage {
                Gid = ReadGid(ref reader),
                EntityType = reader.ReadByte(),
                NetworkSchemaVersion = reader.ReadUshort(),
                Owner = new NetworkPeerId(reader.ReadUshort()),
                Authority = (NetworkAuthority)reader.ReadByte(),
                NetworkArchetypeId = reader.ReadUshort(),
                SnapshotPayload = ReadBytes(ref reader)
            };
        }

        private static ReplicationSnapshotMessage ReadSnapshot(ref BinaryPackReader reader) {
            return new ReplicationSnapshotMessage {
                Kind = (ReplicationSnapshotKind)reader.ReadByte(),
                ClusterId = reader.ReadUshort(),
                ChunkIdx = reader.ReadUint(),
                ChunkIds = ReadChunkIds(ref reader),
                Gzip = reader.ReadBool(),
                Payload = ReadBytes(ref reader)
            };
        }

        private static ChunkLeaseMessage ReadChunkLease(ref BinaryPackReader reader) {
            var count = reader.ReadUshort();
            var chunks = new uint[count];
            for (var i = 0; i < chunks.Length; i++)
                chunks[i] = reader.ReadUint();

            return new ChunkLeaseMessage(chunks);
        }

        private static void WriteChunkIds(ref BinaryPackWriter writer, uint[] chunkIds) {
            if (chunkIds == null || chunkIds.Length == 0) {
                writer.WriteUshort(0);
                return;
            }

            var count = chunkIds.Length;
            if (count > ushort.MaxValue)
                throw new InvalidOperationException($"Snapshot chunk list is too large: {count}.");

            writer.WriteUshort((ushort)count);
            for (var i = 0; i < count; i++)
                writer.WriteUint(chunkIds[i]);
        }

        private static uint[] ReadChunkIds(ref BinaryPackReader reader) {
            var count = reader.ReadUshort();
            if (count == 0)
                return Array.Empty<uint>();

            var chunkIds = new uint[count];
            for (var i = 0; i < chunkIds.Length; i++)
                chunkIds[i] = reader.ReadUint();

            return chunkIds;
        }

        private static EntitySnapshotBatch ReadEntitySnapshotBatch(ref BinaryPackReader reader, NetworkPeerId fallbackSource) {
            // Source peer is authoritative only at the transport boundary.
            reader.ReadUshort();
            var batch = new EntitySnapshotBatch {
                SourcePeer = fallbackSource,
                Payloads = new List<byte[]>()
            };

            var count = reader.ReadUshort();
            for (var i = 0; i < count; i++)
                batch.Payloads.Add(ReadBytes(ref reader));

            return batch;
        }

        private static bool TryReadNetworkEvent(ref BinaryPackReader reader, NetworkPeerId sourcePeer, NetInbox inbox) {
            var length = reader.ReadInt();
            var eventTypeId = reader.ReadUshort();
            var bytes = length > 0
                ? reader.ReadBytesAsSpan((uint)length).ToArray()
                : Array.Empty<byte>();

            if (!NetworkEventRegistry.DecodePacket(sourcePeer, eventTypeId, bytes, out var packet))
                return false;

            inbox.Events.Add(packet.WithReceiveOrder(inbox.NextReceiveOrder()));
            return true;
        }

        private static bool TryReadNetworkEventBatch(ref BinaryPackReader reader, NetworkPeerId sourcePeer, NetInbox inbox) {
            var decoded = new NetworkEventPacket[reader.ReadUshort()];
            for (var i = 0; i < decoded.Length; i++) {
                var eventTypeId = reader.ReadUshort();
                var length = reader.ReadUint();
                var bytes = length > 0
                    ? reader.ReadBytesAsSpan(length).ToArray()
                    : Array.Empty<byte>();

                if (!NetworkEventRegistry.DecodePacket(sourcePeer, eventTypeId, bytes, out decoded[i]))
                    return false;
            }

            for (var i = 0; i < decoded.Length; i++)
                inbox.Events.Add(decoded[i].WithReceiveOrder(inbox.NextReceiveOrder()));
            return true;
        }

        private static void WriteBytes(ref BinaryPackWriter writer, byte[] bytes) {
            writer.WriteInt(bytes?.Length ?? 0);
            if (bytes != null)
                writer.WriteBytes(bytes);
        }

        private static byte[] ReadBytes(ref BinaryPackReader reader) {
            var length = reader.ReadInt();
            return length > 0 ? reader.ReadBytesAsSpan((uint)length).ToArray() : Array.Empty<byte>();
        }

        private static void WriteGid(ref BinaryPackWriter writer, EntityGID gid) {
            writer.WriteUlong(gid.Raw);
        }

        private static EntityGID ReadGid(ref BinaryPackReader reader) {
            return new EntityGID(reader.ReadUlong());
        }
    }
}
