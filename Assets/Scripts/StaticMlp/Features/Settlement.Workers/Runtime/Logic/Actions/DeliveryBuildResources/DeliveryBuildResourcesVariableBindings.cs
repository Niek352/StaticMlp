using System;
using StaticMlp.Features.AiBots;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement.Workers
{
    public static class DeliveryBuildResourcesVariableBindings
    {
        public const ushort HAS_DELIVERY_TARGET = 1501;

        public static readonly AiBlackboardFloatBinding[] Bindings =
        {
            new(HAS_DELIVERY_TARGET, ReadHasDeliveryTarget)
        };

        public static readonly UtilityConsideration[] Considerations =
        {
            new()
            {
                VariableId = HAS_DELIVERY_TARGET,
                Curve = UtilityCurveType.Linear,
                Weight = 1f
            }
        };

        private static float ReadHasDeliveryTarget(SW.Entity entity)
        {
            return AiBlackboardAccess.TryGetEntity(entity, DeliveryBuildResourcesCollectVariables.TargetSite, out _) ? 1f : 0f;
        }
    }
}
