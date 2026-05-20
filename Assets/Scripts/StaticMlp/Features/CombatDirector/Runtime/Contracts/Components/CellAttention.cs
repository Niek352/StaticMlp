using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.CombatDirector
{
    public struct CellAttention : IComponent
    {
        public float Current;
        public float Max;
        public float DecayPerSecond;
        public float Noise;
        public float Trespass;
        public float Combat;
        public float Loot;
        public float FactionAlarm;
    }
}
