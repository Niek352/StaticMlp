using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Buildings;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class ClientBuildingManagementPanelLifetimeSystem : ISystem
    {
        public void Update()
        {
            ref var session = ref CW.GetResource<BuildingManagementPanelSession>();
            if (!session.IsOpen)
                return;

            if (session.Target.TryUnpack<ClientCoreWT>(out var target)
                && target.Has<ConstructionSiteState>())
            {
                return;
            }

            session.Close();
            ref var operationIntent = ref CW.GetResource<BuildingManagementOperationOpenIntent>();
            operationIntent.Clear();
        }
    }
}
