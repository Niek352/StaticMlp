using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.CampFlow
{
    public sealed class ServerCampFlowProgressionSystem : ISystem
    {
        private EventReceiver<ServerWT, CampFlowRepairCompletedEvent> _repairCompleted;
        private EventReceiver<ServerWT, CampFlowWorkerAssignmentAcceptedEvent> _workerAssigned;
        private EventReceiver<ServerWT, CampFlowStockpilePlacedEvent> _stockpilePlaced;
        private EventReceiver<ServerWT, CampFlowShelterPlacedEvent> _shelterPlaced;
        private EventReceiver<ServerWT, CampFlowExtractionOnlineEvent> _extractionOnline;
        private EventReceiver<ServerWT, CampFlowWorkbenchOnlineEvent> _workbenchOnline;
        private EventReceiver<ServerWT, CampFlowLoadoutPreparedEvent> _buildPrepared;

        public void Init()
        {
            _repairCompleted = SW.RegisterEventReceiver<CampFlowRepairCompletedEvent>();
            _workerAssigned = SW.RegisterEventReceiver<CampFlowWorkerAssignmentAcceptedEvent>();
            _stockpilePlaced = SW.RegisterEventReceiver<CampFlowStockpilePlacedEvent>();
            _shelterPlaced = SW.RegisterEventReceiver<CampFlowShelterPlacedEvent>();
            _extractionOnline = SW.RegisterEventReceiver<CampFlowExtractionOnlineEvent>();
            _workbenchOnline = SW.RegisterEventReceiver<CampFlowWorkbenchOnlineEvent>();
            _buildPrepared = SW.RegisterEventReceiver<CampFlowLoadoutPreparedEvent>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _repairCompleted);
            SW.DeleteEventReceiver(ref _workerAssigned);
            SW.DeleteEventReceiver(ref _stockpilePlaced);
            SW.DeleteEventReceiver(ref _shelterPlaced);
            SW.DeleteEventReceiver(ref _extractionOnline);
            SW.DeleteEventReceiver(ref _workbenchOnline);
            SW.DeleteEventReceiver(ref _buildPrepared);
        }

        public void Update()
        {
            AdvanceAutomaticStages();

            foreach (var evt in _repairCompleted)
                AdvanceFromFact(evt.Value.AnchorId, CampFlowStage.RepairResourcesReady, CampFlowStage.CampRepaired);

            foreach (var evt in _workerAssigned)
                AdvanceFromFact(evt.Value.AnchorId, CampFlowStage.CampRepaired, CampFlowStage.WorkerAssigned);

            foreach (var evt in _stockpilePlaced)
                AdvanceFromFact(evt.Value.AnchorId, CampFlowStage.WorkerAssigned, CampFlowStage.StockpilePlaced);

            foreach (var evt in _shelterPlaced)
                AdvanceFromFact(evt.Value.AnchorId, CampFlowStage.StockpilePlaced, CampFlowStage.ShelterPlaced);

            foreach (var evt in _extractionOnline)
                AdvanceFromFact(evt.Value.AnchorId, CampFlowStage.ShelterPlaced, CampFlowStage.ExtractionOnline);

            foreach (var evt in _workbenchOnline)
                AdvanceFromFact(evt.Value.AnchorId, CampFlowStage.ExtractionOnline, CampFlowStage.WorkbenchOnline);

            foreach (var evt in _buildPrepared)
                AdvanceFromFact(evt.Value.AnchorId, CampFlowStage.WorkbenchOnline, CampFlowStage.LoadoutPrepared);
        }

        private static void AdvanceAutomaticStages()
        {
            foreach (var anchor in SW.Query<All<CampFlowProgression>>().Entities())
            {
                ref readonly var progression = ref anchor.Read<CampFlowProgression>();

                switch (progression.Stage)
                {
                    case CampFlowStage.DamagedCampStart:
                    {
                        ref var mutable = ref ReplicationMut.Mut<CampFlowProgression>(anchor);
                        mutable.AdvanceTo(CampFlowStage.RepairObjectiveActive);
                        break;
                    }
                    case CampFlowStage.RepairObjectiveActive:
                    {
                        if (!HasRequiredRepairResources(progression.Anchor))
                            break;

                        ref var mutable = ref ReplicationMut.Mut<CampFlowProgression>(anchor);
                        mutable.AdvanceTo(CampFlowStage.RepairResourcesReady);
                        break;
                    }
                }
            }
        }

        private static void AdvanceFromFact(
            SettlementAnchorId anchorId,
            CampFlowStage expectedStage,
            CampFlowStage nextStage)
        {
            var anchor = CampFlowProgressionQuery.GetServerAnchor(anchorId);
            ref readonly var current = ref anchor.Read<CampFlowProgression>();
            if (current.Stage != expectedStage)
                return;

            ref var mutable = ref ReplicationMut.Mut<CampFlowProgression>(anchor);
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
                $"Camp flow repair site is missing for settlement anchor {anchorId.Value}.");
        }
    }
}
