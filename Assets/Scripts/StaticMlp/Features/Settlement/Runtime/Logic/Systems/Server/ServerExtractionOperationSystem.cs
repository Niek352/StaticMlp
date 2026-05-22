using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    public sealed class ServerExtractionOperationSystem : ISystem
    {
        public void Update()
        {
            foreach (var entity in SW.Query<All<FinishedBuildingTag, ExtractionOperationState>>().Entities())
            {
                ref var state = ref ReplicationMut.Mut<ExtractionOperationState>(entity);
                ExtractionRules.FillBuffer(ref state, CountAssignedWorkers(entity.GID));
            }
        }

        private static int CountAssignedWorkers(EntityGID building)
        {
            var count = 0;
            foreach (var worker in SW.Query<All<BuildingWorkerAssignmentState>>().Entities())
            {
                ref readonly var assignment = ref worker.Read<BuildingWorkerAssignmentState>();
                if (assignment.IsAssigned && assignment.Building == building)
                    count++;
            }

            return count;
        }
    }
}
