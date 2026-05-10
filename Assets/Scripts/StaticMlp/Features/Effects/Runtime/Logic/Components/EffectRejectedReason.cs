using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Effects
{
    public struct EffectRejectedReason : IComponent
    {
        public EffectRejectedReasonCode Value;
    }
}
