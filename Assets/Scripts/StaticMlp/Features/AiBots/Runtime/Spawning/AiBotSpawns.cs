using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public static class AiBotSpawns
    {
        public static EntityGID SpawnBot(
            Vector3 spawnPosition,
            EntityGID leader,
            ushort behaviorId,
            float health01,
            float hunger,
            float fear)
        {
            return NetworkEntitySpawner.SpawnServerEntity<AiBotNetworkEntity>(
                new NetworkPeerId(0),
                NetworkAuthority.Server,
                AiBotsGameplayFeature.BOT,
                entity =>
                {
                    entity.Set<MonsterTag>();
                    entity.Set<AiAgentTag>();
                    entity.Set(new CharacterNetState
                    {
                        Position = spawnPosition,
                        Velocity = Vector3.zero,
                        Rotation = Quaternion.identity
                    });
                    entity.Set(new AiBrain
                    {
                        BehaviorId = behaviorId,
                        CurrentTask = AiTaskType.Idle,
                        DecisionCooldown = 0f
                    });
                    entity.Set(new AiBlackboard
                    {
                        Hunger = hunger,
                        Health01 = health01,
                        Fear = fear,
                        EnemyDistance = 999f,
                        WoodStorage01 = 1f,
                        Leader = leader,
                        LastKnownEnemyPosition = spawnPosition
                    });
                    entity.Set(new AiTaskState
                    {
                        Task = AiTaskType.Idle,
                        Step = 0,
                        Timer = 0f
                    });
                    entity.Set(new AiNetState
                    {
                        CurrentTask = AiTaskType.Idle,
                        LocomotionState = 0,
                        CombatState = 0
                    });
                });
        }
    }
}
