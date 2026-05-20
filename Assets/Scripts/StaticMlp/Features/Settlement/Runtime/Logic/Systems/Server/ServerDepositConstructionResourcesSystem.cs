using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    public sealed class ServerDepositConstructionResourcesSystem : ISystem
    {
        private EventReceiver<ServerWT, DepositConstructionResourcesEvent> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<DepositConstructionResourcesEvent>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _requests);
        }

        public void Update()
        {
            foreach (var request in _requests)
                Handle(in request.Value);
        }

        private static void Handle(in DepositConstructionResourcesEvent request)
        {
            if (!request.Site.TryUnpack<ServerWT>(out var site))
                throw new InvalidOperationException($"Construction resource deposit target {request.Site} is not a server entity.");

            var storageEntity = SettlementSharedResourcesQuery.GetServerEntity();

            ref readonly var currentState = ref site.Read<ConstructionSiteState>();
            ref readonly var currentResources = ref site.Read<ConstructionResources>();
            ref readonly var currentStorage = ref storageEntity.Read<SettlementSharedResources>();
            if (!SettlementConstructionRules.TryPlanResourceDeposit(
                    in currentState,
                    in currentResources,
                    currentStorage.GetAmount(ResourceCatalog.WoodId),
                    currentStorage.GetAmount(ResourceCatalog.StoneId),
                    currentStorage.GetAmount(ResourceCatalog.PlanksId),
                    currentStorage.GetAmount(ResourceCatalog.SimplePartsId),
                    request.Wood,
                    request.Stone,
                    request.Planks,
                    request.SimpleParts,
                    out var acceptedWood,
                    out var acceptedStone,
                    out var acceptedPlanks,
                    out var acceptedSimpleParts))
                return;

            ref var storage = ref ReplicationMut.Mut<SettlementSharedResources>(storageEntity);
            var spentWood = storage.Spend(ResourceCatalog.WoodId, acceptedWood);
            var spentStone = storage.Spend(ResourceCatalog.StoneId, acceptedStone);
            var spentPlanks = storage.Spend(ResourceCatalog.PlanksId, acceptedPlanks);
            var spentSimpleParts = storage.Spend(ResourceCatalog.SimplePartsId, acceptedSimpleParts);

            ref var state = ref ReplicationMut.Mut<ConstructionSiteState>(site);
            ref var resources = ref ReplicationMut.Mut<ConstructionResources>(site);
            SettlementConstructionRules.ApplyResourceDeposit(
                ref state,
                ref resources,
                spentWood,
                spentStone,
                spentPlanks,
                spentSimpleParts);
        }
    }
}
