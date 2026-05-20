using FFS.Libraries.StaticEcs;
using Unity.Mathematics;

namespace StaticMlp.Features.CombatDirector
{
    public struct SpawnSource : IComponent
    {
        public SpawnSourceType Type;
        public SpawnSourceKind Kind;
        public float3 Position;
        public float Radius;
        public int FactionId;
        public int BiomeId;
        public bool IsActive;
        public bool AllowsAmbient;
        public bool AllowsEscalation;
        public bool AllowsPressureEvent;
    }
}
