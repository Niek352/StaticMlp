using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;

namespace StaticMlp.Networking {
    public enum NetworkAuthority : byte {
        Server = 0,
        Owner = 1,
        LocalOnly = 2
    }

    public readonly struct NetworkPeerId : IEquatable<NetworkPeerId> {
        public readonly ushort Value;

        public NetworkPeerId(ushort value) {
            Value = value;
        }

        public bool Equals(NetworkPeerId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is NetworkPeerId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => Value.ToString();

        public static bool operator ==(NetworkPeerId left, NetworkPeerId right) => left.Equals(right);
        public static bool operator !=(NetworkPeerId left, NetworkPeerId right) => !left.Equals(right);
    }

    public struct NetworkIdentity : IComponent, IComponentConfig<NetworkIdentity> {
        public NetworkPeerId Owner;
        public NetworkAuthority Authority;
        public ushort PrefabId;

        public ComponentTypeConfig<NetworkIdentity> Config() => new(
            guid: new Guid("2ac6d128-cd2c-48a4-a18b-6cd79cefe903"),
            trackAdded: true,
            trackDeleted: true,
            trackChanged: true
        );

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self) where TWorld : struct, IWorldType {
            writer.WriteUshort(Owner.Value);
            writer.WriteByte((byte)Authority);
            writer.WriteUshort(PrefabId);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled) where TWorld : struct, IWorldType {
            self.Set(new NetworkIdentity {
                Owner = new NetworkPeerId(reader.ReadUshort()),
                Authority = (NetworkAuthority)reader.ReadByte(),
                PrefabId = reader.ReadUshort()
            });
        }
    }

    public enum NetDelivery : byte {
        Unreliable = 0,
        UnreliableSequenced = 1,
        ReliableSequenced = 2
    }

    public enum NetPacketType : byte {
        Hello = 1,
        Welcome = 2,
        Spawn = 10,
        Despawn = 11,
        OwnershipChanged = 12,
        ComponentBatch = 20,
        NetworkEvent = 30,
        Ping = 40,
        Pong = 41
    }

    public static class NetworkRuntime {
        public static NetworkPeerId LocalPeerId;
    }
}
