using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Stage1
{
    public sealed class ServerStage1FlowSystem : ISystem
    {
        private EventReceiver<ServerWT, Stage1RepairCompletedEvent> _repairCompleted;
        private EventReceiver<ServerWT, Stage1WorkerAssignmentAcceptedEvent> _workerAssigned;
        private EventReceiver<ServerWT, Stage1LoadoutPreparedEvent> _buildPrepared;

        public void Init()
        {
            _repairCompleted = SW.RegisterEventReceiver<Stage1RepairCompletedEvent>();
            _workerAssigned = SW.RegisterEventReceiver<Stage1WorkerAssignmentAcceptedEvent>();
            _buildPrepared = SW.RegisterEventReceiver<Stage1LoadoutPreparedEvent>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _repairCompleted);
            SW.DeleteEventReceiver(ref _workerAssigned);
            SW.DeleteEventReceiver(ref _buildPrepared);
        }

        public void Update()
        {
            AdvanceAutomaticStages();

            foreach (var evt in _repairCompleted)
                AdvanceFromFact(evt.Value.AnchorId, Stage1SettlementProgressStage.RepairResourcesReady, Stage1SettlementProgressStage.CampRepaired);

            foreach (var evt in _workerAssigned)
                AdvanceFromFact(evt.Value.AnchorId, Stage1SettlementProgressStage.CampRepaired, Stage1SettlementProgressStage.WorkerAssigned);

            foreach (var evt in _buildPrepared)
                AdvanceFromFact(evt.Value.AnchorId, Stage1SettlementProgressStage.WorkerAssigned, Stage1SettlementProgressStage.LoadoutPrepared);
        }

        private static void AdvanceAutomaticStages()
        {
            foreach (var anchor in SW.Query<All<Stage1SettlementProgression>>().Entities())
            {
                ref readonly var progression = ref anchor.Read<Stage1SettlementProgression>();

                switch (progression.Stage)
                {
                    case Stage1SettlementProgressStage.DamagedCampStart:
                    {
                        ref var mutable = ref ReplicationMut.Mut<Stage1SettlementProgression>(anchor);
                        mutable.AdvanceTo(Stage1SettlementProgressStage.RepairObjectiveActive);
                        break;
                    }
                    case Stage1SettlementProgressStage.RepairObjectiveActive:
                    {
                        if (!HasRequiredRepairResources(progression.Anchor))
                            break;

                        ref var mutable = ref ReplicationMut.Mut<Stage1SettlementProgression>(anchor);
                        mutable.AdvanceTo(Stage1SettlementProgressStage.RepairResourcesReady);
                        break;
                    }
                }
            }
        }

        private static void AdvanceFromFact(
            SettlementAnchorId anchorId,
            Stage1SettlementProgressStage expectedStage,
            Stage1SettlementProgressStage nextStage)
        {
            var anchor = Stage1SettlementProgressionQuery.GetServerAnchor(anchorId);
            ref readonly var current = ref anchor.Read<Stage1SettlementProgression>();
            if (current.Stage != expectedStage)
                return;

            ref var mutable = ref ReplicationMut.Mut<Stage1SettlementProgression>(anchor);
            mutable.AdvanceTo(nextStage);
        }

        private static bool HasRequiredRepairResources(SettlementAnchorId anchorId)
        {
            var repairSite = GetRepairSite(anchorId);
            var storageEntity = SettlementSharedResourcesQuery.GetServerEntity();
            var remaining = ConstructionResourcesAccess.GetRemainingResources(repairSite);
            for (var i = 0; i < remaining.Length; i++)
            {
                if (SettlementSharedResourcesAccess.GetAmount(storageEntity, remaining[i].Id) < remaining[i].Amount)
                    return false;
            }

            return true;
        }

        private static SW.Entity GetRepairSite(SettlementAnchorId anchorId)
        {
            foreach (var site in SW.Query<All<ConstructionSiteTag, SettlementAnchorRef, ConstructionResources>>().Entities())
            {
                if (site.Read<SettlementAnchorRef>().Anchor != anchorId)
                    continue;

                return site;
            }

            throw new System.InvalidOperationException(
                $"Stage 1 repair site is missing for settlement anchor {anchorId.Value}.");
        }
    }
}
