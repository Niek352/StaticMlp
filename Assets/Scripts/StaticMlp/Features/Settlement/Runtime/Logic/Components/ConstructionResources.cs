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
        guid: "0d11fbba-0d66-40e4-973b-c19cdfb4ec03"
    )]
    public partial struct ConstructionResources : IComponent, IComponentConfig<ConstructionResources>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        public ComponentTypeConfig<ConstructionResources> Config() =>
            new(guid: new Guid("0d11fbba-0d66-40e4-973b-c19cdfb4ec03"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            ref readonly var rows = ref self.Ref<World<TWorld>.Multi<ConstructionResourceEntry>>();
            ValidateRows(in rows);
            writer.WriteInt(rows.Length);
            for (var i = 0; i < rows.Length; i++)
            {
                writer.WriteUshort(rows.Get(i).Id.Value);
                writer.WriteInt(rows.Get(i).Required);
                writer.WriteInt(rows.Get(i).Delivered);
            }
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            var count = reader.ReadInt();
            if (count < 0 || count > ResourceCatalog.All.Count)
                throw new InvalidOperationException($"Invalid construction resource row count {count}.");

            ref var rows = ref self.Has<World<TWorld>.Multi<ConstructionResourceEntry>>()
                ? ref self.Ref<World<TWorld>.Multi<ConstructionResourceEntry>>()
                : ref self.Add<World<TWorld>.Multi<ConstructionResourceEntry>>();
            rows.Clear();

            for (var i = 0; i < count; i++)
            {
                var id = new ResourceId(reader.ReadUshort());
                var required = reader.ReadInt();
                var delivered = reader.ReadInt();
                ValidateRow(id, required, delivered);
                if (FindIndex(in rows, id) >= 0)
                    throw new InvalidOperationException($"Duplicate construction resource id {id.Value}.");

                rows.Add(new ConstructionResourceEntry(id, required, delivered));
            }
        }

        private static void ValidateRows<TWorld>(in World<TWorld>.Multi<ConstructionResourceEntry> rows)
            where TWorld : struct, IWorldType
        {
            if (rows.Length > ResourceCatalog.All.Count)
                throw new InvalidOperationException($"Invalid construction resource row count {rows.Length}.");

            for (var i = 0; i < rows.Length; i++)
            {
                ValidateRow(rows.Get(i).Id, rows.Get(i).Required, rows.Get(i).Delivered);
                for (var j = i + 1; j < rows.Length; j++)
                {
                    if (rows[i].Id == rows[j].Id)
                        throw new InvalidOperationException($"Duplicate construction resource id {rows[i].Id.Value}.");
                }
            }
        }

        private static void ValidateRow(ResourceId id, int required, int delivered)
        {
            ResourceCatalog.Get(id);
            if (required < 0)
                throw new InvalidOperationException($"Construction resource id {id.Value} has negative required amount {required}.");

            if (delivered < 0)
                throw new InvalidOperationException($"Construction resource id {id.Value} has negative delivered amount {delivered}.");

            if (delivered > required)
                throw new InvalidOperationException($"Construction resource id {id.Value} delivered amount {delivered} exceeds required amount {required}.");
        }

        private static int FindIndex<TWorld>(in World<TWorld>.Multi<ConstructionResourceEntry> rows, ResourceId id)
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
