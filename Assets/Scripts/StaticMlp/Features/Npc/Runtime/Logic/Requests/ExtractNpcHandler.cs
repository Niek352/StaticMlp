using StaticMlp.Features.Shared;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Npc
{
    public sealed class ExtractNpcHandler
        : IRequestHandler<ExtractNpcRequestEvent, ExtractNpcResultEvent>
    {
        private const float INTERACTION_RANGE = 4f;

        public ExtractNpcResultEvent Handle(NetworkPeerId sourcePeer, in ExtractNpcRequestEvent request)
        {
            var rejected = new ExtractNpcResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Rejected,
                Target = request.Target,
                RosterRecord = default,
                AcquisitionResult = NpcAcquisitionResult.Rejected
            };

            if (!request.Target.TryUnpack<ServerWT>(out var target))
                return rejected;

            if (!target.Has<ExtractableState>())
                return rejected;

            ref readonly var extractable = ref target.Read<ExtractableState>();
            var simulationTime = SW.GetResource<SimulationTime>();
            if (extractable.IsExpired(simulationTime.ServerTick))
                return rejected;

            if (!NpcDefinitionCatalog.TryGet(extractable.Definition, out var definition))
                return rejected;

            if ((definition.AllowedAcquisitionPaths & NpcAcquisitionPathFlags.Extraction) == 0)
                return rejected;

            if (target.Has<Health>() && target.Read<Health>().Current <= 0f)
                return rejected;

            if (!target.Has<CharacterNetState>())
                return rejected;

            var targetPosition = target.Read<CharacterNetState>().Position;
            if (!ServerPeerPlayers.IsPlayerNear(sourcePeer, targetPosition, INTERACTION_RANGE))
                return rejected;

            var factory = SW.GetResource<NpcRosterRecordFactory>();
            var rosterRecord = factory.Spawn(new NpcRosterRecordSpawnSpec(
                definition.Id.Value,
                definition.Class,
                NpcAcquisitionPath.Extraction,
                NpcRosterState.Captured,
                simulationTime.ServerTick));

            SW.SendEvent(new NpcAcquisitionAcceptedEvent(rosterRecord, NpcAcquisitionPath.Extraction));
            target.Delete<ExtractableState>();
            target.Delete<ExtractionTargetTag>();

            return new ExtractNpcResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Accepted,
                Target = request.Target,
                RosterRecord = rosterRecord,
                AcquisitionResult = NpcAcquisitionResult.Accepted
            };
        }
    }
}
