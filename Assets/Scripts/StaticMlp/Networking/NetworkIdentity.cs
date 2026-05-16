using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;

namespace StaticMlp.Networking {
    public struct NetworkIdentity : IComponent, IComponentConfig<NetworkIdentity>, ITrackableAdded, ITrackableChanged, ITrackableDeleted {
        public NetworkPeerId Owner;
        public NetworkAuthority Authority;
        public ushort NetworkArchetypeId;

        public ComponentTypeConfig<NetworkIdentity> Config() => new(guid: new Guid("2ac6d128-cd2c-48a4-a18b-6cd79cefe903"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self) where TWorld : struct, IWorldType {
            writer.WriteUshort(Owner.Value);
            writer.WriteByte((byte)Authority);
            writer.WriteUshort(NetworkArchetypeId);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled) where TWorld : struct, IWorldType {
            Owner = new NetworkPeerId(reader.ReadUshort());
            Authority = (NetworkAuthority)reader.ReadByte();
            NetworkArchetypeId = reader.ReadUshort();
        }
    }
}
