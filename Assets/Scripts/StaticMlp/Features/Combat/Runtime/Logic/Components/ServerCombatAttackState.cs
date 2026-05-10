using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    public struct ServerCombatAttackState : IComponent
    {
        public float NextAttackAt;
        public uint LastAcceptedShotSequence;
    }
}
