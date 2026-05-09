namespace StaticMlp.Features.Combat
{
    public enum EffectRejectedReasonCode : byte
    {
        None = 0,
        MissingTarget = 1,
        MissingSource = 2,
        ChainDepthExceeded = 3,
    }
}
