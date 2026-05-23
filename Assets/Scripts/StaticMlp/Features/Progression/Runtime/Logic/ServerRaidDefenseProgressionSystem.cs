using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Progression
{
    public sealed class ServerRaidDefenseProgressionSystem : ISystem
    {
        private EventReceiver<ServerWT, RaidDefenseResolvedEvent> _raidDefenses;

        public void Init()
        {
            _raidDefenses = SW.RegisterEventReceiver<RaidDefenseResolvedEvent>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _raidDefenses);
        }

        public void Update()
        {
            foreach (var evt in _raidDefenses)
                Handle(in evt.Value);
        }

        private static void Handle(in RaidDefenseResolvedEvent evt)
        {
            var anchor = CampFlowProgressionQuery.GetServerAnchor(evt.AnchorId);
            ref var progression = ref anchor.Mut<ProgressionState>();
            if (progression.HasFlag(ProgressFlagCatalog.CounterattackDefendedId))
                return;

            progression.ApplyFlag(ProgressFlagCatalog.CounterattackDefendedId);
            SW.SendEvent(new ProgressFlagAppliedEvent(evt.AnchorId, ProgressFlagCatalog.CounterattackDefendedId));
        }
    }
}
