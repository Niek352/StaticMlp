using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Build
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5
    )]
    public struct BossBuildPreparationState : IComponent, IComponentConfig<BossBuildPreparationState>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public BossBuildPreparationStatus Status;

        public ComponentTypeConfig<BossBuildPreparationState> Config() =>
            new(guid: new Guid("4a6ac13f-d0a4-4180-b15a-c544f665fafd"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteByte((byte)Status);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            self.Set(new BossBuildPreparationState
            {
                Status = (BossBuildPreparationStatus)reader.ReadByte()
            });
        }
    }
}
