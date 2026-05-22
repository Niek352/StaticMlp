using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Interaction;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Input;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Buildings
{
    public sealed class ClientConstructionInteractionSystem : ISystem
    {
        private readonly float _buildWorkPerSecond;

        public ClientConstructionInteractionSystem(float buildWorkPerSecond = ConstructionActionProfiles.PlayerBuildHoldWorkPerSecond)
        {
            _buildWorkPerSecond = buildWorkPerSecond;
        }

        public void Update()
        {
            ref readonly var focus = ref CW.GetResource<InteractionFocus>();
            if (!focus.HasFocus || focus.Kind != InteractableKind.ConstructionSite)
                return;

            if (!focus.Target.TryUnpack<ClientCoreWT>(out var buildSite))
                return;

            var inputState = CW.GetResource<ClientInputState>();
            if (inputState.IsPressed(BuildingsInputActions.BuildConstruction))
                SendBuild(buildSite, UnityEngine.Time.deltaTime);
        }

        private void SendBuild(CW.Entity site, float deltaTime)
        {
            if (!site.Has<ConstructionSiteState>() || !site.Has<ConstructionResources>())
                return;

            ref readonly var state = ref ClientProjection.Read<ConstructionSiteState>(site);
            if (!ConstructionResourcesAccess.IsProjectedComplete(site)
                || (state.Phase != ConstructionPhase.ReadyToBuild
                    && state.Phase != ConstructionPhase.BuildingInProgress))
                return;

            var request = new BuildConstructionRequestEvent(
                site.GID,
                _buildWorkPerSecond * deltaTime);

            RequestApi.Send<BuildConstructionRequestEvent, BuildConstructionResultEvent>(request);
        }
    }
}
