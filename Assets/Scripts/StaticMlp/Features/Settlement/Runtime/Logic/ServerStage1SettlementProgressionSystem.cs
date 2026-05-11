using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    public sealed class ServerStage1SettlementProgressionSystem : ISystem
    {
        public void Update()
        {
            foreach (var anchor in SW
                         .Query<All<Stage1SettlementProgression, ConstructionSiteState, ConstructionResources>>()
                         .Entities())
            {
                ref readonly var currentProgression = ref anchor.Read<Stage1SettlementProgression>();

                switch (currentProgression.Stage)
                {
                    case Stage1SettlementProgressStage.DamagedCampStart:
                    {
                        ref var progression = ref ReplicationMut.Mut<Stage1SettlementProgression>(anchor);
                        progression.AdvanceTo(Stage1SettlementProgressStage.RepairObjectiveActive);
                        return;
                    }
                    case Stage1SettlementProgressStage.RepairObjectiveActive:
                    {
                        ref readonly var resources = ref anchor.Read<ConstructionResources>();
                        var storageEntity = SettlementSharedResourcesQuery.GetServerEntity();
                        ref readonly var storage = ref storageEntity.Read<SettlementSharedResources>();
                        if (storage.GetAmount(ResourceCatalog.WoodId) < resources.RemainingWood
                            || storage.GetAmount(ResourceCatalog.StoneId) < resources.RemainingStone)
                        {
                            return;
                        }

                        ref var progression = ref ReplicationMut.Mut<Stage1SettlementProgression>(anchor);
                        progression.AdvanceTo(Stage1SettlementProgressStage.RepairResourcesReady);
                        return;
                    }
                }
            }
        }
    }
}
