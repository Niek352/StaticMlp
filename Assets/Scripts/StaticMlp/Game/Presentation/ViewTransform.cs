using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using UnityEngine;

namespace StaticMlp.Game.Presentation
{
    public struct ViewTransform : IComponent, IComponentConfig<ViewTransform>, ITrackableAdded, ITrackableChanged,
        ITrackableDeleted
    {
        public Vector3 RenderPosition;
        public Quaternion RenderRotation;

        public ComponentTypeConfig<ViewTransform> Config() =>
            new(
                guid: new Guid("f6427f09-7e12-4b61-b453-b0837d8d08ba")
            );

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteFloat(RenderPosition.x, RenderPosition.y, RenderPosition.z);
            writer.WriteFloat(RenderRotation.x, RenderRotation.y, RenderRotation.z, RenderRotation.w);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            self.Set(new ViewTransform
            {
                RenderPosition = new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()),
                RenderRotation = new Quaternion(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(),
                    reader.ReadFloat())
            });
        }
    }
}