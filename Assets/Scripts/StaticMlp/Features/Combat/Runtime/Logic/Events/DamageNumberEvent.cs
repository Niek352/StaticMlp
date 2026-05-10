using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Effects;
using StaticMlp.Networking;

namespace StaticMlp.Features.Combat
{
    public struct DamageNumberEvent : IEvent
    {
        public EntityGID Source;
        public EntityGID Target;
        public uint ClientCommandId;
        public float Value;
        public DamageType DamageType;
    }
}
