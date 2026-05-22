using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Progression
{
    public sealed class ClientProgressionHudStateSystem : ISystem
    {
        public void Update()
        {
            if (!Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
                return;

            var next = new ProgressionHudState();

            if (anchor.Has<Projected<Stage1ProgressionState>>())
            {
                ref readonly var progression = ref ClientProjection.Read<Stage1ProgressionState>(anchor);
                next.HasRecoveredWarCache = progression.HasFlag(ProgressFlagCatalog.RecoveredWarCacheAppliedId);
                next.HasCounterattackDefended = progression.HasFlag(ProgressFlagCatalog.CounterattackDefendedId);
                next.HasBossUnlocked = progression.HasFlag(ProgressFlagCatalog.BossUnlockedId);
                next.BossPreparationTokens = progression.BossPreparationTokens;
            }

            CW.SetResource(next);
        }
    }
}
