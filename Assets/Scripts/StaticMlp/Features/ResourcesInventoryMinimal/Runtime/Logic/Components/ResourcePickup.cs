using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        guid: "70eaf461-6ab4-46d7-b4b7-49179f7ad6bf"
    )]
    public partial struct ResourcePickup : IComponent, IComponentConfig<ResourcePickup>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public ushort ResourceId;

        [ReplicatedField]
        public int Amount;

        [ReplicatedField(Quantize = 0.01f)]
        public Vector3 Position;

        public ResourceAmount Resource => new(new ResourceId(ResourceId), Amount);

        public void Validate()
        {
            if (ResourceId == 0)
                throw new InvalidOperationException($"{nameof(ResourcePickup)} has no resource id.");
            if (Amount <= 0)
                throw new InvalidOperationException($"{nameof(ResourcePickup)} resource id {ResourceId} has invalid amount {Amount}.");
            if (!IsFinite(Position.x) || !IsFinite(Position.y) || !IsFinite(Position.z))
                throw new InvalidOperationException($"{nameof(ResourcePickup)} resource id {ResourceId} has non-finite position {Position}.");

            ref readonly var definition = ref ResourceCatalog.Get(new ResourceId(ResourceId));
            if (definition.Family != ResourceFamily.Raw)
                throw new InvalidOperationException($"{nameof(ResourcePickup)} resource id {ResourceId} is {definition.Family}, not raw.");
        }

        public ComponentTypeConfig<ResourcePickup> Config() =>
            new(guid: new Guid("70eaf461-6ab4-46d7-b4b7-49179f7ad6bf"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            Validate();
            writer.WriteUshort(ResourceId);
            writer.WriteInt(Amount);
            writer.WriteFloat(Position.x, Position.y, Position.z);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            ResourceId = reader.ReadUshort();
            Amount = reader.ReadInt();
            Position = new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
            Validate();
        }

        private static bool IsFinite(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
