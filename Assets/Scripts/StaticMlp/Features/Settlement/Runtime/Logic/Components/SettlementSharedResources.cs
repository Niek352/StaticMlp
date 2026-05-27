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
        public int Capacity;

        public ComponentTypeConfig<SettlementSharedResources> Config() =>
            new(guid: new Guid("1fd72b18-30b4-4f37-ae61-b95f52eafb65"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteInt(Capacity);

            ref readonly var rows = ref self.Ref<World<TWorld>.Multi<SettlementStoredResource>>();
            ValidateRows(in rows);
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

            var count = reader.ReadInt();
            if (count < 0 || count > ResourceCatalog.All.Count)
                throw new InvalidOperationException($"Invalid settlement stored resource row count {count}.");

            ref var rows = ref self.Has<World<TWorld>.Multi<SettlementStoredResource>>()
                ? ref self.Ref<World<TWorld>.Multi<SettlementStoredResource>>()
                : ref self.Add<World<TWorld>.Multi<SettlementStoredResource>>();
            rows.Clear();

            for (var i = 0; i < count; i++)
            {
                var id = new ResourceId(reader.ReadUshort());
                var amount = reader.ReadInt();
                ValidateRow(id, amount);
                if (FindIndex(in rows, id) >= 0)
                    throw new InvalidOperationException($"Duplicate settlement stored resource id {id.Value}.");

                rows.Add(new SettlementStoredResource(id, amount));
            }
        }

        private static void ValidateRows<TWorld>(in World<TWorld>.Multi<SettlementStoredResource> rows)
            where TWorld : struct, IWorldType
        {
            if (rows.Length > ResourceCatalog.All.Count)
                throw new InvalidOperationException($"Invalid settlement stored resource row count {rows.Length}.");

            for (var i = 0; i < rows.Length; i++)
            {
                ValidateRow(rows.Get(i).Id, rows.Get(i).Amount);
                for (var j = i + 1; j < rows.Length; j++)
                {
                    if (rows[i].Id == rows[j].Id)
                        throw new InvalidOperationException($"Duplicate settlement stored resource id {rows[i].Id.Value}.");
                }
            }
        }

        private static void ValidateRow(ResourceId id, int amount)
        {
            ref readonly var definition = ref ResourceCatalog.Get(id);
            if (!definition.IsSettlementStored)
                throw new InvalidOperationException($"Settlement storage cannot contain non-stored resource id {id.Value}.");

            if (amount < 0)
                throw new InvalidOperationException($"Settlement stored resource id {id.Value} has negative amount {amount}.");
        }

        private static int FindIndex<TWorld>(in World<TWorld>.Multi<SettlementStoredResource> rows, ResourceId id)
            where TWorld : struct, IWorldType
        {
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Id == id)
                    return i;
            }

            return -1;
        }
    }
}
