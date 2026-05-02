using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Game.Components
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.UnreliableSequenced,
        sendRate: 20
    )]
    public struct PhysicsCubeNetState : IComponent, IComponentConfig<PhysicsCubeNetState>, ITrackableAdded,
        ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField(Quantize = 0.01f, Interpolation = ReplicatedFieldInterpolation.Auto)]
        public Vector3 Position;

        [ReplicatedField(Quantize = 0.01f)] public Vector3 Velocity;

        [ReplicatedField(Compress = true, Interpolation = ReplicatedFieldInterpolation.Auto)]
        public Quaternion Rotation;

        public ComponentTypeConfig<PhysicsCubeNetState> Config() =>
            new(guid: new Guid("85f080de-0634-4b3d-bc95-c361c73d7f98"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteFloat(Position.x, Position.y, Position.z);
            writer.WriteFloat(Velocity.x, Velocity.y, Velocity.z);
            writer.WriteFloat(Rotation.x, Rotation.y, Rotation.z, Rotation.w);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            self.Set(new PhysicsCubeNetState
            {
                Position = new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()),
                Velocity = new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()),
                Rotation = new Quaternion(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(),
                    reader.ReadFloat())
            });
        }
    }
}
