using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    public struct DamageData : IComponent
    {
        public DamageType Type;
        public float ArmorPierce;
        public bool CanCrit;
        public bool CanTriggerOnHit;
    }
}
