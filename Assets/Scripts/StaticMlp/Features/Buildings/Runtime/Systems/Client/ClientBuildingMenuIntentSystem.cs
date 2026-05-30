using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    public sealed class ClientBuildingMenuIntentSystem : ISystem
    {
        private EventReceiver<ClientCoreWT, BuildingMenuCloseIntent> _closeIntents;
        private EventReceiver<ClientCoreWT, BuildingMenuSelectIntent> _selectIntents;

        public void Init()
        {
            _closeIntents = CW.RegisterEventReceiver<BuildingMenuCloseIntent>();
            _selectIntents = CW.RegisterEventReceiver<BuildingMenuSelectIntent>();
        }

        public void Destroy()
        {
            CW.DeleteEventReceiver(ref _closeIntents);
            CW.DeleteEventReceiver(ref _selectIntents);
        }

        public void Update()
        {
            foreach (var _ in _closeIntents)
                CloseMenu();

            foreach (var evt in _selectIntents)
                SelectBuilding(evt.Value.BuildingId);
        }

        private static void CloseMenu()
        {
            ref var state = ref CW.GetResource<BuildingMenuState>();
            state.IsOpen = false;
            state.ClearSelection();
        }

        private static void SelectBuilding(BuildingId buildingId)
        {
            var definition = BuildingCatalogData.Get(buildingId);
            if (!UnlockEvaluation.IsMet(in definition.UnlockRequirement, CW.GetResource<ClientSettlementUnlockState>()))
                throw new InvalidOperationException($"Cannot select locked building {buildingId.Value}.");

            ref var state = ref CW.GetResource<BuildingMenuState>();
            state.Select(buildingId);
        }
    }
}
