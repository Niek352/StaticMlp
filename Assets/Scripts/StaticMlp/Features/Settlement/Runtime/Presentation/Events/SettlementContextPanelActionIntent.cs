using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public readonly struct SettlementContextPanelActionIntent : IEvent
    {
        public readonly bool IsPrimary;

        public SettlementContextPanelActionIntent(bool isPrimary)
        {
            IsPrimary = isPrimary;
        }
    }
}
