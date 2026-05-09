using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    public struct EffectChainData : IComponent
    {
        public uint RootEffectId;
        public byte Depth;
        public byte MaxDepth;
    }
}
