using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public readonly struct AiAgentSpawnStateSpec
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly ushort BehaviorId;
        public readonly float MaxHealth;
        public readonly float Health01;
        public readonly float Hunger;
        public readonly float Fear;
        public readonly EntityGID Leader;

        public AiAgentSpawnStateSpec(
            Vector3 position,
            Quaternion rotation,
            ushort behaviorId,
            float maxHealth,
            float health01,
            float hunger,
            float fear,
            EntityGID leader)
        {
            Position = position;
            Rotation = rotation;
            BehaviorId = behaviorId;
            MaxHealth = maxHealth;
            Health01 = health01;
            Hunger = hunger;
            Fear = fear;
            Leader = leader;
        }
    }
}
