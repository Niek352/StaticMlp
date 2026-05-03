using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Game.Components.Buildings
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5
    )]
    public struct ConstructionTransform : IComponent, IComponentConfig<ConstructionTransform>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField(Quantize = 0.01f)] public Vector3 Position;
        [ReplicatedField(Compress = true)] public Quaternion Rotation;

        public ComponentTypeConfig<ConstructionTransform> Config() =>
            new(guid: new Guid("0d11fbba-0d66-40e4-973b-c19cdfb4ec02"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteFloat(Position.x, Position.y, Position.z);
            writer.WriteFloat(Rotation.x, Rotation.y, Rotation.z, Rotation.w);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            self.Set(new ConstructionTransform
            {
                Position = new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()),
                Rotation = new Quaternion(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat())
            });
        }
    }
}
