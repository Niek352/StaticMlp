using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.CombatDirector
{
    public struct PlayerThreatInputState : IComponent
    {
        public uint LastAcceptedShotSequence;
    }
}
