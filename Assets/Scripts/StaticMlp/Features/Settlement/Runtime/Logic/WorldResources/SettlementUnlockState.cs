using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementUnlockState : IResource, ISettlementUnlockReadModel
    {
        public int SettlementLevel => SW.GetResource<SettlementProgressionState>().SettlementLevel;

        public bool HasConstructedBuilding(int buildingIdValue)
        {
            var targetId = new BuildingId((ushort)buildingIdValue);
            foreach (var entity in SW.Query<All<FinishedBuildingTag, ConstructionSiteState>>().Entities())
            {
                if (new BuildingId(entity.Read<ConstructionSiteState>().BuildingId) == targetId)
                    return true;
            }

            return false;
        }
    }
}
