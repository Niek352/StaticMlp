using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Requests
{
    public static class ProjectionRegistry
    {
        private static readonly Dictionary<Type, IProjectionHandler> Handlers = new();

        public static bool HasRegistrations => Handlers.Count > 0;

        public static void Clear()
        {
            Handlers.Clear();
        }

        public static void Register<T>() where T : struct, IComponent
        {
            Handlers[typeof(T)] = new ProjectionHandler<T>();
        }

        public static void RegisterClientWorldTypes()
        {
            foreach (var handler in Handlers.Values)
                handler.RegisterClientWorldType();
        }

        public static void Rebuild()
        {
            foreach (var handler in Handlers.Values)
                handler.Rebuild();
        }

        private interface IProjectionHandler
        {
            void RegisterClientWorldType();
            void Rebuild();
        }

        private sealed class ProjectionHandler<T> : IProjectionHandler where T : struct, IComponent
        {
            public void RegisterClientWorldType()
            {
                CW.Types().Component<Projected<T>>();
            }

            public void Rebuild()
            {
                foreach (var entity in CW.Query<All<T>>().Entities())
                {
                    var authoritative = entity.Read<T>();
                    if (entity.Has<Projected<T>>())
                    {
                        ref var projected = ref entity.Mut<Projected<T>>();
                        projected.Value = authoritative;
                    }
                    else
                    {
                        entity.Set(new Projected<T>(authoritative));
                    }
                }

                foreach (var entity in CW.Query<All<Projected<T>>>().Entities())
                {
                    if (!entity.Has<T>())
                        entity.Delete<Projected<T>>();
                }
            }
        }
    }
}
