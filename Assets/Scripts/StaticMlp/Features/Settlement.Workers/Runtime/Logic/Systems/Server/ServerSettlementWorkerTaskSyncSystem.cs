using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Features.Settlement.Workers
{
    public sealed class ServerSettlementWorkerTaskSyncSystem : ISystem
    {
        private readonly AiTaskExecutionTransitions _transitions = new();

        public void Update()
        {
            foreach (var worker in SW.Query<All<ServerOwned, SettlementWorkerTag, SettlementWorkerAssignment, AiBrain, AiTaskState>>().Entities())
            {
                ref readonly var assignment = ref worker.Read<SettlementWorkerAssignment>();
                ref var task = ref worker.Mut<AiTaskState>();

                if (!assignment.IsAssigned
                    || !Stage1SettlementProgressionQuery.TryGetServerAnchor(assignment.Anchor, out var anchor)
                    || !anchor.Has<SettlementCampBuilderJobState>())
                {
                    ClearTargets(worker);
                    _transitions.SwitchToIdle(worker, ref task);
                    continue;
                }

                ref readonly var jobState = ref anchor.Read<SettlementCampBuilderJobState>();
                if (jobState.AssignedWorker != worker.GID
                    || !jobState.TargetSite.TryUnpack<ServerWT>(out _)
                    || jobState.CurrentTask == AiTaskType.Idle)
                {
                    ClearTargets(worker);
                    _transitions.SwitchToIdle(worker, ref task);
                    continue;
                }

                ApplyTarget(worker, in jobState);
                if (task.Task != jobState.CurrentTask)
                    _transitions.SwitchTo(worker, ref task, jobState.CurrentTask);
            }
        }

        private static void ApplyTarget(SW.Entity worker, in SettlementCampBuilderJobState jobState)
        {
            switch (jobState.CurrentTask)
            {
                case AiTaskType.DeliveryResourceToBuilding:
                    AiBlackboardAccess.SetEntity(worker, DeliveryBuildResourcesCollectVariables.TargetSite, jobState.TargetSite);
                    AiBlackboardAccess.Remove(worker, BuildConstructionCollectVariables.BuildTargetSite);
                    break;
                case AiTaskType.BuildConstruction:
                    AiBlackboardAccess.SetEntity(worker, BuildConstructionCollectVariables.BuildTargetSite, jobState.TargetSite);
                    AiBlackboardAccess.Remove(worker, DeliveryBuildResourcesCollectVariables.TargetSite);
                    break;
                default:
                    ClearTargets(worker);
                    break;
            }
        }

        private static void ClearTargets(SW.Entity worker)
        {
            AiBlackboardAccess.Remove(worker, DeliveryBuildResourcesCollectVariables.TargetSite);
            AiBlackboardAccess.Remove(worker, BuildConstructionCollectVariables.BuildTargetSite);
        }
    }
}
