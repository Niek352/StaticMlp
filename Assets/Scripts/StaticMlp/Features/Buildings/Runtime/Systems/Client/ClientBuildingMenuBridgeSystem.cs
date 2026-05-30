using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    public sealed class ClientBuildingMenuBridgeSystem : ISystem
    {
        public void Update()
        {
            ref readonly var state = ref CW.GetResource<BuildingMenuState>();
            var presentation = BuildingMenuPresentation.Create(in state, CW.GetResource<ClientSettlementUnlockState>());

            foreach (var entity in CW.Query<All<BuildingMenuViewData>>().Entities())
            {
                ref var data = ref entity.Mut<BuildingMenuViewData>();
                data.Presentation = presentation;
                return;
            }

            throw new InvalidOperationException($"{nameof(BuildingMenuViewData)} entity is missing.");
        }
    }
}
