using FFS.Libraries.StaticEcs;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public struct RuntimeNavMeshZoneState : IComponent
    {
        public int NavVersion;
        public int RequestedNavVersion;
        public RuntimeNavMeshBuildState BuildState;
        public uint NextAllowedRebuildTick;
        public float3 LastQueuedCenter;
        public float LastQueuedRadius;
        public float LastQueuedNavBuildRadius;
        public float LastQueuedSourceCollectRadius;
        public int LastQueuedPriority;
    }
}
