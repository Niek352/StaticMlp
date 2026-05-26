using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Combat
{
    public struct PassiveAutoAttackState : IComponent
    {
        public CombatTargetRef CurrentTarget;
        public float NextFireAt;
        public uint LastShotSequence;
        public uint LastSentShotSequence;
    }
}
