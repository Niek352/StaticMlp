using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Networking.Replication {
    public static class ReplicationRegistry {
        public static void ApplyInitialState(CW.Entity e, System.Collections.Generic.List<ComponentDelta> components) {
            foreach (var delta in components)
                ApplyDelta(e, delta);
        }

        public static void ApplyDelta(CW.Entity e, ComponentDelta delta) {
            switch (delta.ComponentTypeId) {
                case ComponentTypeIds.CharacterNetState:
                    e.Set(DeserializeCharacterNetState(delta.Payload));
                    break;
                case ComponentTypeIds.NetworkIdentity:
                    e.Set(DeserializeNetworkIdentity(delta.Payload));
                    break;
            }
        }

        public static void ApplyDelta(SW.Entity e, ComponentDelta delta) {
            switch (delta.ComponentTypeId) {
                case ComponentTypeIds.CharacterNetState:
                    e.Set(DeserializeCharacterNetState(delta.Payload));
                    break;
                case ComponentTypeIds.NetworkIdentity:
                    e.Set(DeserializeNetworkIdentity(delta.Payload));
                    break;
            }
        }

        public static void CollectDirty(CW.Entity e, NetOutbox outbox, NetworkPeerId peer) {
            if (e.Has<CharacterNetState>() && e.HasChanged<CharacterNetState>())
                outbox.EnqueueComponentDelta(peer, CreateDelta(e.GID, e.Read<CharacterNetState>()), NetDelivery.UnreliableSequenced);
        }

        public static void CollectDirty(SW.Entity e, NetOutbox outbox, NetworkPeerId peer) {
            if (e.Has<CharacterNetState>() && e.HasChanged<CharacterNetState>())
                outbox.EnqueueComponentDelta(peer, CreateDelta(e.GID, e.Read<CharacterNetState>()), NetDelivery.UnreliableSequenced);
        }

        public static ComponentDelta CreateDelta(EntityGID gid, in CharacterNetState state) {
            return new ComponentDelta(gid, ComponentTypeIds.CharacterNetState, Serialize(state));
        }

        public static ComponentDelta CreateDelta(EntityGID gid, in NetworkIdentity identity) {
            return new ComponentDelta(gid, ComponentTypeIds.NetworkIdentity, Serialize(identity));
        }

        private static byte[] Serialize(in CharacterNetState state) {
            var writer = BinaryPackWriter.CreateFromPool(64);
            writer.WriteFloat(
                Quantize(state.Position.x), Quantize(state.Position.y), Quantize(state.Position.z)
            );
            writer.WriteFloat(
                Quantize(state.Velocity.x), Quantize(state.Velocity.y), Quantize(state.Velocity.z)
            );
            writer.WriteFloat(state.Rotation.x, state.Rotation.y, state.Rotation.z, state.Rotation.w);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        private static byte[] Serialize(in NetworkIdentity identity) {
            var writer = BinaryPackWriter.CreateFromPool(8);
            writer.WriteUshort(identity.Owner.Value);
            writer.WriteByte((byte)identity.Authority);
            writer.WriteUshort(identity.PrefabId);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        private static CharacterNetState DeserializeCharacterNetState(byte[] payload) {
            if (payload == null || payload.Length == 0)
                return default;

            var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
            return new CharacterNetState {
                Position = new UnityEngine.Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()),
                Velocity = new UnityEngine.Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()),
                Rotation = new UnityEngine.Quaternion(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat())
            };
        }

        private static NetworkIdentity DeserializeNetworkIdentity(byte[] payload) {
            if (payload == null || payload.Length == 0)
                return default;

            var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
            return new NetworkIdentity {
                Owner = new NetworkPeerId(reader.ReadUshort()),
                Authority = (NetworkAuthority)reader.ReadByte(),
                PrefabId = reader.ReadUshort()
            };
        }

        private static float Quantize(float value) {
            return (float)Math.Round(value / 0.01f) * 0.01f;
        }
    }
}
