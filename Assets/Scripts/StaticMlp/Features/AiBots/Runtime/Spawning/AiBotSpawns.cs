using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Features.Shared;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public static class AiBotSpawns
    {
        public static EntityGID Spawn(AiBotSpawnSpec spec)
        {
            return NetworkEntitySpawner.SpawnServerEntity<AiBotNetworkEntity>(
                new NetworkPeerId(0),
                NetworkAuthority.Server,
                spec.NetworkArchetypeId,
                entity =>
                {
                    entity.Set<MonsterTag>();
                    entity.Set<AiAgentTag>();
                    entity.Set(new ServerCombatAttackState());
                    ApplyServerAiAgentState(entity, new AiAgentSpawnStateSpec(
                        spec.Position,
                        spec.Rotation,
                        spec.BehaviorId,
                        spec.MaxHealth,
                        spec.Health01,
                        spec.Hunger,
                        spec.Fear,
                        spec.Leader));
                });
        }

        public static void ApplyServerAiAgentState(SW.Entity entity, in AiAgentSpawnStateSpec spec)
        {
            var clampedHealth01 = Mathf.Clamp01(spec.Health01);
            entity.Set(new Health
            {
                Current = spec.MaxHealth * clampedHealth01,
                Max = spec.MaxHealth
            });
            entity.Set(new CharacterNetState
            {
                Position = spec.Position,
                Velocity = Vector3.zero,
                Rotation = spec.Rotation
            });
            entity.Set(new AiBrain
            {
                BehaviorId = spec.BehaviorId,
                CurrentTask = AiTaskType.Idle,
                NextDecisionTick = 0
            });
            entity.Add<SW.Multi<AiBlackboardEntry>>();
            AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.Hunger, spec.Hunger);
            AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.Health01, clampedHealth01);
            AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.Fear, spec.Fear);
            AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.EnemyDistance, 999f);
            AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.WoodStorage01, 1f);
            AiBlackboardAccess.SetEntity(entity, AiCoreVariableIds.Leader, spec.Leader);
            AiBlackboardAccess.SetVector(entity, AiCoreVariableIds.LastKnownEnemyPosition, spec.Position);
            entity.Set(new AiTaskState
            {
                Task = AiTaskType.Idle,
                ActiveTask = AiTaskType.Idle,
                HasActiveTask = false,
                Step = 0,
                ElapsedTicks = 0
            });
            entity.Set(new AiNetState
            {
                CurrentTask = AiTaskType.Idle,
                LocomotionState = 0,
                CombatState = 0
            });
        }
    }
}
