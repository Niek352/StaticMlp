using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        audience: ReplicationAudience.OwnerOnly
    )]
    public struct ResourcesInventory : IComponent, IComponentConfig<ResourcesInventory>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public int Wood;

        [ReplicatedField]
        public int Stone;

        public int SpendWood(int requested)
        {
            var amount = Math.Min(Math.Max(0, requested), Wood);
            Wood -= amount;
            return amount;
        }

        public int SpendStone(int requested)
        {
            var amount = Math.Min(Math.Max(0, requested), Stone);
            Stone -= amount;
            return amount;
        }

        public ComponentTypeConfig<ResourcesInventory> Config() =>
            new(guid: new Guid("943d0dd7-7080-4d28-8afc-44a649ac2c01"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteInt(Wood);
            writer.WriteInt(Stone);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            self.Set(new ResourcesInventory
            {
                Wood = reader.ReadInt(),
                Stone = reader.ReadInt()
            });
        }
    }
}
