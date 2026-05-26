using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    public struct PassiveAutoAttackState : IComponent
    {
        public EntityGID CurrentTarget;
        public float NextFireAt;
        public uint LastShotSequence;
        public uint LastSentShotSequence;
    }
}
