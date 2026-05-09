using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    public sealed class CombatAutoAttackConfig : IResource
    {
        public float Radius = 8f;
        public float FireInterval = 0.4f;
        public float TracerLifetime = 0.08f;
        public float HighlightFadeOut = 0.12f;
    }
}
