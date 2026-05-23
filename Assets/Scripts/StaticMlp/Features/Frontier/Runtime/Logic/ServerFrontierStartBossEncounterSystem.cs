using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Frontier
{
    public sealed class ServerFrontierStartBossEncounterSystem : ISystem
    {
        private EventReceiver<ServerWT, NetworkEventFromClient<StartBossEncounterRequestEvent>> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<NetworkEventFromClient<StartBossEncounterRequestEvent>>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _requests);
        }

        public void Update()
        {
            foreach (var evt in _requests)
                Handle(in evt.Value);
        }

        private static void Handle(in NetworkEventFromClient<StartBossEncounterRequestEvent> request)
        {
            if (!ServerPeerPlayers.TryGetPlayer(request.SourcePeer, out var player)
                || !player.Has<PreparedLoadoutSnapshot>())
            {
                return;
            }

            var anchor = CampFlowProgressionQuery.GetServerAnchor(new SettlementAnchorId(request.Value.AnchorId));
            ref readonly var progression = ref anchor.Read<ProgressionState>();
            ref readonly var activeExpedition = ref anchor.Read<ActiveExpeditionState>();
            ref readonly var threat = ref anchor.Read<ThreatState>();
            ref readonly var raidSchedule = ref anchor.Read<RaidScheduleState>();
            ref readonly var bossState = ref anchor.Read<BossEncounterState>();
            ref readonly var bossBuildState = ref anchor.Read<BossLoadoutPreparationState>();

            if (!progression.HasFlag(ProgressFlagCatalog.BossUnlockedId)
                || bossState.Status != BossEncounterStatus.Available
                || bossState.BossIdValue != request.Value.BossId
                || bossBuildState.Status != BossLoadoutPreparationStatus.Committed
                || activeExpedition.Status == ExpeditionActivityStatus.Active
                || threat.Phase == ThreatPhase.RaidPending
                || threat.Phase == ThreatPhase.RaidActive
                || raidSchedule.Status != RaidScheduleStatus.None)
            {
                return;
            }

            var boss = BossCatalog.Get(new BossId(request.Value.BossId));
            FrontierEncounterSpawner.SpawnEncounter(
                request.Value.AnchorId,
                FrontierEncounterKind.Boss,
                request.Value.BossId,
                boss.EncounterProfileId,
                boss.EncounterOrigin);

            ref var mutableBossState = ref ReplicationMut.Mut<BossEncounterState>(anchor);
            mutableBossState.Status = BossEncounterStatus.Active;
        }
    }
}
