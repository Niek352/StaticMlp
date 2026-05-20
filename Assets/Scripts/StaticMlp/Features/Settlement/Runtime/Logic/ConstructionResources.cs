using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

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
        [ReplicatedField] public int WoodRequired;
        [ReplicatedField] public int StoneRequired;
        [ReplicatedField] public int WoodDelivered;
        [ReplicatedField] public int StoneDelivered;
        [ReplicatedField] public int PlanksRequired;
        [ReplicatedField] public int SimplePartsRequired;
        [ReplicatedField] public int PlanksDelivered;
        [ReplicatedField] public int SimplePartsDelivered;

        public int RemainingWood => Mathf.Max(0, WoodRequired - WoodDelivered);
        public int RemainingStone => Mathf.Max(0, StoneRequired - StoneDelivered);
        public int RemainingPlanks => Mathf.Max(0, PlanksRequired - PlanksDelivered);
        public int RemainingSimpleParts => Mathf.Max(0, SimplePartsRequired - SimplePartsDelivered);
        public bool IsComplete => RemainingWood == 0
                                  && RemainingStone == 0
                                  && RemainingPlanks == 0
                                  && RemainingSimpleParts == 0;

        public readonly int GetRequired(ResourceId resourceId)
        {
            if (resourceId == ResourceCatalog.WoodId)
                return WoodRequired;

            if (resourceId == ResourceCatalog.StoneId)
                return StoneRequired;

            if (resourceId == ResourceCatalog.PlanksId)
                return PlanksRequired;

            if (resourceId == ResourceCatalog.SimplePartsId)
                return SimplePartsRequired;

            throw new InvalidOperationException($"Unsupported construction resource id {resourceId.Value}.");
        }

        public readonly int GetDelivered(ResourceId resourceId)
        {
            if (resourceId == ResourceCatalog.WoodId)
                return WoodDelivered;

            if (resourceId == ResourceCatalog.StoneId)
                return StoneDelivered;

            if (resourceId == ResourceCatalog.PlanksId)
                return PlanksDelivered;

            if (resourceId == ResourceCatalog.SimplePartsId)
                return SimplePartsDelivered;

            throw new InvalidOperationException($"Unsupported construction resource id {resourceId.Value}.");
        }

        public readonly int GetRemaining(ResourceId resourceId)
        {
            return Mathf.Max(0, GetRequired(resourceId) - GetDelivered(resourceId));
        }

        public void SetRequired(ResourceId resourceId, int amount)
        {
            if (amount < 0)
                throw new InvalidOperationException($"Construction resource id {resourceId.Value} has negative required amount {amount}.");

            if (resourceId == ResourceCatalog.WoodId)
            {
                WoodRequired = amount;
                return;
            }

            if (resourceId == ResourceCatalog.StoneId)
            {
                StoneRequired = amount;
                return;
            }

            if (resourceId == ResourceCatalog.PlanksId)
            {
                PlanksRequired = amount;
                return;
            }

            if (resourceId == ResourceCatalog.SimplePartsId)
            {
                SimplePartsRequired = amount;
                return;
            }

            throw new InvalidOperationException($"Unsupported construction resource id {resourceId.Value}.");
        }

        public int Deliver(ResourceId resourceId, int requested)
        {
            var amount = Math.Max(0, requested);
            if (resourceId == ResourceCatalog.WoodId)
            {
                var delivered = Math.Min(amount, RemainingWood);
                WoodDelivered += delivered;
                return delivered;
            }

            if (resourceId == ResourceCatalog.StoneId)
            {
                var delivered = Math.Min(amount, RemainingStone);
                StoneDelivered += delivered;
                return delivered;
            }

            if (resourceId == ResourceCatalog.PlanksId)
            {
                var delivered = Math.Min(amount, RemainingPlanks);
                PlanksDelivered += delivered;
                return delivered;
            }

            if (resourceId == ResourceCatalog.SimplePartsId)
            {
                var delivered = Math.Min(amount, RemainingSimpleParts);
                SimplePartsDelivered += delivered;
                return delivered;
            }

            throw new InvalidOperationException($"Unsupported construction resource id {resourceId.Value}.");
        }

        public void MarkDelivered(ResourceId resourceId)
        {
            if (resourceId == ResourceCatalog.WoodId)
            {
                WoodDelivered = WoodRequired;
                return;
            }

            if (resourceId == ResourceCatalog.StoneId)
            {
                StoneDelivered = StoneRequired;
                return;
            }

            if (resourceId == ResourceCatalog.PlanksId)
            {
                PlanksDelivered = PlanksRequired;
                return;
            }

            if (resourceId == ResourceCatalog.SimplePartsId)
            {
                SimplePartsDelivered = SimplePartsRequired;
                return;
            }

            throw new InvalidOperationException($"Unsupported construction resource id {resourceId.Value}.");
        }

        public ComponentTypeConfig<ConstructionResources> Config() =>
            new(guid: new Guid("0d11fbba-0d66-40e4-973b-c19cdfb4ec03"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteInt(WoodRequired);
            writer.WriteInt(StoneRequired);
            writer.WriteInt(WoodDelivered);
            writer.WriteInt(StoneDelivered);
            writer.WriteInt(PlanksRequired);
            writer.WriteInt(SimplePartsRequired);
            writer.WriteInt(PlanksDelivered);
            writer.WriteInt(SimplePartsDelivered);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            WoodRequired = reader.ReadInt();
            StoneRequired = reader.ReadInt();
            WoodDelivered = reader.ReadInt();
            StoneDelivered = reader.ReadInt();
            PlanksRequired = reader.ReadInt();
            SimplePartsRequired = reader.ReadInt();
            PlanksDelivered = reader.ReadInt();
            SimplePartsDelivered = reader.ReadInt();
        }
    }
}
