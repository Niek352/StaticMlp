using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using Unity.Mathematics;

namespace StaticMlp.Features.CombatDirector
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        guid: "33487314-85fa-4e81-85ac-35fe029e1a78"
    )]
    public partial struct EnemySpawnSource : IComponent, IComponentConfig<EnemySpawnSource>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        public EntityGID SourceEntity;
        public SpawnSourceType SourceType;
        public float3 SpawnPosition;

        public ComponentTypeConfig<EnemySpawnSource> Config() =>
            new(guid: new Guid("33487314-85fa-4e81-85ac-35fe029e1a78"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteUlong(SourceEntity.Raw);
            writer.WriteByte((byte)SourceType);
            writer.WriteFloat(SpawnPosition.x, SpawnPosition.y, SpawnPosition.z);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            SourceEntity = new EntityGID(reader.ReadUlong());
            SourceType = (SpawnSourceType)reader.ReadByte();
            SpawnPosition = new float3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
        }
    }
}
