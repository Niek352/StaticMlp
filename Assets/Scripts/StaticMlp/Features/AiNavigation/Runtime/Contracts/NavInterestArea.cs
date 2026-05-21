using FFS.Libraries.StaticEcs;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public struct NavInterestArea : IComponent
    {
        public int AreaId;
        public NavInterestAreaKind Kind;
        public float3 Center;
        public float Radius;
        public float NavBuildRadius;
        public float SourceCollectRadius;
        public int Priority;
    }
}
