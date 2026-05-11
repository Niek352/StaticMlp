using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public struct Stage1ContextPanelSession : IResource
    {
        public Stage1ContextPanelMode Mode;
        public EntityGID FocusedSite;
        public SettlementAnchorId WorkerAnchorId;
    }
}
