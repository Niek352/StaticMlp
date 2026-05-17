using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.OpenWorldResources
{
    public struct OpenWorldResourceProxyState : IComponent
    {
        public ushort KindIdValue;
        public ushort RemainingAmount;
        public OpenWorldResourceOverlayFlags Flags;
    }
}
