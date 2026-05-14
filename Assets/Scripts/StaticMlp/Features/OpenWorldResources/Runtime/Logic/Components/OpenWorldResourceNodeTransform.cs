using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldResources
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5
    )]
    public struct OpenWorldResourceNodeTransform : IComponent, IComponentConfig<OpenWorldResourceNodeTransform>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField(Quantize = 0.01f)]
        public Vector3 Position;

        [ReplicatedField(Quantize = 0.1f)]
        public float YawDegrees;

        [ReplicatedField(Quantize = 0.01f)]
        public float Scale;

        public ComponentTypeConfig<OpenWorldResourceNodeTransform> Config() =>
            new(guid: new Guid("c35019c4-4469-4ae6-a59f-4a9d10d9efaa"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteFloat(Position.x, Position.y, Position.z);
            writer.WriteFloat(YawDegrees);
            writer.WriteFloat(Scale);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            self.Set(new OpenWorldResourceNodeTransform
            {
                Position = new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()),
                YawDegrees = reader.ReadFloat(),
                Scale = reader.ReadFloat()
            });
        }
    }
}
