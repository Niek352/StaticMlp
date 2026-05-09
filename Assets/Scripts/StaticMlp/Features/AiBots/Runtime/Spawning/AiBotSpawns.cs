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
                    entity.Add<SW.Multi<AiBlackboardEntry>>();
                    AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.Hunger, hunger);
                    AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.Health01, health01);
                    AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.Fear, fear);
                    AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.EnemyDistance, 999f);
                    AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.WoodStorage01, 1f);
                    AiBlackboardAccess.SetEntity(entity, AiCoreVariableIds.Leader, leader);
                    AiBlackboardAccess.SetVector(entity, AiCoreVariableIds.LastKnownEnemyPosition, spawnPosition);
                    entity.Set(new AiTaskState
                    {
                        Task = AiTaskType.Idle,
                        ActiveTask = AiTaskType.Idle,
                        HasActiveTask = false,
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
