using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs
{
    public sealed class EcsLinkSyncSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        public void Update()
        {
            World<TWorld>.GetResource<EcsLinkRegistry<TWorld>>().Sync();
        }
    }
}
