using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Requests
{
    public static class ClientProjection
    {
        public static ref readonly T Read<T>(CW.Entity entity)
            where T : struct, IComponent
        {
            if (entity.Has<Projected<T>>())
            {
                ref readonly var projected = ref entity.Read<Projected<T>>();
                return ref projected.Value;
            }

            if (entity.Has<T>())
                return ref entity.Read<T>();

            throw new InvalidOperationException($"Entity {entity.GID.Raw} is missing {typeof(T).Name}.");
        }

        public static ref T Mut<T>(CW.Entity entity)
            where T : struct, IComponent
        {
            if (!entity.Has<Projected<T>>())
                throw new InvalidOperationException($"Entity {entity.GID.Raw} is missing projected {typeof(T).Name}.");

            ref var projected = ref entity.Mut<Projected<T>>();
            return ref projected.Value;
        }
    }
}
