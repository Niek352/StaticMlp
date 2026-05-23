using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public struct SettlementContextPanelSession : IResource
    {
        public SettlementContextPanelMode Mode;
        public EntityGID FocusedSite;
        public SettlementAnchorId WorkerAnchorId;
    }
}
