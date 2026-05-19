using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Features.Shared;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public sealed class AiBotFactory : NetEntityFactory<AiBotNetworkEntity>, IResource
    {
        private AiBotSpawnSpec _spec;

        public EntityGID Spawn(AiBotSpawnSpec spec)
        {
            _spec = spec;
            var entity = CreateEntity(
                new NetworkPeerId(0),
                NetworkAuthority.Server,
                spec.NetworkArchetypeId);
            Configure(entity);
            SendEntity(entity);
            return entity;
        }

        private void Configure(SW.Entity entity)
        {
            entity.Set<MonsterTag>();
            entity.Set<AiAgentTag>();
            entity.Set(new ServerCombatAttackState());
            ApplyServerAiAgentState(entity, new AiAgentSpawnStateSpec(
                ResolveTerrainPosition(_spec.Position),
                _spec.Rotation,
                _spec.BehaviorId,
                _spec.MaxHealth,
                _spec.Health01,
                _spec.Hunger,
                _spec.Fear,
                _spec.Leader));
            ApplyInitialEnemy(entity, _spec.InitialEnemy);
        }

        private static Vector3 ResolveTerrainPosition(Vector3 position)
        {
            if (!SW.HasResource<IHeightSampler>())
                return position;

            var sampler = SW.GetResource<IHeightSampler>();
            var height = sampler.SampleHeight(position.x, position.z);
            return new Vector3(position.x, height, position.z);
        }

        public void ApplyServerAiAgentState(SW.Entity entity, in AiAgentSpawnStateSpec spec)
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

        private static void ApplyInitialEnemy(SW.Entity entity, EntityGID target)
        {
            if (target.Raw == 0UL)
                return;

            if (!target.TryUnpack<ServerWT>(out var targetEntity) || !targetEntity.Has<CharacterNetState>())
                throw new System.InvalidOperationException("Initial AI enemy target must be an available server entity with CharacterNetState.");

            ref readonly var targetState = ref targetEntity.Read<CharacterNetState>();
            ref readonly var botState = ref entity.Read<CharacterNetState>();
            AiBlackboardAccess.SetEntity(entity, AiCoreVariableIds.Enemy, target);
            AiBlackboardAccess.SetVector(entity, AiCoreVariableIds.LastKnownEnemyPosition, targetState.Position);
            AiBlackboardAccess.SetFloat(
                entity,
                AiCoreVariableIds.EnemyDistance,
                (targetState.Position - botState.Position).magnitude);
        }
    }
}
