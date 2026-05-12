using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    public sealed class CombatConfig : IResource
    {
        public float Radius = 8f;
        public float FireInterval = 0.4f;
        public float DamageValue = 10f;
        public float PoisonArrowRange = 10f;
        public float PoisonArrowCooldown = 0.8f;
        public float PoisonArrowDamage = 8f;
        public float FireFlaskRange = 9f;
        public float FireFlaskCooldown = 1.2f;
        public float FireFlaskDamage = 12f;
        public byte MaxChainDepth = 4;
    }
}
