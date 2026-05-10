using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Effects
{
    public struct DamageData : IComponent
    {
        public DamageType Type;
        public float ArmorPierce;
        public bool CanCrit;
        public bool CanTriggerOnHit;
    }
}
