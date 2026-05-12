using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Statuses
{
    public sealed class StatusesConfig : IResource
    {
        public float PoisonDuration = 6f;
        public float PoisonTickInterval = 1f;
        public float PoisonPower = 3f;
        public float OiledDuration = 5f;
        public float OiledPower = 1f;
        public float BurningDuration = 4f;
        public float BurningTickInterval = 1f;
        public float BurningPower = 4f;
        public float BurningPoolRadius = 2.75f;
        public float BurningPoolDuration = 3f;
        public float BurningPoolTickInterval = 1f;
        public float BurningPoolDamage = 2f;
    }
}
