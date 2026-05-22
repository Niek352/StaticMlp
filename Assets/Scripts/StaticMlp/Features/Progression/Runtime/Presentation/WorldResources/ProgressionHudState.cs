using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Progression
{
    public struct ProgressionHudState : IResource
    {
        public bool HasRecoveredWarCache;
        public bool HasCounterattackDefended;
        public bool HasBossUnlocked;
        public byte BossPreparationTokens;
    }
}
