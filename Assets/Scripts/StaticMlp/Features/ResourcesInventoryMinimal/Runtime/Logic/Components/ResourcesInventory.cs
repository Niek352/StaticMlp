using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        audience: ReplicationAudience.OwnerOnly,
        guid: "943d0dd7-7080-4d28-8afc-44a649ac2c01"
    )]
    public partial struct ResourcesInventory : IComponent, IComponentConfig<ResourcesInventory>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        public const int MAX_SLOTS = 20;
        public const int MAX_STACK_AMOUNT = 20;

        [ReplicatedField]
        public int Capacity;

        public ComponentTypeConfig<ResourcesInventory> Config() =>
            new(guid: new Guid("943d0dd7-7080-4d28-8afc-44a649ac2c01"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            ValidateCapacity(Capacity);
            writer.WriteInt(Capacity);

            ref readonly var rows = ref self.Ref<World<TWorld>.Multi<CarriedResourceEntry>>();
            ResourcesInventoryAccess.ValidateRows(in rows, Capacity);
            writer.WriteInt(rows.Length);
            for (var i = 0; i < rows.Length; i++)
            {
                writer.WriteUshort(rows.Get(i).Id.Value);
                writer.WriteInt(rows.Get(i).Amount);
            }
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            Capacity = reader.ReadInt();
            ValidateCapacity(Capacity);

            var count = reader.ReadInt();
            if (count < 0 || count > Capacity)
                throw new InvalidOperationException($"Invalid carried resource row count {count}.");

            ref var rows = ref self.Has<World<TWorld>.Multi<CarriedResourceEntry>>()
                ? ref self.Ref<World<TWorld>.Multi<CarriedResourceEntry>>()
                : ref self.Add<World<TWorld>.Multi<CarriedResourceEntry>>();
            rows.Clear();

            for (var i = 0; i < count; i++)
            {
                var id = new ResourceId(reader.ReadUshort());
                var amount = reader.ReadInt();
                ResourcesInventoryAccess.ValidateRow(id, amount);
                rows.Add(new CarriedResourceEntry(id, amount));
            }

            ResourcesInventoryAccess.ValidateRows(in rows, Capacity);
        }

        public static void ValidateCapacity(int capacity)
        {
            if (capacity != MAX_SLOTS)
                throw new InvalidOperationException($"Resources inventory capacity must be {MAX_SLOTS}.");
        }
    }
}
