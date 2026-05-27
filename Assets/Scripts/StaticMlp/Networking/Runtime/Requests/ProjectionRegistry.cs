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

        public static void RegisterMulti<T>() where T : struct, IMultiComponent
        {
            Handlers[typeof(World<ClientCoreWT>.Multi<T>)] = new MultiProjectionHandler<T>();
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

        private sealed class MultiProjectionHandler<T> : IProjectionHandler where T : struct, IMultiComponent
        {
            public void RegisterClientWorldType()
            {
                CW.Types().Multi<ProjectedMulti<T>>();
            }

            public void Rebuild()
            {
                foreach (var entity in CW.Query<All<CW.Multi<T>>>().Entities())
                {
                    ref readonly var authoritative = ref entity.Ref<CW.Multi<T>>();
                    ref var projected = ref entity.Has<CW.Multi<ProjectedMulti<T>>>()
                        ? ref entity.Ref<CW.Multi<ProjectedMulti<T>>>()
                        : ref entity.Add<CW.Multi<ProjectedMulti<T>>>();

                    projected.Clear();
                    for (var i = 0; i < authoritative.Length; i++)
                        projected.Add(new ProjectedMulti<T>(authoritative.Get(i)));
                }

                foreach (var entity in CW.Query<All<CW.Multi<ProjectedMulti<T>>>>().Entities())
                {
                    if (!entity.Has<CW.Multi<T>>())
                        entity.Delete<CW.Multi<ProjectedMulti<T>>>();
                }
            }
        }
    }
}
