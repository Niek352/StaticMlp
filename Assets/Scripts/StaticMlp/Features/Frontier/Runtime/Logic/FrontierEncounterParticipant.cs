using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Frontier
{
    public struct FrontierEncounterParticipant : IComponent
    {
        public ushort AnchorId;
        public FrontierEncounterKind EncounterKind;
        public ushort SourceId;

        public SettlementAnchorId Anchor => new(AnchorId);
    }
}
