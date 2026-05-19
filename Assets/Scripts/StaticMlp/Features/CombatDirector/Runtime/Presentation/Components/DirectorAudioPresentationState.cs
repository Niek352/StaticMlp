using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.CombatDirector
{
    public struct DirectorAudioPresentationState : IComponent
    {
        public DirectorPhase LastPhase;
        public bool IsInitialized;
    }
}
