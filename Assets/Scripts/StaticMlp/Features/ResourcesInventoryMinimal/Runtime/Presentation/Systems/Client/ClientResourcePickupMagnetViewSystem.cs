using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public sealed class ClientResourcePickupMagnetViewSystem : ISystem
    {
        public void Update()
        {
            if (!TryGetLocalPlayerPosition(out var playerPosition))
                return;

            var config = CW.GetResource<ResourcesInventoryConfig>();
            var magnetRadiusSq = config.PickupMagnetRadius * config.PickupMagnetRadius;
            var step = config.PickupMagnetSpeed * Time.deltaTime;

            foreach (var pickup in CW.Query<All<ResourcePickup, ViewTransform, ResourcePickupViewState>>().Entities())
            {
                ref readonly var pickupState = ref pickup.Read<ResourcePickup>();
                ref var viewTransform = ref pickup.Mut<ViewTransform>();
                ref var viewState = ref pickup.Mut<ResourcePickupViewState>();

                if (viewState.ResourceId == 0 && viewState.Amount == 0)
                    viewTransform.RenderPosition = pickupState.Position;

                var isMagnetized = (playerPosition - pickupState.Position).sqrMagnitude <= magnetRadiusSq;
                viewTransform.RenderPosition = isMagnetized
                    ? Vector3.MoveTowards(viewTransform.RenderPosition, playerPosition, step)
                    : pickupState.Position;

                viewState.ResourceId = pickupState.ResourceId;
                viewState.Amount = pickupState.Amount;
                viewState.IsMagnetized = isMagnetized;
            }
        }

        private static bool TryGetLocalPlayerPosition(out Vector3 position)
        {
            foreach (var player in CW.Query<All<LocalOwned, PlayerTag, CharacterNetState>>().Entities())
            {
                position = player.Read<CharacterNetState>().Position;
                return true;
            }

            position = default;
            return false;
        }
    }
}
