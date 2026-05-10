using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Effects
{
    public struct EffectChainData : IComponent
    {
        public uint RootEffectId;
        public byte Depth;
        public byte MaxDepth;
    }
}
