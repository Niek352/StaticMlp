using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.CampFlow
{
    public static class CampFlowProgressionQuery
    {
        public static SW.Entity GetServerAnchor(SettlementAnchorId anchorId)
        {
            if (TryGetServerAnchor(anchorId, out var anchor))
                return anchor;

            throw new InvalidOperationException(
                $"Stage 1 settlement anchor {anchorId.Value} is missing in the server world.");
        }

        public static bool TryGetServerAnchor(SettlementAnchorId anchorId, out SW.Entity anchor)
        {
            foreach (var entity in SW.Query<All<CampFlowProgression>>().Entities())
            {
                ref readonly var progression = ref entity.Read<CampFlowProgression>();
                if (progression.AnchorId != anchorId.Value)
                    continue;

                anchor = entity;
                return true;
            }

            anchor = default;
            return false;
        }

        public static CW.Entity GetClientAnchor(SettlementAnchorId anchorId)
        {
            if (TryGetClientAnchor(anchorId, out var anchor))
                return anchor;

            throw new InvalidOperationException(
                $"Stage 1 settlement anchor {anchorId.Value} is missing in the client world.");
        }

        public static bool TryGetClientAnchor(SettlementAnchorId anchorId, out CW.Entity anchor)
        {
            foreach (var entity in CW.Query<All<CampFlowProgression>>().Entities())
            {
                ref readonly var progression = ref entity.Read<CampFlowProgression>();
                if (progression.AnchorId != anchorId.Value)
                    continue;

                anchor = entity;
                return true;
            }

            anchor = default;
            return false;
        }
    }
}
