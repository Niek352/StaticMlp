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
    public struct ConstructionProgress : IComponent, IComponentConfig<ConstructionProgress>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField(Quantize = 0.01f)] public float BuildWorkRequired;
        [ReplicatedField(Quantize = 0.01f)] public float BuildWorkDone;

        public float Normalized => BuildWorkRequired <= 0f ? 1f : Mathf.Clamp01(BuildWorkDone / BuildWorkRequired);
        public bool IsComplete => BuildWorkDone >= BuildWorkRequired;

        public ComponentTypeConfig<ConstructionProgress> Config() =>
            new(guid: new Guid("0d11fbba-0d66-40e4-973b-c19cdfb4ec04"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteFloat(BuildWorkRequired);
            writer.WriteFloat(BuildWorkDone);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            self.Set(new ConstructionProgress
            {
                BuildWorkRequired = reader.ReadFloat(),
                BuildWorkDone = reader.ReadFloat()
            });
        }
    }
}
