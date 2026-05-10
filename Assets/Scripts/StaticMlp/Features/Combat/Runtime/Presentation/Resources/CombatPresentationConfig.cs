using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    public sealed class CombatPresentationConfig : IResource
    {
        public float TracerLifetime = 0.08f;
        public float HighlightFadeOut = 0.12f;
        public float DamageFlashLifetime = 0.18f;
    }
}
