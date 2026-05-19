using StaticMlp.Game;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Npc
{
    public sealed class RescueNpcHandler
        : IRequestHandler<RescueNpcRequestEvent, RescueNpcResultEvent>
    {
        private const float INTERACTION_RANGE = 4f;

        public RescueNpcResultEvent Handle(NetworkPeerId sourcePeer, in RescueNpcRequestEvent request)
        {
            var rejected = new RescueNpcResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Rejected,
                RescueSite = request.RescueSite,
                RosterRecord = default,
                AcquisitionResult = NpcAcquisitionResult.Rejected
            };

            if (!request.RescueSite.TryUnpack<ServerWT>(out var site))
                return rejected;

            if (!site.Has<NpcRescueSite>())
                return rejected;

            ref var rescueSite = ref site.Mut<NpcRescueSite>();
            if (rescueSite.State != NpcRescueSiteState.Rescuable)
                return rejected;

            if (!NpcDefinitionCatalog.TryGet(rescueSite.Definition, out var definition))
                return rejected;

            if ((definition.AllowedAcquisitionPaths & NpcAcquisitionPathFlags.Rescue) == 0)
                return rejected;

            if (!ServerPeerPlayers.IsPlayerNear(sourcePeer, rescueSite.Position, INTERACTION_RANGE))
                return rejected;

            rescueSite.State = NpcRescueSiteState.Resolved;

            var simulationTime = SW.GetResource<SimulationTime>();
            var factory = SW.GetResource<NpcRosterRecordFactory>();
            var rosterRecord = factory.Spawn(new NpcRosterRecordSpawnSpec(
                definition.Id.Value,
                definition.Class,
                NpcAcquisitionPath.Rescue,
                NpcRosterState.Recruited,
                simulationTime.ServerTick));

            SW.SendEvent(new NpcAcquisitionAcceptedEvent(rosterRecord, NpcAcquisitionPath.Rescue));

            return new RescueNpcResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Accepted,
                RescueSite = request.RescueSite,
                RosterRecord = rosterRecord,
                AcquisitionResult = NpcAcquisitionResult.Accepted
            };
        }
    }
}
