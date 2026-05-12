using StaticMlp.Features.Settlement;
using UnityEngine;

namespace StaticMlp.Features.Settlement.Workers
{
    public readonly struct SettlementWorkerSpawnSpec
    {
        public readonly SettlementAnchorId HomeAnchorId;
        public readonly WorkerRoleId RoleId;
        public readonly ushort NetworkArchetypeId;
        public readonly ushort BehaviorId;
        public readonly float MaxHealth;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public SettlementWorkerSpawnSpec(
            SettlementAnchorId homeAnchorId,
            WorkerRoleId roleId,
            ushort networkArchetypeId,
            ushort behaviorId,
            float maxHealth,
            Vector3 position,
            Quaternion rotation)
        {
            HomeAnchorId = homeAnchorId;
            RoleId = roleId;
            NetworkArchetypeId = networkArchetypeId;
            BehaviorId = behaviorId;
            MaxHealth = maxHealth;
            Position = position;
            Rotation = rotation;
        }
    }
}
