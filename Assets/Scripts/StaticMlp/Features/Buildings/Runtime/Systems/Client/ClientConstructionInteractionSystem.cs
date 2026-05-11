using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Player;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Input;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    public sealed class ClientConstructionInteractionSystem : ISystem
    {
        private readonly float _interactionRange;
        private readonly float _buildWorkPerSecond;

        public ClientConstructionInteractionSystem(float interactionRange = 4f, float buildWorkPerSecond = 35f)
        {
            _interactionRange = interactionRange;
            _buildWorkPerSecond = buildWorkPerSecond;
        }

        public void Update()
        {
            var inputState = CW.GetResource<ClientInputState>();

            if (!ClientLocalPlayer.TryGetPosition(out var playerPosition))
                return;

            if (!TryFindNearestSite(playerPosition, out var site))
                return;

            if (inputState.WasPressed(CoreInputActions.Interact))
                SendDeposit(site);

            if (inputState.IsPressed(BuildingsInputActions.BuildConstruction))
                SendBuild(site);
        }

        private void SendDeposit(CW.Entity site)
        {
            if (!site.Has<ConstructionResources>())
                return;

            ref readonly var resources = ref ClientProjection.Read<ConstructionResources>(site);
            if (resources.IsComplete)
                return;

            var request = new DepositConstructionResourcesRequestEvent(
                site.GID,
                resources.RemainingWood,
                resources.RemainingStone);

            RequestApi.Send(request);
        }

        private void SendBuild(CW.Entity site)
        {
            if (!site.Has<ConstructionSiteState>() || !site.Has<ConstructionResources>())
                return;

            ref readonly var state = ref ClientProjection.Read<ConstructionSiteState>(site);
            ref readonly var resources = ref ClientProjection.Read<ConstructionResources>(site);
            if (!resources.IsComplete
                || (state.Phase != ConstructionPhase.ReadyToBuild
                    && state.Phase != ConstructionPhase.BuildingInProgress))
                return;

            var request = new BuildConstructionRequestEvent(
                site.GID,
                _buildWorkPerSecond * Time.deltaTime);

            RequestApi.Send(request);
        }

        private bool TryFindNearestSite(Vector3 playerPosition, out CW.Entity site)
        {
            var bestDistanceSq = _interactionRange * _interactionRange;
            var found = false;
            site = default;

            foreach (var e in CW.Query<All<ConstructionTransform, ConstructionSiteState>>().Entities())
            {
                ref readonly var state = ref ClientProjection.Read<ConstructionSiteState>(e);
                if (state.Phase == ConstructionPhase.Completed)
                    continue;

                var position = e.Read<ConstructionTransform>().Position;
                var distanceSq = (position - playerPosition).sqrMagnitude;
                if (distanceSq > bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                site = e;
                found = true;
            }

            return found;
        }

    }
}
