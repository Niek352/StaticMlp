using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs.Windows
{
    public readonly struct SetAllEcsWindowsPresentationActiveRequest : IEvent
    {
        public readonly bool IsActive;

        public SetAllEcsWindowsPresentationActiveRequest(bool isActive)
        {
            IsActive = isActive;
        }
    }
}
