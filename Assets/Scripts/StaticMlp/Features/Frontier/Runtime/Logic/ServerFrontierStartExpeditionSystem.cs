using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Frontier
{
    public sealed class ServerFrontierStartExpeditionSystem : ISystem
    {
        private EventReceiver<ServerWT, NetworkEventFromClient<StartExpeditionRequestEvent>> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<NetworkEventFromClient<StartExpeditionRequestEvent>>();
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

        private static void Handle(in NetworkEventFromClient<StartExpeditionRequestEvent> request)
        {
            if (!ServerPeerPlayers.TryGetPlayer(request.SourcePeer, out var player)
                || !player.Has<PreparedLoadoutSnapshot>())
            {
                return;
            }

            var anchorId = new SettlementAnchorId(request.Value.AnchorId);
            var expeditionId = new ExpeditionId(request.Value.ExpeditionId);
            var anchor = CampFlowProgressionQuery.GetServerAnchor(anchorId);
            ref readonly var progression = ref anchor.Read<CampFlowProgression>();
            ref readonly var availability = ref anchor.Read<ExpeditionAvailabilityState>();
            ref readonly var activeExpedition = ref anchor.Read<ActiveExpeditionState>();
            ref readonly var threat = ref anchor.Read<ThreatState>();
            ref readonly var raidSchedule = ref anchor.Read<RaidScheduleState>();

            if (progression.Stage != CampFlowStage.LoadoutPrepared
                || availability.Status != ExpeditionAvailabilityStatus.Available
                || availability.ExpeditionIdValue != expeditionId.Value
                || activeExpedition.Status == ExpeditionActivityStatus.Active
                || threat.Phase == ThreatPhase.RaidPending
                || threat.Phase == ThreatPhase.RaidActive
                || raidSchedule.Status != RaidScheduleStatus.None)
            {
                return;
            }

            var expedition = ExpeditionCatalog.Get(expeditionId);
            FrontierEncounterSpawner.SpawnEncounter(
                anchorId.Value,
                FrontierEncounterKind.Expedition,
                expeditionId.Value,
                expedition.EncounterProfileId,
                expedition.EncounterOrigin);

            ref var mutableActive = ref ReplicationMut.Mut<ActiveExpeditionState>(anchor);
            mutableActive.ExpeditionIdValue = expeditionId.Value;
            mutableActive.Status = ExpeditionActivityStatus.Active;

            ref var mutableAvailability = ref ReplicationMut.Mut<ExpeditionAvailabilityState>(anchor);
            mutableAvailability.ExpeditionIdValue = expeditionId.Value;
            mutableAvailability.Status = ExpeditionAvailabilityStatus.Unavailable;
        }
    }
}
