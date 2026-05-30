using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class BuildingManagementPanelBridgeSystem : ISystem
    {
        public void Update()
        {
            ref readonly var session = ref CW.GetResource<BuildingPanelSession>();
            var state = BuildingPanelPresentation.Build(in session);
            foreach (var entity in CW.Query<All<BuildingManagementPanelViewData>>().Entities())
            {
                ref var data = ref entity.Mut<BuildingManagementPanelViewData>();
                data.State = state;
                return;
            }

            throw new InvalidOperationException($"{nameof(BuildingManagementPanelViewData)} entity is missing.");
        }
    }
}
