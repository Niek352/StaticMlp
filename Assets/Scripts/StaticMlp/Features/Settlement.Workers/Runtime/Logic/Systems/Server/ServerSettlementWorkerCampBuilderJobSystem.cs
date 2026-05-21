using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement.Workers
{
    public sealed class ServerSettlementWorkerCampBuilderJobSystem : ISystem
    {
        public void Update()
        {
            foreach (var anchor in SW.Query<All<Stage1SettlementProgression>>().Entities())
            {
                ref readonly var progression = ref anchor.Read<Stage1SettlementProgression>();
                var assignedWorker = FindAssignedCampBuilder(progression.Anchor);

                var jobState = CreateJobState(progression.Anchor, assignedWorker, in progression);
                var summary = CreateSummary(progression.Anchor, jobState, assignedWorker);

                ref var mutableJobState = ref ReplicationMut.Mut<SettlementCampBuilderJobState>(anchor);
                mutableJobState = jobState;

                ref var mutableSummary = ref ReplicationMut.Mut<SettlementWorkerSummary>(anchor);
                mutableSummary = summary;
            }
        }

        private static SettlementCampBuilderJobState CreateJobState(
            SettlementAnchorId anchorId,
            EntityGID assignedWorker,
            in Stage1SettlementProgression progression)
        {
            var state = new SettlementCampBuilderJobState
            {
                AnchorId = anchorId.Value,
                AssignedWorker = assignedWorker,
                TargetSite = default,
                CurrentTask = AiTaskType.Idle,
                BlockingReason = SettlementWorkerBlockingReason.None
            };

            if (!assignedWorker.TryUnpack<ServerWT>(out var worker))
            {
                state.BlockingReason = SettlementWorkerBlockingReason.NoAssignment;
                return state;
            }

            var role = WorkerRoleCatalog.Get(worker.Read<SettlementWorkerIdentity>().Role);

            if ((byte)progression.Stage < (byte)Stage1SettlementProgressStage.CampRepaired)
            {
                state.BlockingReason = SettlementWorkerBlockingReason.AwaitingCampRepair;
                return state;
            }

            if ((role.AllowedJobs & WorkerJobFlags.DeliverConstructionResources) != 0
                && TryFindDeliverySite(worker, out var deliverySite))
            {
                state.TargetSite = deliverySite;
                state.CurrentTask = AiTaskType.DeliveryResourceToBuilding;
                return state;
            }

            if ((role.AllowedJobs & WorkerJobFlags.BuildConstruction) != 0
                && TryFindBuildSite(worker, out var buildSite))
            {
                state.TargetSite = buildSite;
                state.CurrentTask = AiTaskType.BuildConstruction;
                return state;
            }

            state.BlockingReason = HasAnyResourceWaitingSite()
                ? SettlementWorkerBlockingReason.MissingResources
                : SettlementWorkerBlockingReason.NoConstructionDemand;
            return state;
        }

        private static SettlementWorkerSummary CreateSummary(
            SettlementAnchorId anchorId,
            SettlementCampBuilderJobState jobState,
            EntityGID assignedWorker)
        {
            var summary = new SettlementWorkerSummary
            {
                AnchorId = anchorId.Value,
                ActiveTask = jobState.CurrentTask,
                BlockingReason = jobState.BlockingReason
            };

            foreach (var worker in SW.Query<All<SettlementWorkerTag, SettlementWorkerIdentity, SettlementWorkerAssignment>>().Entities())
            {
                ref readonly var identity = ref worker.Read<SettlementWorkerIdentity>();
                if (identity.HomeAnchorId != anchorId.Value)
                    continue;

                summary.TotalWorkers++;

                if (identity.Role == WorkerRoleCatalog.CampBuilderId)
                    summary.CampBuilderWorkers++;

                ref readonly var assignment = ref worker.Read<SettlementWorkerAssignment>();
                if (!assignment.IsAssigned || assignment.AnchorId != anchorId.Value)
                    continue;

                summary.AssignedWorkers++;

                if (identity.Role == WorkerRoleCatalog.CampBuilderId)
                    summary.CampBuilderAssignedWorkers++;
            }

            if (!assignedWorker.TryUnpack<ServerWT>(out _))
                summary.BlockingReason = SettlementWorkerBlockingReason.NoAssignment;

            return summary;
        }

        private static EntityGID FindAssignedCampBuilder(SettlementAnchorId anchorId)
        {
            foreach (var worker in SW.Query<All<SettlementWorkerTag, SettlementWorkerIdentity, SettlementWorkerAssignment>>().Entities())
            {
                ref readonly var identity = ref worker.Read<SettlementWorkerIdentity>();
                if (identity.HomeAnchorId != anchorId.Value || identity.Role != WorkerRoleCatalog.CampBuilderId)
                    continue;

                ref readonly var assignment = ref worker.Read<SettlementWorkerAssignment>();
                if (!assignment.IsAssigned || assignment.AnchorId != anchorId.Value)
                    continue;

                return worker.GID;
            }

            return default;
        }

        private static bool TryFindDeliverySite(SW.Entity worker, out EntityGID siteGid)
        {
            ref readonly var workerState = ref worker.Read<StaticMlp.Game.Components.CharacterNetState>();
            var storageEntity = SettlementSharedResourcesQuery.GetServerEntity();
            var bestDistanceSq = float.MaxValue;
            siteGid = default;

            foreach (var site in SW.Query<All<ConstructionSiteTag, ConstructionSiteState, ConstructionResources, ConstructionTransform>>().Entities())
            {
                ref readonly var siteState = ref site.Read<ConstructionSiteState>();
                if (!SettlementConstructionRules.TryPlanResourceDeposit(
                        in siteState,
                        site,
                        storageEntity,
                        ConstructionResourcesAccess.GetRemainingResources(site),
                        out _))
                {
                    continue;
                }

                ref readonly var transform = ref site.Read<ConstructionTransform>();
                var distanceSq = (transform.Position - workerState.Position).sqrMagnitude;
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                siteGid = site.GID;
            }

            return siteGid.TryUnpack<ServerWT>(out _);
        }

        private static bool TryFindBuildSite(SW.Entity worker, out EntityGID siteGid)
        {
            ref readonly var workerState = ref worker.Read<StaticMlp.Game.Components.CharacterNetState>();
            var bestDistanceSq = float.MaxValue;
            siteGid = default;

            foreach (var site in SW.Query<All<ConstructionSiteTag, ConstructionSiteState, ConstructionResources, ConstructionTransform, ConstructionProgress>>().Entities())
            {
                ref readonly var siteState = ref site.Read<ConstructionSiteState>();
                if (!ConstructionRules.CanBuild(site, in siteState))
                    continue;

                ref readonly var transform = ref site.Read<ConstructionTransform>();
                var distanceSq = (transform.Position - workerState.Position).sqrMagnitude;
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                siteGid = site.GID;
            }

            return siteGid.TryUnpack<ServerWT>(out _);
        }

        private static bool HasAnyResourceWaitingSite()
        {
            foreach (var site in SW.Query<All<ConstructionSiteTag, ConstructionSiteState>>().Entities())
            {
                ref readonly var siteState = ref site.Read<ConstructionSiteState>();
                if (SettlementConstructionRules.CanDepositResources(in siteState))
                    return true;
            }

            return false;
        }
    }
}
