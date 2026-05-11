using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Settlement.Workers
{
    public readonly struct SettlementWorkerRuntimeProfile
    {
        public readonly WorkerRoleId RoleId;
        public readonly ushort NetworkArchetypeId;
        public readonly ushort BehaviorId;
        public readonly float MaxHealth;

        public SettlementWorkerRuntimeProfile(
            WorkerRoleId roleId,
            ushort networkArchetypeId,
            ushort behaviorId,
            float maxHealth)
        {
            RoleId = roleId;
            NetworkArchetypeId = networkArchetypeId;
            BehaviorId = behaviorId;
            MaxHealth = maxHealth;
        }
    }
}
