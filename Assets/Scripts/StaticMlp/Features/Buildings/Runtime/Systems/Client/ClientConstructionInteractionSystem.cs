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
        private EventReceiver<ClientCoreWT, InteractPressedEvent> _interactEvents;

        public ClientConstructionInteractionSystem(float buildWorkPerSecond = ConstructionActionProfiles.PlayerBuildHoldWorkPerSecond)
        {
            _buildWorkPerSecond = buildWorkPerSecond;
        }

        public void Init()
        {
            _interactEvents = CW.RegisterEventReceiver<InteractPressedEvent>();
        }

        public void Destroy()
        {
            CW.DeleteEventReceiver(ref _interactEvents);
        }

        public void Update()
        {
            // Deposit: реакция на E-press от Interaction фичи
            foreach (var evt in _interactEvents)
            {
                ref readonly var press = ref evt.Value;
                if (press.Kind != InteractableKind.ConstructionSite)
                    continue;

                if (!press.Target.TryUnpack<ClientCoreWT>(out var site))
                    continue;

                SendDeposit(site);
            }

            // Hold-build: прямое чтение input + текущий фокус (поток нажатий, не единичный press)
            ref readonly var focus = ref CW.GetResource<InteractionFocus>();
            if (!focus.HasFocus || focus.Kind != InteractableKind.ConstructionSite)
                return;

            if (!focus.Target.TryUnpack<ClientCoreWT>(out var buildSite))
                return;

            var inputState = CW.GetResource<ClientInputState>();
            if (inputState.IsPressed(BuildingsInputActions.BuildConstruction))
                SendBuild(buildSite, UnityEngine.Time.deltaTime);
        }

        private void SendDeposit(CW.Entity site)
        {
            if (!site.Has<ConstructionResources>())
                return;

            if (ConstructionResourcesAccess.IsProjectedComplete(site))
                return;

            var request = new DepositConstructionResourcesRequestEvent(
                site.GID,
                ConstructionResourcesAccess.GetProjectedRemainingResources(site));

            RequestApi.Send<DepositConstructionResourcesRequestEvent, DepositConstructionResourcesResultEvent>(request);
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
