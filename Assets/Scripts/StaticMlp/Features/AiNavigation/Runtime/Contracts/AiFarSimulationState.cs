using FFS.Libraries.StaticEcs;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public struct AiFarSimulationState : IComponent
    {
        public float3 LogicalPosition;
        public float3 TargetPosition;
        public float NextSimulationTime;
        public int GlobalRouteNodeId;
    }
}
