using FFS.Libraries.StaticEcs;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public struct ResolvedSpawnPoint : IComponent
    {
        public EntityGID Source;
        public float3 Position;
        public int ZoneId;
        public int NavVersion;
    }
}
