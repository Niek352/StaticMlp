using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.OpenWorldResources
{
    public struct OpenWorldResourceHarvestCooldownState : IComponent
    {
        public uint NextHarvestTick;
    }
}
