using StaticMlp.Networking;

namespace StaticMlp.Features.Effects
{
    public static class EffectLifecycle
    {
        public static void MarkProcessed(SW.Entity effect)
        {
            effect.Set<EffectProcessedTag>();
        }

        public static void MarkRejected(SW.Entity effect)
        {
            effect.Set<EffectRejectedTag>();
        }
    }
}
