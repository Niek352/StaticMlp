using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Frontier
{
    public static class FrontierEncounterParticipantQuery
    {
        public static bool HasAnyLivingParticipant(
            SettlementAnchorId anchorId,
            FrontierEncounterKind encounterKind,
            ushort sourceId)
        {
            foreach (var entity in SW.Query<All<FrontierEncounterParticipant>, None<IsDiedTag>>().Entities())
            {
                ref readonly var participant = ref entity.Read<FrontierEncounterParticipant>();
                if (participant.AnchorId != anchorId.Value
                    || participant.EncounterKind != encounterKind
                    || participant.SourceId != sourceId)
                {
                    continue;
                }

                return true;
            }

            return false;
        }
    }
}
