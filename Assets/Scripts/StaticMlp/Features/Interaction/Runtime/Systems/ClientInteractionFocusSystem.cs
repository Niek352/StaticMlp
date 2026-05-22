using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Player;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Interaction
{
    /// <summary>
    /// Each frame scans all entities with <see cref="InteractableTag"/> and publishes the closest one
    /// within <see cref="FOCUS_RANGE"/> metres to the <see cref="InteractionFocus"/> world resource.
    /// Clears the focus when no valid target is nearby or the local player position is unavailable.
    /// </summary>
    public sealed class ClientInteractionFocusSystem : ISystem
    {
        private const float FOCUS_RANGE = 4f;

        public void Update()
        {
            ref var focus = ref CW.GetResource<InteractionFocus>();
            focus.Clear();

            if (!ClientLocalPlayer.TryGetPosition(out var playerPosition))
                return;

            var bestDistanceSq = FOCUS_RANGE * FOCUS_RANGE;
            var found          = false;
            var bestTarget     = default(EntityGID);
            var bestKind       = InteractableKind.None;

            foreach (var entity in CW.Query<All<InteractableTag, Interactable, ConstructionTransform>>().Entities())
            {
                var position   = entity.Read<ConstructionTransform>().Position;
                var distanceSq = (position - playerPosition).sqrMagnitude;

                if (distanceSq > bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                bestTarget     = entity.GID;
                bestKind       = entity.Read<Interactable>().Kind;
                found          = true;
            }

            if (!found)
                return;

            focus.Target = bestTarget;
            focus.Kind   = bestKind;
        }
    }
}
