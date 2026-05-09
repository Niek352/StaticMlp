using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Combat
{
    public struct PassiveAutoAttackIntent : IComponent
    {
        public EntityGID Target;
        public uint ShotSequence;
        public float LocalFireTime;
    }
}
