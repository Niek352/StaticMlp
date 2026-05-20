using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.CombatDirector
{
    [Obsolete("Temp")]
    public struct ThreatBudget : IComponent
    {
        public float Current;
        public float Max;
        public float AccumulationPerSecond;
    }
}
