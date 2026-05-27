using FFS.Libraries.StaticEcs;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public sealed class ClientResourcePickupMagnetViewSystem : ISystem
    {
        private const float VISUAL_CONSUME_DISTANCE = 0.2f;

        public void Update()
        {
            var config = CW.GetResource<ResourcesInventoryConfig>();
            var step = config.PickupMagnetSpeed * CW.GetResource<GameTime>().DeltaTime;

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
                    if (TryGetCollectorPosition(pickupState.CollectorPlayer, out var collectorPosition))
                    {
                        viewTransform.RenderPosition = Vector3.MoveTowards(viewTransform.RenderPosition, collectorPosition, step);
                        isConsumed = (viewTransform.RenderPosition - collectorPosition).sqrMagnitude
                                     <= VISUAL_CONSUME_DISTANCE * VISUAL_CONSUME_DISTANCE;
                    }
                }
                // When not magnetized: leave RenderPosition as-is.
                // Initial position is set on spawn by ClientResourcePickupViewBindSystem.

                viewState.ResourceId = pickupState.ResourceId;
                viewState.Amount = pickupState.Amount;
                viewState.IsMagnetized = isMagnetized;
                viewState.IsConsumed = isConsumed;
            }
        }

        private static bool TryGetCollectorPosition(EntityGID collectorPlayer, out Vector3 position)
        {
            if (collectorPlayer.Equals(default(EntityGID)) || !collectorPlayer.TryUnpack<ClientCoreWT>(out var collector))
            {
                position = default;
                return false;
            }

            if (collector.Has<ViewTransform>())
            {
                position = collector.Read<ViewTransform>().RenderPosition;
                return true;
            }

            if (collector.Has<CharacterNetState>())
            {
                position = collector.Read<CharacterNetState>().Position;
                return true;
            }

            position = default;
            return false;
        }
    }
}
