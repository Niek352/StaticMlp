using FFS.Libraries.StaticEcs;
using Unity.Mathematics;

namespace StaticMlp.Features.Frontier
{
    public struct SpawnSource : IComponent
    {
        public SpawnSourceType Type;
        public float3 Position;
        public float Radius;
        public bool IsActive;
    }
}
