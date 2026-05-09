using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    public sealed class CombatAutoAttackConfig : IResource
    {
        public float Radius = 8f;
        public float FireInterval = 0.4f;
        public float DamageValue = 10f;
        public float PoisonArrowRange = 10f;
        public float PoisonArrowCooldown = 0.8f;
        public float PoisonArrowDamage = 8f;
        public float PoisonDuration = 6f;
        public float PoisonTickInterval = 1f;
        public float PoisonPower = 3f;
        public float FireFlaskRange = 9f;
        public float FireFlaskCooldown = 1.2f;
        public float FireFlaskDamage = 12f;
        public float OiledDuration = 5f;
        public float OiledPower = 1f;
        public float BurningDuration = 4f;
        public float BurningTickInterval = 1f;
        public float BurningPower = 4f;
        public float BurningPoolRadius = 2.75f;
        public float BurningPoolDuration = 3f;
        public float BurningPoolTickInterval = 1f;
        public float BurningPoolDamage = 2f;
        public byte MaxChainDepth = 4;
        public float TracerLifetime = 0.08f;
        public float HighlightFadeOut = 0.12f;
        public float DamageFlashLifetime = 0.18f;
    }
}
