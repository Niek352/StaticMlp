using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Game.Components.Buildings;
using StaticMlp.Game.Systems;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;
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
            if (NetworkRuntime.LocalPeerId.Value == 0)
                return;

            if (!TryGetLocalPlayerPosition(out var playerPosition))
                return;

            if (!TryFindNearestSite(playerPosition, out var site))
                return;

            if (BuildingPlacementInput.DepositResourcesWasPressedProvider())
                SendDeposit(site);

            if (BuildingPlacementInput.BuildConstructionIsPressedProvider())
                SendBuild(site);
        }

        private void SendDeposit(CW.Entity site)
        {
            if (!site.Has<ConstructionResources>())
                return;

            var resources = site.Read<ConstructionResources>();
            if (resources.IsComplete)
                return;

            var request = new DepositConstructionResourcesRequestEvent(
                site.GID,
                resources.RemainingWood,
                resources.RemainingStone);

            ref var outbox = ref CW.GetResource<NetOutbox>();
            outbox.EnqueueNetworkEvent(
                new NetworkPeerId(0),
                GameplayEventTypeIds.DepositConstructionResourcesRequest,
                ConstructionEventCodec.Write(request),
                NetDelivery.ReliableSequenced);
        }

        private void SendBuild(CW.Entity site)
        {
            if (!site.Has<ConstructionSiteState>() || !site.Has<ConstructionResources>())
                return;

            var state = site.Read<ConstructionSiteState>();
            var resources = site.Read<ConstructionResources>();
            if (!resources.IsComplete
                || (state.Phase != ConstructionPhase.ReadyToBuild
                    && state.Phase != ConstructionPhase.BuildingInProgress))
                return;

            var request = new BuildConstructionRequestEvent(
                site.GID,
                _buildWorkPerSecond * Time.deltaTime);

            ref var outbox = ref CW.GetResource<NetOutbox>();
            outbox.EnqueueNetworkEvent(
                new NetworkPeerId(0),
                GameplayEventTypeIds.BuildConstructionRequest,
                ConstructionEventCodec.Write(request),
                NetDelivery.ReliableSequenced);
        }

        private bool TryFindNearestSite(Vector3 playerPosition, out CW.Entity site)
        {
            var bestDistanceSq = _interactionRange * _interactionRange;
            var found = false;
            site = default;

            foreach (var e in CW.Query<All<ConstructionSiteTag, ConstructionTransform, ConstructionSiteState>>().Entities())
            {
                var state = e.Read<ConstructionSiteState>();
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

        private static bool TryGetLocalPlayerPosition(out Vector3 position)
        {
            foreach (var e in CW.Query<All<LocalOwned, PlayerTag, CharacterNetState>>().Entities())
            {
                position = e.Read<CharacterNetState>().Position;
                return true;
            }

            position = default;
            return false;
        }
    }
}
