using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public readonly struct BuildingPanelRoute
    {
        public readonly EntityGID Target;
        public readonly BuildingId BuildingId;
        public readonly BuildingPanelKind Kind;

        public BuildingPanelRoute(EntityGID target, BuildingId buildingId, BuildingPanelKind kind)
        {
            Target = target;
            BuildingId = buildingId;
            Kind = kind;
        }

        public static BuildingPanelRoute Resolve(CW.Entity target)
        {
            ref readonly var state = ref ClientProjection.Read<ConstructionSiteState>(target);
            var buildingId = new BuildingId(state.BuildingId);
            var definition = BuildingCatalogData.Get(buildingId);

            if (state.Phase != ConstructionPhase.Completed)
                return new BuildingPanelRoute(target.GID, buildingId, BuildingPanelKind.ConstructionSitePanel);

            if (buildingId == BuildingCatalogData.StockpileId)
                return new BuildingPanelRoute(target.GID, buildingId, BuildingPanelKind.StockpilePanel);

            if ((definition.Operation.OperationCapabilities & BuildingCapabilityFlags.ExtractsFromNode) != 0)
                return new BuildingPanelRoute(target.GID, buildingId, BuildingPanelKind.ExtractionPanel);

            if ((definition.Operation.OperationCapabilities & BuildingCapabilityFlags.OpensQueue) != 0)
                return new BuildingPanelRoute(target.GID, buildingId, BuildingPanelKind.WorkbenchPanel);

            if ((definition.Operation.OperationCapabilities & BuildingCapabilityFlags.ProvidesRest) != 0)
                return new BuildingPanelRoute(target.GID, buildingId, BuildingPanelKind.ShelterPanel);

            if (buildingId == BuildingCatalogData.CampCoreId)
                return new BuildingPanelRoute(target.GID, buildingId, BuildingPanelKind.CampCorePanel);

            throw new InvalidOperationException($"Building {buildingId.Value} cannot resolve a panel route.");
        }
    }
}
