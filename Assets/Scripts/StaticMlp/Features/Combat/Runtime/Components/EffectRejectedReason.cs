using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    public struct EffectRejectedReason : IComponent
    {
        public EffectRejectedReasonCode Value;
    }
}
