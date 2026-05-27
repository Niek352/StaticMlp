using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public static class AvailableBuildingsQuery
    {
        public static IEnumerable<BuildingDefinition> Filter(IEnumerable<BuildingDefinition> definitions)
        {
            var state = ResolveState();
            foreach (var definition in definitions)
            {
                if (UnlockEvaluation.IsMet(in definition.UnlockRequirement, state))
                    yield return definition;
            }
        }

        public static bool IsAvailable(in BuildingDefinition definition)
        {
            var state = ResolveState();
            return UnlockEvaluation.IsMet(in definition.UnlockRequirement, state);
        }

        private static ISettlementUnlockReadModel ResolveState()
        {
            return new ClientUnlockReadModel(CW.GetResource<SettlementProgressionState>());
        }

        private sealed class ClientUnlockReadModel : ISettlementUnlockReadModel
        {
            private readonly SettlementProgressionState _progression;

            public ClientUnlockReadModel(SettlementProgressionState progression)
            {
                _progression = progression;
            }

            public int SettlementLevel => _progression.SettlementLevel;

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
}
