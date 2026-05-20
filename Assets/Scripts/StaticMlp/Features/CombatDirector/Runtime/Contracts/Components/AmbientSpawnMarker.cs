using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.CombatDirector
{
    public struct AmbientSpawnMarker : IComponent
    {
        public AmbientSpawnKind Kind;
        public int MinCount;
        public int MaxCount;
        public float CooldownSeconds;
        public float CooldownRemaining;
    }
}
