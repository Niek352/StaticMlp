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

        [ReplicatedField]
        public int Planks;

        [ReplicatedField]
        public int SimpleParts;

        [ReplicatedField]
        public int RepairKits;

        [ReplicatedField]
        public int Food;

        [ReplicatedField]
        public int Fuel;

        [ReplicatedField]
        public int ResearchData;

        [ReplicatedField]
        public int Medicine;

        public readonly int GetAmount(ResourceId resourceId)
        {
            if (resourceId == ResourceCatalog.WoodId)
                return Wood;

            if (resourceId == ResourceCatalog.StoneId)
                return Stone;

            if (resourceId == ResourceCatalog.PlanksId)
                return Planks;

            if (resourceId == ResourceCatalog.SimplePartsId)
                return SimpleParts;

            if (resourceId == ResourceCatalog.RepairKitsId)
                return RepairKits;

            if (resourceId == ResourceCatalog.FoodId)
                return Food;

            if (resourceId == ResourceCatalog.FuelId)
                return Fuel;

            if (resourceId == ResourceCatalog.ResearchDataId)
                return ResearchData;

            if (resourceId == ResourceCatalog.MedicineId)
                return Medicine;

            throw new InvalidOperationException($"Unsupported settlement resource id {resourceId.Value}.");
        }

        public void Add(ResourceId resourceId, int amount)
        {
            if (amount < 0)
                throw new InvalidOperationException($"Cannot add negative settlement resource amount {amount} for resource id {resourceId.Value}.");

            if (resourceId == ResourceCatalog.WoodId)
            {
                Wood += amount;
                return;
            }

            if (resourceId == ResourceCatalog.StoneId)
            {
                Stone += amount;
                return;
            }

            if (resourceId == ResourceCatalog.PlanksId)
            {
                Planks += amount;
                return;
            }

            if (resourceId == ResourceCatalog.SimplePartsId)
            {
                SimpleParts += amount;
                return;
            }

            if (resourceId == ResourceCatalog.RepairKitsId)
            {
                RepairKits += amount;
                return;
            }

            if (resourceId == ResourceCatalog.FoodId)
            {
                Food += amount;
                return;
            }

            if (resourceId == ResourceCatalog.FuelId)
            {
                Fuel += amount;
                return;
            }

            if (resourceId == ResourceCatalog.ResearchDataId)
            {
                ResearchData += amount;
                return;
            }

            if (resourceId == ResourceCatalog.MedicineId)
            {
                Medicine += amount;
                return;
            }

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

            if (resourceId == ResourceCatalog.PlanksId)
            {
                var spent = Math.Min(amount, Planks);
                Planks -= spent;
                return spent;
            }

            if (resourceId == ResourceCatalog.SimplePartsId)
            {
                var spent = Math.Min(amount, SimpleParts);
                SimpleParts -= spent;
                return spent;
            }

            if (resourceId == ResourceCatalog.RepairKitsId)
            {
                var spent = Math.Min(amount, RepairKits);
                RepairKits -= spent;
                return spent;
            }

            if (resourceId == ResourceCatalog.FoodId)
            {
                var spent = Math.Min(amount, Food);
                Food -= spent;
                return spent;
            }

            if (resourceId == ResourceCatalog.FuelId)
            {
                var spent = Math.Min(amount, Fuel);
                Fuel -= spent;
                return spent;
            }

            if (resourceId == ResourceCatalog.ResearchDataId)
            {
                var spent = Math.Min(amount, ResearchData);
                ResearchData -= spent;
                return spent;
            }

            if (resourceId == ResourceCatalog.MedicineId)
            {
                var spent = Math.Min(amount, Medicine);
                Medicine -= spent;
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
            writer.WriteInt(Planks);
            writer.WriteInt(SimpleParts);
            writer.WriteInt(RepairKits);
            writer.WriteInt(Food);
            writer.WriteInt(Fuel);
            writer.WriteInt(ResearchData);
            writer.WriteInt(Medicine);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            Wood = reader.ReadInt();
            Stone = reader.ReadInt();
            Planks = reader.ReadInt();
            SimpleParts = reader.ReadInt();
            RepairKits = reader.ReadInt();
            Food = reader.ReadInt();
            Fuel = reader.ReadInt();
            ResearchData = reader.ReadInt();
            Medicine = reader.ReadInt();
        }
    }
}
