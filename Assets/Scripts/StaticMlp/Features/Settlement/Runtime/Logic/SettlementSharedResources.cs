using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        guid: "1fd72b18-30b4-4f37-ae61-b95f52eafb65"
    )]
    public partial struct SettlementSharedResources : IComponent, IComponentConfig<SettlementSharedResources>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public int Wood;

        [ReplicatedField]
        public int Stone;

        public readonly int GetAmount(ResourceId resourceId)
        {
            if (resourceId == ResourceCatalog.WoodId)
                return Wood;

            if (resourceId == ResourceCatalog.StoneId)
                return Stone;

            throw new InvalidOperationException($"Unsupported settlement resource id {resourceId.Value}.");
        }

        public int Spend(ResourceId resourceId, int requested)
        {
            var amount = Math.Max(0, requested);
            if (resourceId == ResourceCatalog.WoodId)
            {
                var spent = Math.Min(amount, Wood);
                Wood -= spent;
                return spent;
            }

            if (resourceId == ResourceCatalog.StoneId)
            {
                var spent = Math.Min(amount, Stone);
                Stone -= spent;
                return spent;
            }

            throw new InvalidOperationException($"Unsupported settlement resource id {resourceId.Value}.");
        }

        public ComponentTypeConfig<SettlementSharedResources> Config() =>
            new(guid: new Guid("1fd72b18-30b4-4f37-ae61-b95f52eafb65"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteInt(Wood);
            writer.WriteInt(Stone);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            Wood = reader.ReadInt();
            Stone = reader.ReadInt();
        }
    }
}
