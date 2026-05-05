using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;

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
            WriteDeltaList(ref writer, spawn.Components);
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
            writer.WriteBool(msg.Gzip);
            writer.WriteInt(msg.Payload?.Length ?? 0);
            if (msg.Payload != null)
                writer.WriteBytes(msg.Payload);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static byte[] EncodeComponentBatch(ComponentBatch batch) {
            var writer = BinaryPackWriter.CreateFromPool();
            writer.WriteByte((byte)NetPacketType.ComponentBatch);
            writer.WriteUshort(batch.SourcePeer.Value);
            WriteDeltaList(ref writer, batch.Deltas);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static byte[] EncodeNetworkEvent(ushort eventTypeId, byte[] payload) {
            var writer = BinaryPackWriter.CreateFromPool();
            writer.WriteByte((byte)NetPacketType.NetworkEvent);
            writer.WriteInt(payload?.Length ?? 0);
            writer.WriteUshort(eventTypeId);
            if (payload != null)
                writer.WriteBytes(payload);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static bool Decode(NetworkPeerId sourcePeer, byte[] payload, NetInbox inbox) {
            if (payload == null || payload.Length == 0)
                return false;

            var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
            var type = (NetPacketType)reader.ReadByte();

            switch (type) {
                case NetPacketType.Welcome:
                    NetworkRuntime.LocalPeerId = new NetworkPeerId(reader.ReadUshort());
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
                    inbox.Snapshots.Add(ReadSnapshot(ref reader));
                    return true;
                case NetPacketType.ComponentBatch:
                    inbox.ComponentBatches.Add(ReadComponentBatch(ref reader, sourcePeer));
                    return true;
                case NetPacketType.NetworkEvent:
                    inbox.Events.Add(ReadNetworkEvent(ref reader, sourcePeer));
                    return true;
                default:
                    return true;
            }
        }

        private static SpawnMessage ReadSpawn(ref BinaryPackReader reader) {
            var spawn = new SpawnMessage {
                Gid = ReadGid(ref reader),
                EntityType = reader.ReadByte(),
                NetworkSchemaVersion = reader.ReadUshort(),
                Owner = new NetworkPeerId(reader.ReadUshort()),
                Authority = (NetworkAuthority)reader.ReadByte(),
                NetworkArchetypeId = reader.ReadUshort()
            };
            ReadDeltaList(ref reader, spawn.Components);
            return spawn;
        }

        private static ReplicationSnapshotMessage ReadSnapshot(ref BinaryPackReader reader) {
            var msg = new ReplicationSnapshotMessage {
                Kind = (ReplicationSnapshotKind)reader.ReadByte(),
                ClusterId = reader.ReadUshort(),
                ChunkIdx = reader.ReadUint(),
                Gzip = reader.ReadBool()
            };

            var length = reader.ReadInt();
            msg.Payload = length > 0 ? reader.ReadBytesAsSpan((uint)length).ToArray() : Array.Empty<byte>();
            return msg;
        }

        private static ComponentBatch ReadComponentBatch(ref BinaryPackReader reader, NetworkPeerId fallbackSource) {
            var batch = new ComponentBatch {
                SourcePeer = new NetworkPeerId(reader.ReadUshort())
            };
            if (batch.SourcePeer.Value == 0)
                batch.SourcePeer = fallbackSource;

            ReadDeltaList(ref reader, batch.Deltas);
            return batch;
        }

        private static NetworkEventPacket ReadNetworkEvent(ref BinaryPackReader reader, NetworkPeerId sourcePeer) {
            var length = reader.ReadInt();
            var eventTypeId = reader.ReadUshort();
            var payload = length > 0
                ? reader.ReadBytesAsSpan((uint)length).ToArray()
                : Array.Empty<byte>();
            return new NetworkEventPacket(
                sourcePeer,
                NetworkRuntime.LocalPeerId,
                eventTypeId,
                default,
                payload);
        }

        private static void WriteDeltaList(ref BinaryPackWriter writer, System.Collections.Generic.List<ComponentDelta> deltas) {
            writer.WriteUshort((ushort)deltas.Count);
            foreach (var delta in deltas) {
                WriteGid(ref writer, delta.Gid);
                writer.WriteUshort(delta.ComponentTypeId);
                writer.WriteInt(delta.Payload?.Length ?? 0);
                if (delta.Payload != null)
                    writer.WriteBytes(delta.Payload);
            }
        }

        private static void ReadDeltaList(ref BinaryPackReader reader, System.Collections.Generic.List<ComponentDelta> deltas) {
            var count = reader.ReadUshort();
            for (var i = 0; i < count; i++) {
                var gid = ReadGid(ref reader);
                var typeId = reader.ReadUshort();
                var length = reader.ReadInt();
                var bytes = length > 0 ? reader.ReadBytesAsSpan((uint)length).ToArray() : Array.Empty<byte>();
                deltas.Add(new ComponentDelta(gid, typeId, bytes));
            }
        }

        private static void WriteGid(ref BinaryPackWriter writer, EntityGID gid) {
            writer.WriteUlong(gid.Raw);
        }

        private static EntityGID ReadGid(ref BinaryPackReader reader) {
            return new EntityGID(reader.ReadUlong());
        }
    }
}
