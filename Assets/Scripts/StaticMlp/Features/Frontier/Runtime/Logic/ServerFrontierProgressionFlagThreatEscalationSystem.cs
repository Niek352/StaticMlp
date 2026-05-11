using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Game;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Frontier
{
    public sealed class ServerFrontierProgressionFlagThreatEscalationSystem : ISystem
    {
        private const float RAID_DELAY_SECONDS = 5f;

        private EventReceiver<ServerWT, ProgressFlagAppliedEvent> _flags;

        public void Init()
        {
            _flags = SW.RegisterEventReceiver<ProgressFlagAppliedEvent>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _flags);
        }

        public void Update()
        {
            var simulationTime = SW.GetResource<SimulationTime>();
            foreach (var evt in _flags)
                Handle(simulationTime, in evt.Value);
        }

        private static void Handle(SimulationTime simulationTime, in ProgressFlagAppliedEvent evt)
        {
            if (evt.FlagId != ProgressFlagCatalog.RecoveredWarCacheAppliedId)
                return;

            var anchor = Stage1SettlementProgressionQuery.GetServerAnchor(evt.AnchorId);
            var raidId = RaidCatalog.RaiderCounterattackId;
            var raid = RaidCatalog.Get(raidId);

            ref var threat = ref ReplicationMut.Mut<ThreatState>(anchor);
            threat.Phase = ThreatPhase.RaidPending;
            threat.ThreatValue = (ushort)raid.ThreatValue;

            ref var schedule = ref ReplicationMut.Mut<RaidScheduleState>(anchor);
            schedule.RaidIdValue = raidId.Value;
            schedule.Status = RaidScheduleStatus.Pending;
            schedule.ActivateAtTick = simulationTime.DeadlineAfter(RAID_DELAY_SECONDS);
        }
    }
}
