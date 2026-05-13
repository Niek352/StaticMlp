using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Build
{
    public struct PrepareBuildCommand : IEvent
    {
        public SettlementAnchorId AnchorId;
        public BuildModuleId PrimaryModuleId;

        public PrepareBuildCommand(SettlementAnchorId anchorId, BuildModuleId primaryModuleId)
        {
            AnchorId = anchorId;
            PrimaryModuleId = primaryModuleId;
        }
    }
}
