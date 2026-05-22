using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement.Workers
{
    public sealed class SetBuildingWorkerAssignmentHandler
        : IRequestHandler<SetBuildingWorkerAssignmentRequestEvent, SetBuildingWorkerAssignmentResultEvent>
    {
        private readonly float _interactionRange;

        public SetBuildingWorkerAssignmentHandler(float interactionRange = 4f)
        {
            _interactionRange = interactionRange;
        }

        public SetBuildingWorkerAssignmentResultEvent Handle(
            NetworkPeerId sourcePeer,
            in SetBuildingWorkerAssignmentRequestEvent request)
        {
            var rejected = new SetBuildingWorkerAssignmentResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Rejected,
                Worker = request.Worker,
                Building = request.Building,
                AssignmentStatus = SettlementWorkerAssignmentStatus.Unassigned,
                SlotIndex = request.SlotIndex
            };

            if (!request.Worker.TryUnpack<ServerWT>(out var worker)
                || !worker.Has<SettlementWorkerTag>()
                || !worker.Has<SettlementWorkerIdentity>()
                || !worker.Has<SettlementWorkerAssignment>()
                || !worker.Has<BuildingWorkerAssignmentState>())
            {
                return rejected;
            }

            if (!request.Building.TryUnpack<ServerWT>(out var building)
                || !building.Has<FinishedBuildingTag>()
                || !building.Has<ConstructionSiteState>()
                || !building.Has<ConstructionTransform>()
                || !building.Has<SettlementAnchorRef>())
            {
                return rejected;
            }

            ref readonly var site = ref building.Read<ConstructionSiteState>();
            if (site.Phase != ConstructionPhase.Completed)
                return rejected;

            ref readonly var transform = ref building.Read<ConstructionTransform>();
            if (!ServerPeerPlayers.IsPlayerNear(sourcePeer, transform.Position, _interactionRange))
                return rejected;

            var definition = BuildingCatalogData.Get(new BuildingId(site.BuildingId));
            if (definition.Operation.WorkerSlots == 0 || request.SlotIndex >= definition.Operation.WorkerSlots)
                return rejected;

            ref readonly var anchorRef = ref building.Read<SettlementAnchorRef>();
            ref readonly var identity = ref worker.Read<SettlementWorkerIdentity>();
            if (identity.HomeAnchorId != anchorRef.AnchorId)
                return rejected;

            if (request.Assigned && IsSlotOccupied(request.Building, request.SlotIndex, request.Worker))
                return rejected;

            ref readonly var current = ref worker.Read<BuildingWorkerAssignmentState>();
            if (!request.Assigned
                && (!current.IsAssigned || current.Building != request.Building || current.SlotIndex != request.SlotIndex))
            {
                return rejected;
            }

            ref var buildingAssignment = ref ReplicationMut.Mut<BuildingWorkerAssignmentState>(worker);
            if (request.Assigned)
            {
                buildingAssignment.Status = SettlementWorkerAssignmentStatus.Assigned;
                buildingAssignment.AnchorId = anchorRef.AnchorId;
                buildingAssignment.Building = request.Building;
                buildingAssignment.SlotIndex = request.SlotIndex;

                ref var settlementAssignment = ref ReplicationMut.Mut<SettlementWorkerAssignment>(worker);
                settlementAssignment.Status = SettlementWorkerAssignmentStatus.Unassigned;
                settlementAssignment.AnchorId = 0;
            }
            else
            {
                buildingAssignment.Status = SettlementWorkerAssignmentStatus.Unassigned;
                buildingAssignment.AnchorId = 0;
                buildingAssignment.Building = default;
                buildingAssignment.SlotIndex = 0;
            }

            return new SetBuildingWorkerAssignmentResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Accepted,
                Worker = request.Worker,
                Building = request.Assigned ? request.Building : default,
                AnchorId = request.Assigned ? anchorRef.AnchorId : (ushort)0,
                SlotIndex = request.Assigned ? request.SlotIndex : (byte)0,
                AssignmentStatus = buildingAssignment.Status
            };
        }

        private static bool IsSlotOccupied(EntityGID building, byte slotIndex, EntityGID requestedWorker)
        {
            foreach (var worker in SW.Query<All<BuildingWorkerAssignmentState>>().Entities())
            {
                if (worker.GID == requestedWorker)
                    continue;

                ref readonly var assignment = ref worker.Read<BuildingWorkerAssignmentState>();
                if (assignment.IsAssigned
                    && assignment.Building == building
                    && assignment.SlotIndex == slotIndex)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
