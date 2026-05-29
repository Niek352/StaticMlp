using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class ClientSettlementUnlockState : IResource, ISettlementUnlockReadModel
    {
        public int SettlementLevel => CW.GetResource<SettlementProgressionState>().SettlementLevel;

        public bool HasConstructedBuilding(int buildingIdValue)
        {
            var targetId = new BuildingId((ushort)buildingIdValue);
            foreach (var entity in CW.Query<All<FinishedBuildingTag, ConstructionSiteState>>().Entities())
            {
                if (new BuildingId(entity.Read<ConstructionSiteState>().BuildingId) == targetId)
                    return true;
            }

            return false;
        }
    }
}
