using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.CombatDirector
{
    public struct ThreatBudget : IComponent
    {
        public float Current;
        public float Max;
        public float AccumulationPerSecond;
    }
}
