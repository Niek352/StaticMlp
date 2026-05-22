using System;
using FFS.Libraries.StaticEcs;

namespace Code.EcsUi.Mvc
{
    [Obsolete("Temp: Controllers should receive plain view-data structs built by custom bridges.")]
    public interface IResourcePresentationController<TState>
        where TState : struct, IResource
    {
        void Apply(in TState state);
    }
}
