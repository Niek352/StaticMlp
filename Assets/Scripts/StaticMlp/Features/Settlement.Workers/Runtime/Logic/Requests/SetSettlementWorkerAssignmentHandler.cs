using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement.Workers
{
    public sealed class SetSettlementWorkerAssignmentHandler
        : IRequestHandler<SetSettlementWorkerAssignmentRequestEvent, SetSettlementWorkerAssignmentResultEvent>
    {
        public SetSettlementWorkerAssignmentResultEvent Handle(
            NetworkPeerId sourcePeer,
            in SetSettlementWorkerAssignmentRequestEvent request)
        {
            var rejected = new SetSettlementWorkerAssignmentResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Rejected,
                Worker = request.Worker,
                AnchorId = request.AnchorId,
                AssignmentStatus = SettlementWorkerAssignmentStatus.Unassigned
            };

            if (!request.Worker.TryUnpack<ServerWT>(out var worker)
                || !worker.Has<SettlementWorkerTag>()
                || !worker.Has<SettlementWorkerIdentity>()
                || !worker.Has<SettlementWorkerAssignment>())
            {
                return rejected;
            }

            ref readonly var identity = ref worker.Read<SettlementWorkerIdentity>();
            if (identity.HomeAnchorId != request.AnchorId)
                return rejected;

            if (!Stage1SettlementProgressionQuery.TryGetServerAnchor(new SettlementAnchorId(request.AnchorId), out var anchor))
                return rejected;

            ref readonly var progression = ref anchor.Read<Stage1SettlementProgression>();
            if (request.Assigned
                && (byte)progression.Stage < (byte)Stage1SettlementProgressStage.CampRepaired)
                return rejected;

            if (request.Assigned && HasOtherAssignedWorker(new SettlementAnchorId(request.AnchorId), worker.GID))
                return rejected;

            ref var assignment = ref ReplicationMut.Mut<SettlementWorkerAssignment>(worker);
            assignment.Status = request.Assigned
                ? SettlementWorkerAssignmentStatus.Assigned
                : SettlementWorkerAssignmentStatus.Unassigned;
            assignment.AnchorId = request.Assigned ? request.AnchorId : (ushort)0;

            if (request.Assigned && identity.Role == WorkerRoleCatalog.CampBuilderId)
                SW.SendEvent(new Stage1WorkerAssignmentAcceptedEvent(new SettlementAnchorId(request.AnchorId)));

            return new SetSettlementWorkerAssignmentResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Accepted,
                Worker = request.Worker,
                AnchorId = request.Assigned ? request.AnchorId : (ushort)0,
                AssignmentStatus = assignment.Status
            };
        }

        private static bool HasOtherAssignedWorker(SettlementAnchorId anchorId, EntityGID requestedWorker)
        {
            foreach (var worker in SW.Query<All<SettlementWorkerTag, SettlementWorkerIdentity, SettlementWorkerAssignment>>().Entities())
            {
                if (worker.GID == requestedWorker)
                    continue;

                ref readonly var assignment = ref worker.Read<SettlementWorkerAssignment>();
                if (!assignment.IsAssigned || assignment.AnchorId != anchorId.Value)
                    continue;

                ref readonly var identity = ref worker.Read<SettlementWorkerIdentity>();
                if (identity.Role != WorkerRoleCatalog.CampBuilderId)
                    continue;

                return true;
            }

            return false;
        }
    }
}
