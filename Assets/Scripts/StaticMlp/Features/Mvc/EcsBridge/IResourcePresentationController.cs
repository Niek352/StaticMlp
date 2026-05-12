using FFS.Libraries.StaticEcs;

namespace Code.EcsUi.Mvc
{
    public interface IResourcePresentationController<TState>
        where TState : struct, IResource
    {
        void Apply(in TState state);
    }
}
