using StaticMlp.Features.CampFlow;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Progression
{
    public static class ProgressionHudPresentation
    {
        public static ProgressionHudState Build()
        {
            if (!CampFlowProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
                return default;

            var state = new ProgressionHudState();

            if (anchor.Has<Projected<ProgressionState>>())
            {
                ref readonly var progression = ref ClientProjection.Read<ProgressionState>(anchor);
                state.HasRecoveredWarCache = progression.HasFlag(ProgressFlagCatalog.RecoveredWarCacheAppliedId);
                state.HasCounterattackDefended = progression.HasFlag(ProgressFlagCatalog.CounterattackDefendedId);
                state.HasBossUnlocked = progression.HasFlag(ProgressFlagCatalog.BossUnlockedId);
                state.BossPreparationTokens = progression.BossPreparationTokens;
            }

            return state;
        }
    }
}
