using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;

namespace StaticMlp.Features.OpenWorldResources
{
    public struct OpenWorldChunkRef : IComponent, IComponentConfig<OpenWorldChunkRef>
    {
        public int X;
        public int Z;

        public ComponentTypeConfig<OpenWorldChunkRef> Config() =>
            new(guid: new Guid("6abb3da8-3525-48cb-8265-f041b1cf0ba3"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteInt(X);
            writer.WriteInt(Z);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            X = reader.ReadInt();
            Z = reader.ReadInt();
        }
    }
}
