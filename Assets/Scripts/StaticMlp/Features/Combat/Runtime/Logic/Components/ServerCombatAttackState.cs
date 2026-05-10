using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    public struct ServerCombatAttackState : IComponent
    {
        public uint NextAttackTick;
        public uint LastAcceptedShotSequence;
    }
}
