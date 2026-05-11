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
        sendRate: 5
    )]
    public struct ConstructionResources : IComponent, IComponentConfig<ConstructionResources>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField] public int WoodRequired;
        [ReplicatedField] public int StoneRequired;
        [ReplicatedField] public int WoodDelivered;
        [ReplicatedField] public int StoneDelivered;

        public int RemainingWood => Mathf.Max(0, WoodRequired - WoodDelivered);
        public int RemainingStone => Mathf.Max(0, StoneRequired - StoneDelivered);
        public bool IsComplete => RemainingWood == 0 && RemainingStone == 0;

        public ComponentTypeConfig<ConstructionResources> Config() =>
            new(guid: new Guid("0d11fbba-0d66-40e4-973b-c19cdfb4ec03"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteInt(WoodRequired);
            writer.WriteInt(StoneRequired);
            writer.WriteInt(WoodDelivered);
            writer.WriteInt(StoneDelivered);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            self.Set(new ConstructionResources
            {
                WoodRequired = reader.ReadInt(),
                StoneRequired = reader.ReadInt(),
                WoodDelivered = reader.ReadInt(),
                StoneDelivered = reader.ReadInt()
            });
        }
    }
}
