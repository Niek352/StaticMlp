using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Features.Combat;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Build
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        guid: "641d2d15-83f0-4328-8b35-4063e914bc61"
    )]
    public partial struct BossPreparedBuildSnapshot : IComponent, IComponentConfig<BossPreparedBuildSnapshot>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public ushort ArchetypeIdValue;

        [ReplicatedField]
        public ushort PrimaryModuleIdValue;

        [ReplicatedField]
        public CombatAbilityId PreparedAbilityId;

        [ReplicatedField]
        public CombatAbilityId FallbackAbilityId;

        public BuildArchetypeId ArchetypeId => new(ArchetypeIdValue);
        public BuildModuleId PrimaryModuleId => new(PrimaryModuleIdValue);

        public ComponentTypeConfig<BossPreparedBuildSnapshot> Config() =>
            new(guid: new Guid("641d2d15-83f0-4328-8b35-4063e914bc61"));

        public void Apply(in PreparedBuildSnapshot snapshot)
        {
            ArchetypeIdValue = snapshot.ArchetypeId.Value;
            PrimaryModuleIdValue = snapshot.PrimaryModuleId.Value;
            PreparedAbilityId = snapshot.PreparedAbilityId;
            FallbackAbilityId = snapshot.FallbackAbilityId;
        }

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteUshort(ArchetypeIdValue);
            writer.WriteUshort(PrimaryModuleIdValue);
            writer.WriteUshort((ushort)PreparedAbilityId);
            writer.WriteUshort((ushort)FallbackAbilityId);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            ArchetypeIdValue = reader.ReadUshort();
            PrimaryModuleIdValue = reader.ReadUshort();
            PreparedAbilityId = (CombatAbilityId)reader.ReadUshort();
            FallbackAbilityId = (CombatAbilityId)reader.ReadUshort();
        }
    }
}
