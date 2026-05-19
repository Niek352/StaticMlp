using FFS.Libraries.StaticEcs;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public readonly struct LocalNavigationHandoffRequestEvent : IEvent
    {
        public readonly EntityGID Entity;
        public readonly float3 LogicalPosition;
        public readonly float3 Destination;
        public readonly float StopDistance;

        public LocalNavigationHandoffRequestEvent(
            EntityGID entity,
            float3 logicalPosition,
            float3 destination,
            float stopDistance)
        {
            Entity = entity;
            LogicalPosition = logicalPosition;
            Destination = destination;
            StopDistance = stopDistance;
        }
    }
}
