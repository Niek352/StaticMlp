using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    public struct LocalCombatPredictionState : IComponent
    {
        public uint LastPredictedCommandId;
        public uint LastVisualizedCommandId;
        public uint LastConfirmedCommandId;
        public uint LastRejectedCommandId;
        public float LastResolvedDamage;
    }
}
