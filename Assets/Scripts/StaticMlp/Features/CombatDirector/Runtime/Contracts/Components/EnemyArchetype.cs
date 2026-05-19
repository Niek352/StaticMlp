using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.CombatDirector
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        guid: "2fd68c53-cf5c-4d13-94cc-1ad5420a82d7"
    )]
    public partial struct EnemyArchetype : IComponent, IComponentConfig<EnemyArchetype>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public EnemyRole Role;

        public ComponentTypeConfig<EnemyArchetype> Config() =>
            new(guid: new Guid("2fd68c53-cf5c-4d13-94cc-1ad5420a82d7"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteByte((byte)Role);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            Role = (EnemyRole)reader.ReadByte();
        }
    }
}
