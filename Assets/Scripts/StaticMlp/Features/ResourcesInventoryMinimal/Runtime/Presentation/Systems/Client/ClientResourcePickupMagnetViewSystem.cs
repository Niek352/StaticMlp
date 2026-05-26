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
        private const float VISUAL_CONSUME_DISTANCE = 0.2f;

        public void Update()
        {
            if (!TryGetLocalPlayerPosition(out var playerPosition))
                return;

            var config = CW.GetResource<ResourcesInventoryConfig>();
            var step = config.PickupMagnetSpeed * Time.deltaTime;

            foreach (var pickup in CW.Query<All<ResourcePickup, ViewTransform, ResourcePickupViewState>>().Entities())
            {
                ref readonly var pickupState = ref pickup.Read<ResourcePickup>();
                ref var viewTransform = ref pickup.Mut<ViewTransform>();
                ref var viewState = ref pickup.Mut<ResourcePickupViewState>();

                var isMagnetized = pickupState.IsPickedUp;
                var isConsumed = viewState.IsConsumed;

                if (!isMagnetized)
                {
                    isConsumed = false;
                }
                else if (!isConsumed)
                {
                    viewTransform.RenderPosition = Vector3.MoveTowards(viewTransform.RenderPosition, playerPosition, step);
                    isConsumed = (viewTransform.RenderPosition - playerPosition).sqrMagnitude
                                 <= VISUAL_CONSUME_DISTANCE * VISUAL_CONSUME_DISTANCE;
                }
                // When not magnetized: leave RenderPosition as-is.
                // Initial position is set on spawn by ClientResourcePickupViewBindSystem.

                viewState.ResourceId = pickupState.ResourceId;
                viewState.Amount = pickupState.Amount;
                viewState.IsMagnetized = isMagnetized;
                viewState.IsConsumed = isConsumed;
            }
        }

        private static bool TryGetLocalPlayerPosition(out Vector3 position)
        {
            // Prefer the smoothed render position; it stays current even while standing still.
            foreach (var player in CW.Query<All<LocalOwned, PlayerTag, ViewTransform>>().Entities())
            {
                position = player.Read<ViewTransform>().RenderPosition;
                return true;
            }

            // Fallback: authoritative network position if ViewTransform not yet available.
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
