using FFS.Libraries.StaticEcs;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public struct GlobalRoute : IComponent
    {
        public float3 FinalTargetPosition;
        public float RepathTimer;
        public float RepathInterval;
    }
}
