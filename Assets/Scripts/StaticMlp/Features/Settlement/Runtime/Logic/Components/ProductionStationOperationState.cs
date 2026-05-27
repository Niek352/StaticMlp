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
        guid: "d37b9b22-ea96-49e0-bc20-4ab248c04c98"
    )]
    public partial struct ProductionStationOperationState : IComponent, IComponentConfig<ProductionStationOperationState>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public ushort StationId;

        [ReplicatedField]
        public ushort ActiveRecipeId;

        [ReplicatedField]
        public bool Enabled;

        [ReplicatedField]
        public byte WorkerSlotCount;

        [ReplicatedField]
        public float WorkDone;

        public ProductionStationId Station => new(StationId);
        public ProductionRecipeId ActiveRecipe => new(ActiveRecipeId);

        public ComponentTypeConfig<ProductionStationOperationState> Config() =>
            new(guid: new Guid("d37b9b22-ea96-49e0-bc20-4ab248c04c98"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteUshort(StationId);
            writer.WriteUshort(ActiveRecipeId);
            writer.WriteBool(Enabled);
            writer.WriteByte(WorkerSlotCount);
            writer.WriteFloat(WorkDone);

            ref readonly var inputs = ref self.Ref<World<TWorld>.Multi<ProductionStationInputResource>>();
            ValidateInputRows(in inputs);
            writer.WriteInt(inputs.Length);
            for (var i = 0; i < inputs.Length; i++)
            {
                writer.WriteUshort(inputs.Get(i).Id.Value);
                writer.WriteInt(inputs.Get(i).Amount);
            }

            ref readonly var outputs = ref self.Ref<World<TWorld>.Multi<ProductionStationOutputResource>>();
            ValidateOutputRows(in outputs);
            writer.WriteInt(outputs.Length);
            for (var i = 0; i < outputs.Length; i++)
            {
                writer.WriteUshort(outputs.Get(i).Id.Value);
                writer.WriteInt(outputs.Get(i).Amount);
            }
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            StationId = reader.ReadUshort();
            ActiveRecipeId = reader.ReadUshort();
            Enabled = reader.ReadBool();
            WorkerSlotCount = reader.ReadByte();
            WorkDone = reader.ReadFloat();

            var inputCount = reader.ReadInt();
            if (inputCount < 0 || inputCount > ResourceCatalog.All.Count)
                throw new InvalidOperationException($"Invalid production station input row count {inputCount}.");

            ref var inputs = ref self.Has<World<TWorld>.Multi<ProductionStationInputResource>>()
                ? ref self.Ref<World<TWorld>.Multi<ProductionStationInputResource>>()
                : ref self.Add<World<TWorld>.Multi<ProductionStationInputResource>>();
            inputs.Clear();
            for (var i = 0; i < inputCount; i++)
            {
                var id = new ResourceId(reader.ReadUshort());
                var amount = reader.ReadInt();
                ValidateRow(id, amount, "input");
                if (FindInputIndex(in inputs, id) >= 0)
                    throw new InvalidOperationException($"Duplicate production station input resource id {id.Value}.");

                inputs.Add(new ProductionStationInputResource(id, amount));
            }

            var outputCount = reader.ReadInt();
            if (outputCount < 0 || outputCount > ResourceCatalog.All.Count)
                throw new InvalidOperationException($"Invalid production station output row count {outputCount}.");

            ref var outputs = ref self.Has<World<TWorld>.Multi<ProductionStationOutputResource>>()
                ? ref self.Ref<World<TWorld>.Multi<ProductionStationOutputResource>>()
                : ref self.Add<World<TWorld>.Multi<ProductionStationOutputResource>>();
            outputs.Clear();
            for (var i = 0; i < outputCount; i++)
            {
                var id = new ResourceId(reader.ReadUshort());
                var amount = reader.ReadInt();
                ValidateRow(id, amount, "output");
                if (FindOutputIndex(in outputs, id) >= 0)
                    throw new InvalidOperationException($"Duplicate production station output resource id {id.Value}.");

                outputs.Add(new ProductionStationOutputResource(id, amount));
            }
        }

        private static void ValidateInputRows<TWorld>(in World<TWorld>.Multi<ProductionStationInputResource> rows)
            where TWorld : struct, IWorldType
        {
            if (rows.Length > ResourceCatalog.All.Count)
                throw new InvalidOperationException($"Invalid production station input row count {rows.Length}.");

            for (var i = 0; i < rows.Length; i++)
            {
                ValidateRow(rows.Get(i).Id, rows.Get(i).Amount, "input");
                for (var j = i + 1; j < rows.Length; j++)
                {
                    if (rows[i].Id == rows[j].Id)
                        throw new InvalidOperationException($"Duplicate production station input resource id {rows[i].Id.Value}.");
                }
            }
        }

        private static void ValidateOutputRows<TWorld>(in World<TWorld>.Multi<ProductionStationOutputResource> rows)
            where TWorld : struct, IWorldType
        {
            if (rows.Length > ResourceCatalog.All.Count)
                throw new InvalidOperationException($"Invalid production station output row count {rows.Length}.");

            for (var i = 0; i < rows.Length; i++)
            {
                ValidateRow(rows.Get(i).Id, rows.Get(i).Amount, "output");
                for (var j = i + 1; j < rows.Length; j++)
                {
                    if (rows[i].Id == rows[j].Id)
                        throw new InvalidOperationException($"Duplicate production station output resource id {rows[i].Id.Value}.");
                }
            }
        }

        private static void ValidateRow(ResourceId id, int amount, string role)
        {
            ResourceCatalog.Get(id);
            if (amount < 0)
                throw new InvalidOperationException(
                    $"Production station {role} resource id {id.Value} has negative amount {amount}.");
        }

        private static int FindInputIndex<TWorld>(in World<TWorld>.Multi<ProductionStationInputResource> rows, ResourceId id)
            where TWorld : struct, IWorldType
        {
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Id == id)
                    return i;
            }

            return -1;
        }

        private static int FindOutputIndex<TWorld>(in World<TWorld>.Multi<ProductionStationOutputResource> rows, ResourceId id)
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
